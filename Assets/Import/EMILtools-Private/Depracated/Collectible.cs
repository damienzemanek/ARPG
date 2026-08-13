// using System;
// using UnityEngine;
//
// public class Collectible : MonoBehaviour
// {
//     public CollectibleManager.Collectible type;
//     public int value;
//     
//     public GameObject audioParent;
//     public AudioSource source;
//     public AudioClip clip;
//     
//     public string tag;
//     
//
//     private void OnTriggerEnter(Collider other)
//     {
//         if (!other.CompareTag("Player")) return;
//         Collect();
//     }
//     
//
//     void Collect()
//     {
//         CollectibleEventSystem.RaiseEvent(type, value);
//         audioParent.transform.parent = null;
//         audioParent.gameObject.AddComponent<DestroyAfter>().time = 4f;
//         source.clip = clip;
//         source.Play();
//         Destroy(gameObject);
//     }
// }
