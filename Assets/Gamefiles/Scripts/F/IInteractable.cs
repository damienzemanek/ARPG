using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    // Must [field: SerializeField]
    public List<InteractCriteria> criteria { get; set; }
    void Interact(GameObject sender);
}