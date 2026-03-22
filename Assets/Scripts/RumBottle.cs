using System;
using System.Collections;
using UnityEngine;

public class RumBottle : MonoBehaviour
{
    [SerializeField] private Transform boss;

    public void StartHitCheck()
    {
        StartCoroutine(HitCheckRoutine());
    }

    private IEnumerator HitCheckRoutine()
    {
        while (Vector3.Distance(transform.position, boss.position) > 1.0f)
        {
            yield return null;
        }
        Destroy(gameObject);
    }
}
