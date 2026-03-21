using UnityEngine;

public class Boss : MonoBehaviour
{
    [SerializeField] private int max_health;
    private int health;

    public void Damage(int damage)
    {
        health -= damage;
    }
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
