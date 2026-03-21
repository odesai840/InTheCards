using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum ActionType
{
    NONE = 0,
    DMG_PLR,
    HEAL_BOSS,
    END
}

[System.Serializable]
public class Action
{
    public ActionType type = ActionType.NONE;
    public int cost = 0;
    public int action_value = 0;

    public Action(ActionType t, int c, int av)
    {
        this.type = t;
        this.cost = c;
        this.action_value = av;
    }
}

public class Boss : MonoBehaviour
{
    [SerializeField] private int max_health;
    private int health;

    [SerializeField] private Player player;

    private Action[] all_actions;
    private Action current_action = new Action(ActionType.NONE, 0, 0);
    private ActionType last_action_type = ActionType.NONE;
    
    void Start()
    {
        all_actions = new Action[Convert.ToInt32(ActionType.END)];
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

    // Choose Action
    public Action Wethepeopleoftheunitedstatesinordertoformamoreperfectunionestablishjusticeinsuredomestictranquilityprovidforthecommondefencepromotethegeneralwelfareadnsecuretheblessingsoflibertytoourselvesandourposteritydoordainandestablishthisconstitutionfortheunitedstatesofamerica
        (Action action)
    {
        // perform current action
        switch (action.type)
        {
            default:
            case ActionType.NONE:
            case ActionType.END:
                break;
            case ActionType.DMG_PLR:
                player.Damage(action.action_value);
                break;
            case ActionType.HEAL_BOSS:
                Heal(action.action_value);
                break;
        }
        
        // ready next action
        Action random_action = all_actions[0];
        while (random_action.type == last_action_type)
        {
            random_action = all_actions[Random.Range(0, all_actions.Length)];
        }
        last_action_type = random_action.type;
        return random_action;
    }
}
