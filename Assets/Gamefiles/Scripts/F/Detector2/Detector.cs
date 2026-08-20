using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public abstract class Detector : MonoBehaviour, IDetectionResponsive
{
    public enum DetectorType
    {
        Trigger,
        Collider,
        MouseOver,
        Raycast
    }
    
    [Flags]
    public enum DetectionState
    {
        None  = 0,
        Enter = 1 << 0,
        Exit  = 1 << 1,
        Stay  = 1 << 2
    }
    
    public abstract DetectorType type { get; }
    public DetectionState state;
    public bool isRequired = true;
    [ReadOnly] public List<GameObject> detections = new List<GameObject>();
    DetectionGate gate;
    public Action<GameObject> OnDetect { get; set; }
    public Action<GameObject> OnUndetect { get; set; }
    public EmilEvent<GameObject> OnDetectINSP;
    public EmilEvent<GameObject> OnUndetectINSP;
    
    
    public void SetGate(DetectionGate gate) => this.gate = gate;
    public bool DoesntHandle(DetectionState state) => (this.state & state) != state;
    
    protected void Detect(GameObject go)
    {
        if(detections.Any(d => d == go)) return;
        detections.Add(go);
        gate?.PartialDetect(go);
        OnDetect?.Invoke(go);
        OnDetectINSP?.Invoke(go);
    }

    protected void LoseDetect(GameObject go)
    {
        detections.Remove(go);
        gate?.PartialUndetect(go);
        OnUndetect?.Invoke(go);
        OnUndetectINSP?.Invoke(go);
    }
    
    public abstract bool IsDetected(GameObject go);

    void Start()
    {
        //if(gate == null) Debug.Log("Detector " + name + " has no gate");
    }

    [Serializable]
    public struct TagRequirement
    {
        public bool UseTag;
        [ShowIf("UseTag")] public string tag;
        public bool NotMet(Collider c) => UseTag && !c.CompareTag(tag);
    }
    
}