using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum CardType
{
    NONE = 0,
    HEAL_PLR, // drink rum
    DMG_BOSS, // 
    DMG_BOSS_BIG, // 
    SHUF, // 
    GIVE_BLOONS, // 

    //chris cards
    PARRY, // Parry
    DMG_BUFF, // 
    DOUBLE_DMG, // 
    ENDURE, // Endure
    FIREBALL, // Fireball
    END
}

[System.Serializable]
public class Card
{
    public CardType type = CardType.NONE;
    public int cost = 0;
    public int action_value = 0;
    public int frequency = 100;
    public Texture2D texture;

    public Card(CardType _type, int _cost, int _action_value, int _frequency, Texture2D _texture)
    {
        this.type = _type;
        this.cost = _cost;
        this.action_value = _action_value;
        this.frequency = _frequency;
        this.texture = _texture;
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
    [SerializeField] private int bloon_regen_interval = 3;
    [SerializeField] private int bloon_regen_amount = 1;

    [SerializeField] private int max_hand = 5;
    private Card[] hand;
    private Card[] all_cards;
    private CardType last_received_card_type;

    //values for damage calculation
    private int dmg_mult = 0;
    private int dmg_flat = 0;
    private int dmg_flat_duration = 0;

    //boolean to check endure status
    private bool endure_active = false;

    [SerializeField] private GameObject bloons_spawn;
    [SerializeField] private GameObject bloon_prefab;
    private GameObject[] bloons_pool;

    private void Awake()
    {
        all_cards = new Card[Convert.ToInt32(CardType.END)];
        all_cards[Convert.ToInt32(CardType.HEAL_PLR)] = new Card(CardType.HEAL_PLR, 3, 10, 100, Resources.Load<Texture2D>("Cards/c1"));
        all_cards[Convert.ToInt32(CardType.DMG_BOSS)] = new Card(CardType.DMG_BOSS, 2, 5, 100, Resources.Load<Texture2D>("Cards/c2"));
        all_cards[Convert.ToInt32(CardType.DMG_BOSS_BIG)] = new Card(CardType.DMG_BOSS_BIG, 3, 30, 70, Resources.Load<Texture2D>("Cards/c3"));
        all_cards[Convert.ToInt32(CardType.SHUF)] = new Card(CardType.SHUF, 3, 0, 30, Resources.Load<Texture2D>("Cards/c4"));
        all_cards[Convert.ToInt32(CardType.GIVE_BLOONS)] = new Card(CardType.GIVE_BLOONS, 2, 5, 30, Resources.Load<Texture2D>("Cards/c5"));

        //chris cards
        all_cards[Convert.ToInt32(CardType.PARRY)] = new Card(CardType.PARRY, 5, 0, 30, Resources.Load<Texture2D>("Cards/c6"));
        all_cards[Convert.ToInt32(CardType.DMG_BUFF)] = new Card(CardType.DMG_BUFF, 4, 4, 60, Resources.Load<Texture2D>("Cards/c7"));
        all_cards[Convert.ToInt32(CardType.DOUBLE_DMG)] = new Card(CardType.DOUBLE_DMG, 4, 2, 50, Resources.Load<Texture2D>("Cards/c8"));
        all_cards[Convert.ToInt32(CardType.ENDURE)] = new Card(CardType.ENDURE, 3, 1, 30, Resources.Load<Texture2D>("Cards/c9"));
        all_cards[Convert.ToInt32(CardType.FIREBALL)] = new Card(CardType.FIREBALL, 10, 50, 10, Resources.Load<Texture2D>("Cards/c10"));
        
        ShuffleCards();
    }

    void Start()
    {
        health = max_health;
        bloons = 0;
        
        bloons_pool = new GameObject[max_bloons];
        for(int i = 0; i < max_bloons; i++)
        {
            bloons_pool[i] = Instantiate(bloon_prefab, transform.position,  Quaternion.AngleAxis(0.0f, Vector3.up));
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        int second = Mathf.RoundToInt(timer);

        if (second % bloon_regen_interval == 0 && bloons < max_bloons) Earn(bloon_regen_amount);
    }

    public void ChooseCard(Card card)
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
                dmg_mult = 0;
                //however if damage flat buff is in play, 
                //use a counter to count down the duration of the buff based on the number of times damage has been done
                dmg_flat_duration--;
                if (dmg_flat_duration == 0)
                    dmg_flat = 0; //remove the flat dmg buff
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
                dmg_flat = card.action_value;
                dmg_flat_duration = 3;
                break;
            case CardType.DOUBLE_DMG:
                dmg_mult = card.action_value;
                break;
            case CardType.ENDURE:
                endure_active = true;
                break;
        }
        Spend(card.cost);
        Manage();
    }
    
    public Card NextCard()
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
            last_received_card_type = all_cards[i].type;
            if (roll < cumulative) return all_cards[i];
        }

        last_received_card_type = all_cards[1].type;
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
        if (endure_active && health <= 0)
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

        for (int i = 0; i < bloons; i++)
        {
            float offset = i * 0.05f;
            bloons_pool[i].transform.position = new Vector3(bloons_spawn.transform.position.x + offset, bloons_spawn.transform.position.y + offset, bloons_spawn.transform.position.z);
            bloons_pool[i].transform.eulerAngles = new Vector3(107, 41, 0);
        }
    }

    private void Manage()
    {
        health = Mathf.Clamp(health, 0, max_health);
        bloons = Mathf.Clamp(bloons, 0, max_bloons);
    }

    private void DisplayBloons()
    {
        
    }

    public Card[] GetHand()
    {
        return hand; 
    }
}
