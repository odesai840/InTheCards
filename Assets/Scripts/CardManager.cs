using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;


public class CardManager : MonoBehaviour   //// Controls the spawning and use of the cards
{

    public GameObject cardPreFab;

    [SerializeField] private Player player;

    [SerializeField] GameObject cardSpawnPoint;  // where cards fly in from
    [SerializeField] GameObject handCenter;      //Center of hands in card

    public float cardSpacing = 1.5f;

    private readonly List<GameObject> handCards = new List<GameObject>();
    public List<GameObject> GetHandCards() { return handCards; }

    void Start()
    {
        player.ShuffleCards(true);
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            FlipNextCard();

        TrackMouse();
    }

    private CardController hoveredCard = null;

    private void TrackMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        CardController card = null;

        if (Physics.Raycast(ray, out RaycastHit hit))
            card = hit.collider.GetComponent<CardController>();

        if (card != hoveredCard)
        {
            hoveredCard?.Unhover();
            hoveredCard = card;
            hoveredCard?.Hover();
        }

        if (hoveredCard != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!hoveredCard.used && player.CanAffordCard(hoveredCard.card))
            {
                hoveredCard.used = true;
                Debug.Log(hoveredCard.card.type);
                player.ChooseCard(hoveredCard.card);
                GameObject cardObj = hoveredCard.gameObject;
                Vector3 cardPos = hoveredCard.transform.position;
                hoveredCard = null;
                StartCoroutine(PlayCard(cardObj, cardPos, .4f));
                FlipNextCard();
                RepositionAllCards();
            }
        }
    }


    public void FlipNextCard()
    {
        Quaternion spawnRot = Quaternion.Euler(0f, 0f, 90f);
        GameObject newCardObject = Instantiate(cardPreFab, transform.position, spawnRot);

        Card newCard = player.NextCard();
        newCardObject.GetComponent<CardController>().card = newCard;
        newCardObject.GetComponent<CardController>().used = false;
        newCardObject.GetComponent<Renderer>().material.mainTexture = newCard.texture;

        handCards.Add(newCardObject);
        RepositionAllCards();

        Vector3 startPos = transform.position;
        Vector3 endPos = handCenter.transform.position;

        StartCoroutine(MoveCard(newCardObject, startPos, endPos, .5f));
    }


    public void RepositionAllCards()
    {

        float currentCardSpacing = cardSpacing;

        int count = handCards.Count;

        if (count >= 8)
        {
            currentCardSpacing = cardSpacing * .75f;
        } else
        {
            currentCardSpacing = cardSpacing;
        }

        float totalWidth = (count - 1) * currentCardSpacing;

        for (int i = 0; i < count; i++)
        {
            
            float xOffset = i * currentCardSpacing - totalWidth / 2f;
            Vector3 targetPos = handCenter.transform.position + Camera.main.transform.right * xOffset;

            CardController cc = handCards[i].GetComponent<CardController>();
            if (cc != null)
                cc.PlaceInHand(targetPos);
        }
    }


    IEnumerator MoveCard(GameObject newCard, Vector3 startPos, Vector3 endPos, float duration)
    {
        float elapsed = 0f;
        Quaternion startRot = newCard.transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, 90f, 0f);

        while (elapsed < duration)
        {
            if (newCard == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            newCard.transform.position = Vector3.Lerp(startPos, endPos, t);
            newCard.transform.rotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }

        if (newCard == null) yield break;
        newCard.transform.position = endPos;
        newCard.transform.rotation = endRot;

        StartCoroutine(FlipCard(newCard, .3f));
    }

    IEnumerator FlipCard(GameObject newCard, float duration)
    {
        float elapsed = 0f;
        Quaternion startRot = newCard.transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, 0f, 180f);

        while (elapsed < duration)
        {
            if (newCard == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            newCard.transform.rotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }

        if (newCard == null) yield break;
        newCard.transform.rotation = endRot;

        
        CardController cc = newCard.GetComponent<CardController>();
        if (cc != null)
            cc.isInHand = true;
    }


    public IEnumerator PlayCard(GameObject card, Vector3 startPos, float duration)
    {
        float elapsed = 0f;
        Quaternion startRot = card.transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, -90f, 0f); 

        Vector3 endPos = new Vector3(handCenter.transform.position.x + 1.5f, handCenter.transform.position.y, startPos.z);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            card.transform.rotation = Quaternion.Lerp(startRot, endRot, t);
            card.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        if (elapsed >= duration) 
        {
            RemoveCard(card);
        }
        
    }

    public void RemoveCard(GameObject card)
    {
        handCards.Remove(card);
        RepositionAllCards();
        Destroy(card);
    }


    public int GetCardsInHand()
    {
        return handCards.Count;
    }
}
