using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum CardType
{
    NONE = 0,
    HEAL_PLR,
    DMG_BOSS,
    DMG_BOSS_BIG,
    SHUFFLE,
    END
}

[System.Serializable] public class Card
{
    public CardType type = CardType.NONE;
    public int cost = 0;
    public int action_value = 0;
    public int frequency = 100;

    public Card(CardType t, int c, int av, int f)
    {
        this.type = t;
        this.cost = c;
        this.action_value = av;
        this.frequency = f;
    }
}

public class Player : MonoBehaviour
{
    [SerializeField] private int max_health;
    private int health;

    [SerializeField] private Boss boss;

    [SerializeField] private int max_hand = 5;
    private Card[] hand;
    private Card[] all_cards;
    
    void Start()
    {
        health = max_health;

        all_cards[Convert.ToInt32(CardType.HEAL_PLR)] = new Card(CardType.HEAL_PLR, 3, 10, 100);
        all_cards[Convert.ToInt32(CardType.DMG_BOSS)] = new Card(CardType.DMG_BOSS, 2, 5, 100);
        all_cards[Convert.ToInt32(CardType.DMG_BOSS_BIG)] = new Card(CardType.DMG_BOSS_BIG, 3, 30, 100);
        all_cards[Convert.ToInt32(CardType.SHUFFLE)] = new Card(CardType.SHUFFLE, 3, 0, 100);
    }

    void Update()
    {
        
    }

    public void Damage(int amount)
    {
        health -= amount;
        health = Mathf.Clamp(health, 0, max_health);
    }

    public void Heal(int amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0, max_health);
    }

    public void ShuffleCards()
    {
        Card[] random_hand =  new Card[all_cards.Length];
        for (int i = 0; i < max_hand; i++)
            random_hand[i] = all_cards[i];
        hand = random_hand;
    }

    public Card ChooseCard(Card card)
    {
        switch (card.type)
        {
            default:
            case CardType.NONE:
            case CardType.END:
                break;
            case CardType.HEAL_PLR:
                Heal(card.action_value);
                break;
            case CardType.DMG_BOSS:
            case CardType.DMG_BOSS_BIG:
                boss.Damage(card.action_value);
                break;
            case CardType.SHUFFLE:
                ShuffleCards();
                break;
        }

        int random_card = Random.Range(0, all_cards.Length - 1);
        return all_cards[random_card];
    }
}
