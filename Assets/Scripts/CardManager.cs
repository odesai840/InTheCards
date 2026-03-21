using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;



public class CardManager : MonoBehaviour   //// Controls the spawning and use of the cards
{

    public GameObject cardPreFab;

    [SerializeField] GameObject cardSpawnPoint;  // where cards fly in from
    [SerializeField] GameObject handCenter;      //Center of hands in card

    public float cardSpacing = 1.5f;

    private readonly List<GameObject> handCards = new List<GameObject>();


    void Start()
    {

        for (int i = 0; i < 5; i++)
        {
            FlipNextCard();
        }
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            FlipNextCard();
        }
    }


    private void FlipNextCard()
    {
        Quaternion spawnRot = Quaternion.Euler(0f, 0f, 90f);
        GameObject newCard = Instantiate(cardPreFab, transform.position, spawnRot);

        handCards.Add(newCard);
        RepositionAllCards();

        Vector3 startPos = transform.position;
        Vector3 endPos = handCenter.transform.position;

        StartCoroutine(MoveCard(newCard, startPos, endPos, .5f));
    }


    private void RepositionAllCards()
    {
        int count = handCards.Count;
        float totalWidth = (count - 1) * cardSpacing;

        for (int i = 0; i < count; i++)
        {
            float xOffset = i * cardSpacing - totalWidth / 2f;
            Vector3 targetPos = handCenter.transform.position + new Vector3(xOffset, 0f, 0f);

            CardController cc = handCards[i].GetComponent<CardController>();
            if (cc != null)
                cc.SetTargetPosition(targetPos);
        }
    }


    IEnumerator MoveCard(GameObject newCard, Vector3 startPos, Vector3 endPos, float duration)
    {
        float elapsed = 0f;
        Quaternion startRot = newCard.transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, 90f, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            newCard.transform.position = Vector3.Lerp(startPos, endPos, t);
            newCard.transform.rotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }

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
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            newCard.transform.rotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }

        newCard.transform.rotation = endRot;

        
        CardController cc = newCard.GetComponent<CardController>();
        if (cc != null)
            cc.isInHand = true;
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
