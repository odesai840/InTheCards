using UnityEngine;

public enum CardType
{
    NONE = 0,
    HEAL_PLR = 1,
    DMG_BOSS = 2,
}

[System.Serializable] public class Card
{
    public CardType type = CardType.NONE;
    public int cost = 0;
    public int action_value = 0;
}

public class Player : MonoBehaviour
{
    [SerializeField] private int max_health;
    private int health;

    [SerializeField] private Boss boss;

    private Card[] hand;
    private Card[] cards;
    
    void Start()
    {
        health = max_health;
    }

    void Update()
    {
        
    }

    void ChooseCard(Card card)
    {
        switch (card.type)
        {
            case CardType.NONE:
                break;
            case CardType.HEAL_PLR:
                health += card.action_value;
                break;
            case CardType.DMG_BOSS:
                boss.Damage(card.action_value);
                break;
        }
    }
}
