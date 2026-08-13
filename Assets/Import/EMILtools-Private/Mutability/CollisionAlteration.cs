using System;
using UnityEngine;
using UnityEngine.Events;

public class CollisionAlteration : MonoBehaviour
{
   public LayerMask mask;
   public UnityEvent onEnter;
   public UnityEvent onExit;
   public UnityEvent onStay;
   
   bool PassesLayerMask(GameObject go) =>
      (mask.value & (1 << go.layer)) != 0;
   
   private void OnCollisionEnter(Collision other)
   {
      if( PassesLayerMask(other.gameObject)) onEnter.Invoke();
   }
   
   private void OnCollisionExit2D(Collision2D other)
   {
      if( PassesLayerMask(other.gameObject)) onExit.Invoke();
   }
   

   private void OnCollisionStay(Collision other)
   {
      if( PassesLayerMask(other.gameObject)) onStay.Invoke();
   }
}
