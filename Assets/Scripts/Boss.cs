using System;
using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

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
    [SerializeField] private int chip_regen_interval = 2;
    [SerializeField] private int chip_regen_amount = 1;

    private Action[] all_actions;
    private Action current_action; 
    private ActionType last_action_type = ActionType.NONE;

    [SerializeField] private float action_interval = 3f; 
    private float action_timer = 0f; 
    private int last_regen_second = 0; 
    
    [SerializeField] private GameObject poker_chip_prefab;
    private GameObject[] chip_pool;
    [SerializeField] private Transform[] chip_pile_locations; 
    [SerializeField] private float chip_stack_height = 0.05f; // space between each chip on the stack vertically
    [SerializeField] private float chip_stack_jitter = 0.02f; // give random jitter to mix up stack horizontally
    [SerializeField] private float health_chip_scale = 1f; // used to physically scale chip in the scene

    private GameObject[][] health_chip_piles; // [pile][chip from bottom]
    private int current_pile = 0;
    private int chips_in_current_pile;
    private int chips_per_pile;

    void Start()
    {
        health = max_health;
        chips = max_chips;

        all_actions = new Action[Convert.ToInt32(ActionType.END)];
        all_actions[Convert.ToInt32(ActionType.DMG_PLR)] = new Action(ActionType.DMG_PLR, 1, 5, 100);
        all_actions[Convert.ToInt32(ActionType.DMG_PLR_BIG)] = new Action(ActionType.DMG_PLR_BIG, 4, 15, 60);
        all_actions[Convert.ToInt32(ActionType.DMG_PLR_BIG_BIG)] = new Action(ActionType.DMG_PLR_BIG_BIG, 6, 50, 10);
        all_actions[Convert.ToInt32(ActionType.HEAL_BOSS)] = new Action(ActionType.HEAL_BOSS, 1, 10, 100);
        all_actions[Convert.ToInt32(ActionType.SHUF_PLR)] = new Action(ActionType.SHUF_PLR, 8, 0, 20);

        // Boss action chip pool (used for attack animations)
        chip_pool = new GameObject[25];
        Vector3 chipsSpawnPoint = new Vector3(-1, -10, 0);
        for (int i = 0; i < 25; i++)
        {
            chip_pool[i] = Instantiate(poker_chip_prefab, chipsSpawnPoint, Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up));
            chip_pool[i].transform.localScale = Vector3.one * health_chip_scale;
        }

        CreateChipStacks();

        current_action = NextAction(); 
    }

    void Update()
    {
        if (health <= 0)
        {
            SceneManager.LoadScene("MainMenu");
        }

        timer += Time.deltaTime;
        int second = Mathf.FloorToInt(timer); 


        if (second % chip_regen_interval == 0 && second != last_regen_second) 
        {
            Earn(chip_regen_amount);
            last_regen_second = second; 
        }

        

        action_timer += Time.deltaTime;
        if (action_timer >= action_interval)
        {
            action_timer = 0f;
            current_action = ExcutePreamble(current_action);
        }

        


        
    }


    private void CreateChipStacks()
    {
        chips_per_pile = (max_health / 10) / chip_pile_locations.Length;
        health_chip_piles = new GameObject[chip_pile_locations.Length][];
        for (int j = 0; j < chip_pile_locations.Length; j++)
        {
            health_chip_piles[j] = new GameObject[chips_per_pile];
            for (int k = 0; k < chips_per_pile; k++)
            {
                Vector3 jitter = new Vector3(Random.Range(-chip_stack_jitter, chip_stack_jitter), 0f, Random.Range(-chip_stack_jitter, chip_stack_jitter)); // get random jitter
                Vector3 pos = chip_pile_locations[j].position + Vector3.up * (k * chip_stack_height) + jitter; // set position
                health_chip_piles[j][k] = Instantiate(poker_chip_prefab, pos, Quaternion.Euler(90f, Random.Range(0f, 360f), 0f)); // create chip in that position
                health_chip_piles[j][k].transform.localScale = Vector3.one * health_chip_scale; // scale it to look right
                
            }
        }
        current_pile = 0;
        chips_in_current_pile = chips_per_pile;
    }

    
    private void RemoveHealthChips(int damage)
    {
        int chips_to_remove = damage / 10;

        while (chips_to_remove > 0 && current_pile < health_chip_piles.Length)
        {
            int top = chips_in_current_pile - 1;
            if (top >= 0 && health_chip_piles[current_pile][top] != null)
            {
                Destroy(health_chip_piles[current_pile][top]);
                health_chip_piles[current_pile][top] = null;
                chips_in_current_pile--;
                chips_to_remove--;
            }

            if (chips_in_current_pile <= 0)
            {
                current_pile++;
                if (current_pile < health_chip_piles.Length)
                    chips_in_current_pile = chips_per_pile;
            }
        }
    }

    private void AddHealthChips(int healing)
    {
        int chips_to_add = healing / 10;

        while (chips_to_add > 0 && current_pile >= 0)
        {
            if (chips_in_current_pile < chips_per_pile)
            {
                int i = chips_in_current_pile;
                Vector3 jitter = new Vector3(Random.Range(-chip_stack_jitter, chip_stack_jitter), 0f, Random.Range(-chip_stack_jitter, chip_stack_jitter));
                Vector3 pos = chip_pile_locations[current_pile].position + Vector3.up * (i * chip_stack_height) + jitter;
                health_chip_piles[current_pile][i] = Instantiate(poker_chip_prefab, pos, Quaternion.Euler(90f, Random.Range(0f, 360f), 0f));
                health_chip_piles[current_pile][i].transform.localScale = Vector3.one * health_chip_scale;
                chips_in_current_pile++;
                chips_to_add--;
            }

            if (chips_in_current_pile >= chips_per_pile)
            {
                if (current_pile > 0)
                {
                    current_pile--;
                    chips_in_current_pile = 0;
                }
                else break; // pile 0 is full, fully restored
            }
        }
    }

    

    
    private Action ExcutePreamble(Action action) 
    {
        for (int i = 1; i < all_actions.Length; i++) 
        {
            all_actions[i].since_last_use++;
        }
        action.since_last_use = 0;
        
        Spend(action.cost); 

        // perform current action
        switch (action.type)
        {
            default:
            case ActionType.NONE:
            case ActionType.END:
                break;
            case ActionType.DMG_PLR:
                PlayDamagePlayerAnim(action.action_value); 
                break;
            case ActionType.DMG_PLR_BIG:
                BigAttack(action.action_value); 
                break;
            case  ActionType.DMG_PLR_BIG_BIG:
                BigBigAttack(action.action_value); 
                break;
            case ActionType.HEAL_BOSS:
                Heal(action.action_value);
                break;
            case ActionType.SHUF_PLR:
                player.ShuffleCards(true);
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
    
    private void PlayDamagePlayerAnim(int damage) // ***
    {
        StartCoroutine(MoveChipRoutine(0.75f, damage)); // ***
    }

    private IEnumerator MoveChipRoutine(float duration, int damage)
    {
        float elapsed_time = 0.0f;
        Vector3[] target_points = new Vector3[chip_pool.Length];

        for(int i = 0; i < chip_pool.Length; i++){
            Vector2 point = Random.insideUnitCircle * 0.75f;
            target_points[i] = new Vector3(0.0f, point.y, point.x);
            target_points[i] += player.transform.position;
        }

        while (elapsed_time < duration)
        {
            for(int i = 0; i < chip_pool.Length; i++){
                chip_pool[i].transform.position = Vector3.Lerp(chip_pool[i].transform.position, target_points[i], 5.0f * Time.deltaTime);
                chip_pool[i].transform.RotateAround(chip_pool[i].transform.position, Vector3.up, 5.0f);
            }

            elapsed_time += Time.deltaTime;

            yield return null;
        }

        player.Damage(damage); // *** damage lands when chips reach player
        ResetChips();
    }
    
    private void ResetChips()
    {
        foreach (GameObject chip in chip_pool)
        {
            chip.transform.position = transform.position;
        }
    }
    
    public void Damage(int amount)
    {
        health -= amount;
        RemoveHealthChips(amount);
    }
    public void Heal(int amount)
    {
        health += amount;
        AddHealthChips(amount);
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
    public GameObject explosion; // cannon explosion

    public GameObject cannonSpawnPoint;
    public GameObject fuseSpawnPoint;
    public GameObject fireSpawnPoint;


    private void BigBigAttack(int damage) // ***
    {
        // Instantiate Cannon
        // Point Cannon
        // light fuse
        // fire cannon
        Quaternion spawnRot = Quaternion.Euler(-90f, 0f, 0f);
        Quaternion endRot = Quaternion.Euler(-90f, 33f, 0f);
        GameObject cannonGun = Instantiate(cannon, cannonSpawnPoint.transform.position, spawnRot);

        StartCoroutine(AimCannon(cannonGun, spawnRot, endRot, 2f, damage)); // ***
    }

    IEnumerator AimCannon (GameObject cannonGun, Quaternion spawnRot, Quaternion endRot, float duration, int damage) // ***
    {
        float elapsed = 0f;

        while(elapsed < duration)
        {
            float t = elapsed / duration;
            cannonGun.transform.rotation = Quaternion.Lerp(spawnRot, endRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(LightFuse(fuse, cannonGun, 4.0f, damage)); // ***
    }

    IEnumerator LightFuse (GameObject fusePrefab, GameObject cannonGun, float duration, int damage) // ***
    {
        Quaternion fuseRot = Quaternion.Euler(-90f, 0f, 0f);
        GameObject fuseObj = Instantiate(fusePrefab, fuseSpawnPoint.transform.position, fuseRot);
        yield return new WaitForSeconds(duration);
        Destroy(fuseObj);
        // FIRE CANNON HERE **********
        GameObject fireObj = Instantiate(explosion, fireSpawnPoint.transform.position, Quaternion.identity); // Creates visual of cannon exploding
        player.Damage(damage); // *** damage lands when cannon fires

        yield return new WaitForSeconds(3f);
        Destroy(cannonGun);
    }




    public GameObject gun;
    public GameObject smoke; // gunshot

    public GameObject gunSpawnPoint;
    public GameObject smokeSpawnPoint;

    private void BigAttack(int damage) // ***
    {
        Quaternion spawnRot = Quaternion.Euler(-90f, 0f, 0f);
        Quaternion endRot = Quaternion.Euler(-90f, -20f, 0f);
        GameObject gunGun = Instantiate(gun, gunSpawnPoint.transform.position, spawnRot);

        StartCoroutine(AimGun(gunGun, spawnRot, endRot, .5f, damage)); // ***
    }

    IEnumerator AimGun (GameObject gunGun, Quaternion spawnRot, Quaternion endRot, float duration, int damage) // ***
    {
        float elapsed = 0f;

        while(elapsed < duration)
        {
            float t = elapsed / duration;
            gunGun.transform.rotation = Quaternion.Lerp(spawnRot, endRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(FireGun(smoke, gunGun, 1f, damage)); // ***
    }

    IEnumerator FireGun (GameObject smoke, GameObject gunGun, float duration, int damage) // ***
    {
        yield return new WaitForSeconds(duration);
        // FIRE CANNON HERE **********
        GameObject smokeObj = Instantiate(smoke, smokeSpawnPoint.transform.position, Quaternion.identity);
        smokeObj.transform.localScale = Vector3.one * 0.5f;
        player.Damage(damage); // *** damage lands when shot fires
        yield return new WaitForSeconds(.3f);
        Destroy(smokeObj);

        yield return new WaitForSeconds(1f);
        Destroy(gunGun);
    }
}
