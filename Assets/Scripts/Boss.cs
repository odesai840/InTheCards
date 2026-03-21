using UnityEngine;

public class Boss : MonoBehaviour
{
    [SerializeField] private int max_health;
    private int health;

    public void Damage(int amount)
    {
        health -= amount;
    }
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
