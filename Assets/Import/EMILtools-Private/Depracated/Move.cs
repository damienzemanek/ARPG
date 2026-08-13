// using System;
// using UnityEngine;
//
// public class Move : MonoBehaviour
// {
//     private Rigidbody rb;
//     private Vector3 movement;
//     public float moveScalar = 10f;
//
//     private void Awake()
//     {
//         rb = GetComponent<Rigidbody>();
//     }
//
//     private void Update()
//     {
//         float moveX = Input.GetAxis("Horizontal");
//         float moveZ = Input.GetAxis("Vertical");
//         movement = new Vector3(moveX, 0f, moveZ).normalized;
//     }
//
//     private void FixedUpdate()
//     {
//         rb.MovePosition(rb.position + movement * (moveScalar * Time.fixedDeltaTime));
//     }
// }
