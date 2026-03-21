using UnityEngine;



public class CardController : MonoBehaviour   ///// Controls what the physical cards do when they are on the screen
{
    private GameObject CameraReferencePoint;

    private Vector3 targetPosition;
    public bool isInHand = false;
    public float moveSpeed = 5f;


    void Start()
    {
        CameraReferencePoint = GameObject.Find("CameraReferencePoint");
    }

    void Update()
    {
        TurnTowardsCamera();

        if (isInHand)
            PositionCard();
    }


    public void SetTargetPosition(Vector3 pos)
    {
        targetPosition = pos;
    }

    public void PlaceInHand(Vector3 pos)
    {
        targetPosition = pos;
        isInHand = true;
    }


    private void TurnTowardsCamera()
    {
        Vector3 lookLocation = CameraReferencePoint.transform.position;
        transform.LookAt(lookLocation);
        transform.Rotate(-90f, 90f, 180f);
    }

    private void PositionCard()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }
}
