using System;
using EMILtools.Extensions;
using UnityEngine;

public class RigidbodyAlteration : MonoBehaviour
{
    public float maxVelocity = 0f;
    Rigidbody2D rb;
    private void Awake() => GetComponent<Rigidbody2D>();
    private void Update()
    {
        rb.linearVelocityX = maxVelocity;
        rb.linearVelocityY = maxVelocity;
    }
}
