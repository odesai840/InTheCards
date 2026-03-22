using System;
using System.Collections;

using UnityEngine;
using Random = UnityEngine.Random;

public enum ActionYeaType
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
public class ActionYea
{
    public ActionYeaType type = ActionYeaType.NONE;
    public int cost = 0;
    public int action_value = 0;
    public int frequency = 100;
    public int since_last_use = 0;

    public ActionYea(ActionYeaType t, int c, int av, int f)
    {
        this.type = t;
        this.cost = c;
        this.action_value = av;
        this.frequency = f;
    }
}

public class BossTucker : MonoBehaviour
{
    private float timer = 0.0f;

    [SerializeField] private Player player;
    
    [SerializeField] private int max_health = 1000; // 777 later
    private int health;
    
    [SerializeField] private int max_chips = 10;
    private int chips;
    [SerializeField] private int chip_regen_interval = 3;
    [SerializeField] private int chip_regen_amount = 1;

    private ActionYea[] all_actions;
    private ActionYeaType last_action_type = ActionYeaType.NONE;
    
    void Start()
    {
        all_actions = new ActionYea[Convert.ToInt32(ActionYeaType.END)];
        all_actions[Convert.ToInt32(ActionYeaType.DMG_PLR)] = new ActionYea(ActionYeaType.DMG_PLR, 1, 5, 100);
        all_actions[Convert.ToInt32(ActionYeaType.DMG_PLR_BIG)] = new ActionYea(ActionYeaType.DMG_PLR_BIG, 4, 15, 60);
        all_actions[Convert.ToInt32(ActionYeaType.DMG_PLR_BIG_BIG)] = new ActionYea(ActionYeaType.DMG_PLR_BIG_BIG, 10, 50, 10);
        all_actions[Convert.ToInt32(ActionYeaType.HEAL_BOSS)] = new ActionYea(ActionYeaType.HEAL_BOSS, 1, 10, 100);
        all_actions[Convert.ToInt32(ActionYeaType.SHUF_PLR)] = new ActionYea(ActionYeaType.SHUF_PLR, 8, 0, 20);



        BigBigAttack();
    }

    void Update()
    {
        timer += Time.deltaTime;
        int second = Mathf.RoundToInt(timer);

        if (second % chip_regen_interval == 0) Earn(chip_regen_amount);
    }

    // Choose ActionYea
    public ActionYea Wethepeopleoftheunitedstatesinordertoformamoreperfectunionestablishjusticeinsuredomestictranquilityprovidforthecommondefencepromotethegeneralwelfareadnsecuretheblessingsoflibertytoourselvesandourposteritydoordainandestablishthisconstitutionfortheunitedstatesofamerica
        (ActionYea action)
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
            case ActionYeaType.NONE:
            case ActionYeaType.END:
                break;
            case ActionYeaType.DMG_PLR:
            case ActionYeaType.DMG_PLR_BIG: 
            case  ActionYeaType.DMG_PLR_BIG_BIG:
                //BigBigAttack();
                player.Damage(action.action_value);
                break;
            case ActionYeaType.HEAL_BOSS:
                Heal(action.action_value);
                break;
            case ActionYeaType.SHUF_PLR:
                player.ShuffleCards();
                break;
        }
        
        // ready next action
        ActionYea random_action = NextActionYea();
        last_action_type = random_action.type;
        
        Manage();
        return random_action;
    }

    private ActionYea NextActionYea()
    {
        int total = 0;
        for (int i = 1; i < all_actions.Length; i++)
        {
            if (all_actions[i] == null || all_actions[i].type == ActionYeaType.NONE || all_actions[i].type == ActionYeaType.END) continue;
            
            total += Convert.ToInt32(CalculateActionYeaWeight(all_actions[i]));
        }

        int roll = Random.Range(0, total);
        int cumulative = 0;
        for (int i = 1; i < all_actions.Length; i++)
        {
            if (all_actions[i] == null || all_actions[i].type == ActionYeaType.NONE || all_actions[i].type == ActionYeaType.END) continue;
            cumulative += Convert.ToInt32(CalculateActionYeaWeight(all_actions[i]));
            if (roll < cumulative) return all_actions[i];
        }
        return all_actions[1];
    }

    private float CalculateActionYeaWeight(ActionYea action)
    {
        float weight = action.frequency + action.since_last_use;
        if (action.type == ActionYeaType.HEAL_BOSS && (float)health / max_health < 0.5f)
            weight += (int)(50.0f * Mathf.Pow(2, (1 - ((float)health/max_health)) * 4.0f));
        if (action.type == ActionYeaType.DMG_PLR_BIG_BIG && action.since_last_use > 10)
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












    public GameObject cannon;
    public GameObject fuse;
    public GameObject fire; // cannon explosion

    public GameObject cannonSpawnPoint;
    public GameObject fuseSpawnPoint;
    public GameObject fireSpawnPoint;


    private void BigBigAttack()
    {
        // Instantiate Cannon
        // Point Cannon
        // light fuse
        // fire cannon
        Quaternion spawnRot = Quaternion.Euler(-90f, -90f, 0f); // spawn the cannon in pointing near but not at player
        Quaternion endRot = Quaternion.Euler(-90f, -80f, 0f); // direction cannon needs to point to point towards character
        GameObject cannonGun = Instantiate(cannon, cannonSpawnPoint.transform.position, spawnRot);

        UnityEngine.Debug.Log("Creating Cannon");

        StartCoroutine(AimCannon(cannonGun, spawnRot, endRot, 2f)); // 2 equals aim animation duration
    }

    IEnumerator AimCannon (GameObject cannonGun, Quaternion spawnRot, Quaternion endRot, float duration)
    {
        float elapsed = 0f;

        while(elapsed < duration)
        {
            float t = elapsed / duration;
            cannonGun.transform.rotation = Quaternion.Lerp(spawnRot, endRot, t);
            elapsed += Time.deltaTime; 
            yield return null;
        }
        UnityEngine.Debug.Log("Aim Cannon");
        StartCoroutine(LightFuse(fuse, 4.0f)); // 4.0 equals fuse light time
    }

    IEnumerator LightFuse (GameObject fusePrefab, float duration)
    {
        UnityEngine.Debug.Log("Light Fuse");
        GameObject fuseObj = Instantiate(fusePrefab, fuseSpawnPoint.transform.position, Quaternion.identity);
        yield return new WaitForSeconds(duration);
        Destroy(fuseObj);
        // FIRE CANNON HERE **********
        GameObject fireObj = Instantiate(fire, fireSpawnPoint.transform.position, Quaternion.identity); // Creates visual of cannon exploding
        UnityEngine.Debug.Log("Destroy Fuse");
    }

}
