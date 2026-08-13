using System;
using System.Collections.Generic;
using System.Linq;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;


public class Interactable : MonoBehaviour
{
    //Detector detector;
    [ShowInInspector] public DetectionGate canInteractGate;
    [Required] public DetectorTrigger withinTrigger;
    [Required] public DetectorMouseOver mouseOver;

    [ShowInInspector] public DetectionGate clickGate;
    [Required] public DetectorMouseClick mouseClick;
    
    [SerializeField] List<EmilEvent> canInteractEvents = new();
    [SerializeField] List<EmilEvent> canNoLongerInteract = new();
    [SerializeField] List<EmilEvent<GameObject>> localInteractions = new();
    [SerializeField] List<EmilEvent<GameObject>> globalInteractions = new();

    void Awake()
    {
        canInteractGate.AddDetector(withinTrigger);
        canInteractGate.AddDetector(mouseOver);
        canInteractGate.OnDetect += CanInteract;
        canInteractGate.OnUndetect += CanNoLongerInteract;
        
        clickGate.AddDetector(mouseClick);
        clickGate.OnDetect += Interact;
    }

    void CanInteract(GameObject go) => canInteractEvents.ForEach(e => e?.Invoke(go));
    void CanNoLongerInteract(GameObject go) => canNoLongerInteract.ForEach(e => e?.Invoke(go));

    void Interact(GameObject go)
    {
        if (!canInteractGate.hasDetected) return;
        var interactor = canInteractGate.currentlyDetected.First();
        Debug.Log("Interacting with " + gameObject.name);
        foreach (var interaction in localInteractions) interaction?.Invoke(interactor);
        foreach (var interaction in globalInteractions) interaction?.Invoke(interactor);
        canInteractGate.ForceUnDetect(interactor);
    }
    
}