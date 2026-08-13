using System;
using System.Collections;
using EMILtools.Extensions;
using EMILtools.Utilities;
using UnityEngine;

public class VolleyProjectileSpawner : MonoBehaviour
{
    public bool canFire = true;
    public ProjectileSpawnManager projSpawner;
    public bool volleyFiring = false;
    public float volleyDelay = 1f;
    public float volleyLength = 2f;
    public bool volleyOnCooldown;
    public Deviatable animSpeed;

    public Action onVolleyStart;
    public Action onVolleyEnd;

    void Start()
    {
        if(this.Has(out Animator anim)) anim.speed = animSpeed;
    }

    void Update()
    {
        if(!canFire) return;
        if(volleyFiring && !volleyOnCooldown) projSpawner.Spawn();
        FireVolley();
    }

    void FireVolley()
    {
        if (!volleyFiring && !volleyOnCooldown)
        {
            StartCoroutine(C_FireVolley());
            volleyFiring = true;
            onVolleyStart?.Invoke();
        }
    }
    IEnumerator C_FireVolley()
    {
        yield return new WaitForSeconds(volleyLength);
        volleyFiring = false;
        volleyOnCooldown = true;
        yield return new WaitForSeconds(volleyDelay);
        volleyOnCooldown = false;
        onVolleyEnd?.Invoke();
    }
}