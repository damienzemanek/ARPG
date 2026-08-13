using System;
using EMILtools.Core;
using UnityEngine;

public class Health : MonoBehaviour
{

    [SerializeField] int maxHp = 100;
    [SerializeField] FloatEventChannel healthChannelPublisher;
    int hp;

    public bool isDead => (hp < 0);

    void Awake()
    {
        hp = maxHp;
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        healthChannelPublisher?.Invoke(hp / (float)maxHp);
    }
}
