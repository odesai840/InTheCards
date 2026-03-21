using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum ActionType
{
    NONE = 0,
    DMG_PLR,
    DMG_PLR_BIG,
    DMG_PLR_BIG_BIG,
    SHUF_PLR,
    HEAL_BOSS,
    END 
}

[System.Serializable]
public class Action
{
    public ActionType type = ActionType.NONE;
    public int cost = 0;
    public int action_value = 0;
    public int frequency = 100;
    public int since_last_use = 0;

    public Action(ActionType t, int c, int av, int f)
    {
        this.type = t;
        this.cost = c;
        this.action_value = av;
        this.frequency = f;
    }
}

public class Boss : MonoBehaviour
{
    private float timer = 0.0f;

    [SerializeField] private Player player;
    
    [SerializeField] private int max_health = 1000; // 777 later
    private int health;
    
    [SerializeField] private int max_chips = 10;
    private int chips;
    [SerializeField] private int chip_regen_interval = 3;
    [SerializeField] private int chip_regen_amount = 1;

    private Action[] all_actions;
    private ActionType last_action_type = ActionType.NONE;
    
    void Start()
    {
        all_actions = new Action[Convert.ToInt32(ActionType.END)];
        all_actions[Convert.ToInt32(ActionType.DMG_PLR)] = new Action(ActionType.DMG_PLR, 1, 5, 100);
        all_actions[Convert.ToInt32(ActionType.DMG_PLR_BIG)] = new Action(ActionType.DMG_PLR_BIG, 4, 15, 60);
        all_actions[Convert.ToInt32(ActionType.DMG_PLR_BIG_BIG)] = new Action(ActionType.DMG_PLR_BIG_BIG, 10, 50, 10);
        all_actions[Convert.ToInt32(ActionType.HEAL_BOSS)] = new Action(ActionType.HEAL_BOSS, 1, 10, 100);
        all_actions[Convert.ToInt32(ActionType.SHUF_PLR)] = new Action(ActionType.SHUF_PLR, 8, 0, 20);
    }

    void Update()
    {
        timer += Time.deltaTime;
        int second = Mathf.RoundToInt(timer);

        if (second % chip_regen_interval == 0) Earn(chip_regen_amount);
    }

    // Choose Action
    public Action Wethepeopleoftheunitedstatesinordertoformamoreperfectunionestablishjusticeinsuredomestictranquilityprovidforthecommondefencepromotethegeneralwelfareadnsecuretheblessingsoflibertytoourselvesandourposteritydoordainandestablishthisconstitutionfortheunitedstatesofamerica
        (Action action)
    {
        for (int i = 0; i < all_actions.Length; i++)
        {
            all_actions[i].since_last_use++;
        }
        action.since_last_use = 0;
        
        // perform current action
        switch (action.type)
        {
            default:
            case ActionType.NONE:
            case ActionType.END:
                break;
            case ActionType.DMG_PLR:
            case ActionType.DMG_PLR_BIG: 
            case  ActionType.DMG_PLR_BIG_BIG:
                player.Damage(action.action_value);
                break;
            case ActionType.HEAL_BOSS:
                Heal(action.action_value);
                break;
            case ActionType.SHUF_PLR:
                player.ShuffleCards();
                break;
        }
        
        // ready next action
        Action random_action = NextAction();
        last_action_type = random_action.type;
        
        Manage();
        return random_action;
    }

    private Action NextAction()
    {
        int total = 0;
        for (int i = 1; i < all_actions.Length; i++)
        {
            if (all_actions[i] == null || all_actions[i].type == ActionType.NONE || all_actions[i].type == ActionType.END) continue;
            
            total += Convert.ToInt32(CalculateActionWeight(all_actions[i]));
        }

        int roll = Random.Range(0, total);
        int cumulative = 0;
        for (int i = 1; i < all_actions.Length; i++)
        {
            if (all_actions[i] == null || all_actions[i].type == ActionType.NONE || all_actions[i].type == ActionType.END) continue;
            cumulative += Convert.ToInt32(CalculateActionWeight(all_actions[i]));
            if (roll < cumulative) return all_actions[i];
        }
        return all_actions[1];
    }

    private float CalculateActionWeight(Action action)
    {
        float weight = action.frequency + action.since_last_use;
        if (action.type == ActionType.HEAL_BOSS && (float)health / max_health < 0.5f)
            weight += (int)(50.0f * Mathf.Pow(2, (1 - ((float)health/max_health)) * 4.0f));
        if (action.type == ActionType.DMG_PLR_BIG_BIG && action.since_last_use > 10)
            weight += 50;
            
        if (action.cost > chips || action.type == last_action_type) weight = 0.0f;
        return weight;
    }
    
    public void Damage(int amount)
    {
        health -= amount;
    }
    public void Heal(int amount)
    {
        health += amount;
    }
    public void Spend(int amount)
    {
        chips -=  amount;
    }
    public void Earn(int amount)
    {
        chips += amount;
    }
    
    private void Manage()
    {
        health = Mathf.Clamp(health, 0, max_health);
        chips = Mathf.Clamp(chips, 0, max_chips);
    }
}
