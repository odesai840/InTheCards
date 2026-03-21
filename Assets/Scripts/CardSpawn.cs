using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;







public class CardSpawn : MonoBehaviour
{

    public GameObject cardPreFab;
    public GameObject CameraReferencePoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            FlipNextCard();
        }

        if (Keyboard.current.shiftKey.wasPressedThisFrame)
        {
            Quaternion spawnRot = Quaternion.Euler(0f, 0f, 90f);
            GameObject card = Instantiate(cardPreFab, transform.position, spawnRot);
        }
    }

    
    public void FlipNextCard()
    {
        Quaternion spawnRot = Quaternion.Euler(0f, 0f, 90f);
        GameObject newCard = Instantiate(cardPreFab, transform.position, spawnRot);
        Vector3 cardSpawn = transform.position;
        Vector3 endPos = CameraReferencePoint.transform.position; /// REPLACE THIS FOR REAL POS


        StartCoroutine(MoveCard(newCard, cardSpawn, endPos, .5f));
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
    }
    
}
