using System.Collections;
using UnityEngine;

public class PlayerHook : MonoBehaviour
{
    [SerializeField] private Transform boss;
    [SerializeField] private GameObject rum;
    [SerializeField] private GameObject rum_prefab;
    [SerializeField] private Transform hook_throw_point;
    [SerializeField] private Transform hook_heal_point;
    private Vector3 rum_hook_point;
    private Vector3 rum_spawn_pos;
    private Quaternion rum_spawn_rot;
    private Vector3 hook_start_pos;
    private Quaternion hook_start_rot;
    
    void Start()
    {
        rum_spawn_pos = rum.transform.position;
        rum_spawn_pos.y -= 2.0f;
        rum_spawn_rot = rum.transform.rotation;
        hook_start_pos = transform.position;
        hook_start_rot = transform.rotation;
        rum_hook_point = rum.transform.GetChild(0).position;
    }

    public void ThrowRum()
    {
        StartCoroutine(ThrowRumRoutine());
    }
    
    public void HealRum()
    {
        StartCoroutine(HealRumRoutine());
    }
    
    private IEnumerator ThrowRumRoutine()
    {
        while (Vector3.Distance(transform.position, rum_hook_point) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, rum_hook_point, 5.0f * Time.deltaTime);
            if (Vector3.Distance(transform.position, rum_hook_point) > 0.1f)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(transform.position - rum_hook_point), 5.0f * Time.deltaTime);
            }
            yield return null;
        }
        
        rum.transform.SetParent(transform);
        
        while (Vector3.Distance(transform.position, hook_throw_point.position) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, hook_throw_point.position, 5.0f * Time.deltaTime);
            yield return null;
        }
        
        Vector3 throw_target = hook_throw_point.position + Vector3.right;
        while (Vector3.Distance(transform.position, throw_target) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, throw_target, 50.0f * Time.deltaTime);
            yield return null;
        }
        
        rum.transform.SetParent(null);
        Rigidbody rum_rb = rum.AddComponent<Rigidbody>();
        rum_rb.isKinematic = false;
        Vector3 throwDir = (boss.position - transform.position).normalized;
        throwDir.y += 0.25f;
        throwDir.Normalize();
        rum.GetComponent<RumBottle>().StartHitCheck();
        rum_rb.AddForce(throwDir * 20.0f, ForceMode.Impulse);
        
        while (Vector3.Distance(transform.position, hook_start_pos) > 0.005f || Quaternion.Angle(transform.rotation, hook_start_rot) > 1.0f)
        {
            transform.position = Vector3.Lerp(transform.position, hook_start_pos, 5.0f * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, hook_start_rot, 5.0f * Time.deltaTime);
            yield return null;
        }
        
        rum = Instantiate(rum_prefab, rum_spawn_pos, rum_spawn_rot);
        rum_hook_point = rum.transform.GetChild(0).position;
        
        Vector3 rum_table_pos = rum_spawn_pos + Vector3.up * 2.0f;
        while (Vector3.Distance(rum.transform.position, rum_table_pos) > 0.005f)
        {
            rum.transform.position = Vector3.Lerp(rum.transform.position, rum_table_pos, 5.0f * Time.deltaTime);
            yield return null;
        }
    }
    
    private IEnumerator HealRumRoutine()
    {
        while (Vector3.Distance(transform.position, rum_hook_point) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, rum_hook_point, 5.0f * Time.deltaTime);
            if (Vector3.Distance(transform.position, rum_hook_point) > 0.1f)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(transform.position - rum_hook_point), 5.0f * Time.deltaTime);
            }
            yield return null;
        }
        
        rum.transform.SetParent(transform);
        
        while (Vector3.Distance(transform.position, hook_heal_point.position) > 0.005f)
        {
            transform.position = Vector3.Lerp(transform.position, hook_heal_point.position, 5.0f * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, hook_heal_point.rotation * Quaternion.Euler(75.0f, -75.0f, 0.0f), 5.0f * Time.deltaTime);
            yield return null;
        }
        
        yield return new WaitForSeconds(0.3f);
        
        Destroy(rum);
        
        while (Vector3.Distance(transform.position, hook_start_pos) > 0.005f || Quaternion.Angle(transform.rotation, hook_start_rot) > 1.0f)
        {
            transform.position = Vector3.Lerp(transform.position, hook_start_pos, 5.0f * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, hook_start_rot, 5.0f * Time.deltaTime);
            yield return null;
        }
        
        rum = Instantiate(rum_prefab, rum_spawn_pos, rum_spawn_rot);
        rum_hook_point = rum.transform.GetChild(0).position;
        
        Vector3 rum_table_pos = rum_spawn_pos + Vector3.up * 2.0f;
        while (Vector3.Distance(rum.transform.position, rum_table_pos) > 0.005f)
        {
            rum.transform.position = Vector3.Lerp(rum.transform.position, rum_table_pos, 5.0f * Time.deltaTime);
            yield return null;
        }
    }
}
