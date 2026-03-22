using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum CardType
{
    NONE = 0,
    HEAL_PLR,
    DMG_BOSS,
    DMG_BOSS_BIG,
    SHUF,
    GIVE_BLOONS,

    //chris cards
    PARRY,
    DMG_BUFF,
    DOUBLE_DMG,
    ENDURE,
    FIREBALL,
    END
}

[System.Serializable]
public class Card
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
    private float timer = 0.0f;

    [SerializeField] private Boss boss;

    [SerializeField] private int max_health = 100;
    private int health;

    [SerializeField] private int max_bloons = 10;
    private int bloons;
    [SerializeField] private int bloon_regen_interval = 2;
    [SerializeField] private int bloon_regen_amount = 1;

    [SerializeField] private int max_hand = 5;
    private Card[] hand;
    private Card[] all_cards;
    private CardType last_received_card_type;

    //values for damage calculation
    private float dmg_mult = 0.0f;
    private int dmg_flat = 0;
    private int dmg_flat_duration = 0;

    //boolean to check endure status
    private boolean endure_active = false;

    void Start()
    {
        health = max_health;

        all_cards = new Card[Convert.ToInt32(CardType.END)];
        all_cards[Convert.ToInt32(CardType.HEAL_PLR)] = new Card(CardType.HEAL_PLR, 3, 10, 100);
        all_cards[Convert.ToInt32(CardType.DMG_BOSS)] = new Card(CardType.DMG_BOSS, 2, 5, 100);
        all_cards[Convert.ToInt32(CardType.DMG_BOSS_BIG)] = new Card(CardType.DMG_BOSS_BIG, 3, 30, 70);
        all_cards[Convert.ToInt32(CardType.SHUF)] = new Card(CardType.SHUF, 3, 0, 30);
        all_cards[Convert.ToInt32(CardType.GIVE_BLOONS)] = new Card(CardType.GIVE_BLOONS, 2, 5, 30);

        //chris cards
        all_cards[Convert.ToInt32(CardType.PARRY)] = new Card(CardType.PARRY, 5, 0, 30);
        all_cards[Convert.ToInt32(CardType.DMG_BUFF)] = new Card(CardType.DMG_BUFF, 4, 4, 60);
        all_cards[Convert.ToInt32(CardType.DOUBLE_DMG)] = new Card(CardType.DOUBLE_DMG, 4, 2, 50);
        all_cards[Convert.ToInt32(CardType.ENDURE)] = new Card(CardType.ENDURE, 3, 1, 30);
        all_cards[Convert.ToInt32(CardType.FIREBALL)] = new Card(CardType.FIREBALL, 10, 50, 10);
        
        ShuffleCards();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        int second = Mathf.RoundToInt(timer);

        if (second % bloon_regen_interval == 0) Earn(bloon_regen_amount);
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
            case CardType.FIREBALL:
                //damage formula
                boss.Damage((card.action_value + dmg_flat) * dmg_mult);
                //after damage is done, the dmg multiplier is set to 0
                dmg_mult = 0.0f;
                //however if damage flat buff is in play, 
                //use a counter to count down the duration of the buff based on the number of times damage has been done
                dmg_flat_duration--;
                if (dmg_flat_duration == 0)
                {
                    //remove the flat dmg buff
                    dmg_flat = 0;
                }
                break;
            case CardType.SHUF:
                ShuffleCards();
                break;
            case CardType.GIVE_BLOONS:
                Earn(card.action_value);
                break;
            //chris cards
            case CardType.PARRY:
                //implement boss parry
                break;
            case CardType.DMG_BUFF:
                //set damage flat buff number to the action value assigned to the number and set the buff duration
                dmg_flat = action.action_value;
                dmg_flat_duration = 3;
                break;
            case CardType.DOUBLE_DMG:
                dmg_mult = action.action_value;
                break;
            case CardType.ENDURE:
                endure_active = true;
                break;
        }
        Spend(card.cost);
        cards_played++;


        Card random_card = NextCard();
        last_received_card_type = random_card.type;
        
        Manage();
        return random_card;
    }
    
    private Card NextCard()
    {
        int total = 0;
        for (int i = 1; i < all_cards.Length; i++)
        {
            if (all_cards[i] == null || all_cards[i].type == CardType.NONE || all_cards[i].type == CardType.END) continue;
            total += all_cards[i].frequency;
        }

        int roll = Random.Range(0, total);
        int cumulative = 0;
        for (int i = 1; i < all_cards.Length; i++)
        {
            if (all_cards[i] == null || all_cards[i].type == CardType.NONE || all_cards[i].type == CardType.END) continue;
            cumulative += all_cards[i].frequency;
            if (roll < cumulative) return all_cards[i];
        }
        return all_cards[1];
    }

    public void ShuffleCards()
    {
        Card[] random_hand =  new Card[max_hand];
        for (int i = 0; i < max_hand; i++)
        {
            random_hand[i] = NextCard();
            last_received_card_type = random_hand[i].type;
        }
        hand = random_hand;
    }
    
    public void Damage(int amount)
    {
        health -= amount;
        if ((endure_active == true) && health <= 0) 
        {
            health = 1;
            endure_active = false;
        }
    }
    public void Heal(int amount)
    {
        health += amount;
    }
    public void Spend(int amount)
    {
        bloons -=  amount;
    }
    public void Earn(int amount)
    {
        bloons += amount;
    }

    private void Manage()
    {
        health = Mathf.Clamp(health, 0, max_health);
        bloons = Mathf.Clamp(bloons, 0, max_bloons);
    }

    public Card[] GetHand()
    {
        return hand; 
    }
}
