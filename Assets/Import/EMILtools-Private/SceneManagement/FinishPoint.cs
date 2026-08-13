using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class FinishPoint : MonoBehaviour
{
   public LoadSceneConnector loadAdapater;
   public int loadIndx = 3;
   void OnTriggerEnter(Collider other)
   {
      if (!other.CompareTag("Player")) return;
      Win();
   }

   [Button]
   public void Win()
   {
      loadAdapater.Load(loadIndx);
   }
}
