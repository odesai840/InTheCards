using UnityEngine;

public class WaterMovement : MonoBehaviour
{
    [Header("Tilt")]
    public float tiltAmount = 5f;       
    public float tiltSpeed = 0.4f;       

    private Quaternion startRotation;

    void Start()
    {
        startRotation = transform.rotation;
    }

    void Update()
    {
        float t = Time.time;

        float tiltX = Mathf.Sin(t * tiltSpeed) * tiltAmount;
        float tiltZ = Mathf.Sin(t * tiltSpeed * 0.7f + 1.3f) * tiltAmount;
        transform.rotation = startRotation * Quaternion.Euler(tiltX, 0f, tiltZ);
    }
}
