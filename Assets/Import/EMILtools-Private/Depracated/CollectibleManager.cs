// using System;
// using EMILtools.Extensions;
// using Sirenix.OdinInspector;
// using TMPro;
// using UnityEngine;
//
// public class CollectibleManager : MonoBehaviour
// {
//     public enum Collectible { Coin, Gem, Banana, Soup }
//
//     [Serializable]
//     public struct CollectibleContext
//     {
//         public Collectible type;
//         public int amount;
//         public ItemSlot slot;
//     }
//
//     public CollectibleContext[] collectibles;
//     public GameObject itemSlotPrefab;
//     public Transform itemSlotsParent;
//     
//     private void Awake()
//     {
//         InitManager();
//         
//     }
//
//     private void OnEnable()
//     {
//         CollectibleEventSystem.OnEvent += Collect;
//     }
//
//     private void OnDisable()
//     {
//         CollectibleEventSystem.OnEvent -= Collect;
//     }
//
//     void InitManager()
//     {
//         Array values = Enum.GetValues(typeof(Collectible));
//         int size = Enum.GetValues(typeof(Collectible)).Length;
//         collectibles = new CollectibleContext[size];
//         
//         for (int i = 0; i < collectibles.Length; i++)
//         {
//             collectibles[i].type = (Collectible)values.GetValue(i);
//             collectibles[i].amount = 0;
//             
//             GameObject newSlot = Instantiate(itemSlotPrefab, itemSlotsParent);
//             collectibles[i].slot = newSlot.Get<ItemSlot>();
//             collectibles[i].slot.InitSlot(collectibles[i].type);
//         }
//     }
//     
//     public void Collect(Collectible type, int am)
//     {
//         for (int i = 0; i < collectibles.Length; i++)
//             if (collectibles[i].type == type)
//             {
//                 collectibles[i].amount += am;
//                 collectibles[i].slot.UpdateSlotUI(collectibles[i].amount);
//             }
//     }
// }
//
