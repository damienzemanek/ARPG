using System;
using UnityEngine;

public class Projectile : Entity
{
    public Rigidbody rb;
    ProjectileData data;

    public Projectile Initalize(ProjectileData data)
    {
        this.data = data;
        return this;
    }


    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.TryGetComponent(out IDamageable damageable)) 
            damageable.TakeDamage(data.CreateDamageContext(IDamageable.DamageLocation.Body));
        Instantiate(data.hitEffectPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
    
}
