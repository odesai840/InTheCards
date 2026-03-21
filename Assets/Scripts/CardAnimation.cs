using System.Collections;
using UnityEngine;




public class CardAnimation : MonoBehaviour
{


    public GameObject CameraReferencePoint;


    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TurnTowardsCamera();

        
    }



    private void TurnTowardsCamera()
    {
        Vector3 pos = transform.position;
        Vector3 currentRotation = transform.eulerAngles;
        
        Vector3 lookLocation = CameraReferencePoint.transform.position;

        transform.LookAt(lookLocation);
        transform.Rotate(-90f, 90f, 180f);
    }


    
}
