using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;


public interface IDetectionResponsive
{
    Action<GameObject> OnDetect { get; set; }
    Action<GameObject> OnUndetect { get; set; }
}


/// <summary>
/// Separate Object as child of DetectorUser
/// </summary>
[Serializable]
public class DetectionGate : IDetectionResponsive
{
    public bool hasDetected => currentlyDetected.Count > 0;
    
    [ShowInInspector, ReadOnly] List<Detector> requiredDetectors = new List<Detector>();
    [ShowInInspector, ReadOnly] List<GameObject> partialDetections = new List<GameObject>();
    [ShowInInspector, ReadOnly] public List<GameObject> currentlyDetected = new List<GameObject>();
    
    void Evaluate()
    {
        // Use a reverse loop to safely handle removals if necessary
        for (int i = partialDetections.Count - 1; i >= 0; i--)
        {
            var go = partialDetections[i];
            bool allMet = requiredDetectors.All(d => d.IsDetected(go));

            if (allMet && !currentlyDetected.Contains(go))
            {
                currentlyDetected.Add(go);
                OnDetect?.Invoke(go);
                
            }
            else if (!allMet && currentlyDetected.Contains(go))
            {
                currentlyDetected.Remove(go);
                OnUndetect?.Invoke(go);
            }
        }
    }
    
    public void PartialDetect(GameObject go)
    {
        if (!partialDetections.Contains(go)) partialDetections.Add(go);
        Evaluate();
    }
    
    public void PartialUndetect(GameObject go)
    {
        Evaluate();
        // Remove from partial detections regardless of detector state to honor the request
        partialDetections.Remove(go);
    }

    public void ForceUnDetect(GameObject go)
    {
        PartialUndetect(go);
        currentlyDetected.Remove(go);
    }
    
    public void AddDetector(Detector detector)
    {
        requiredDetectors.Add(detector);
        detector.SetGate(this);
    }
    
    public void RemoveDetector(Detector detector)
    {
        requiredDetectors.Remove(detector);
        detector.SetGate(null);
    }

    public Action<GameObject> OnDetect { get; set; }
    public Action<GameObject> OnUndetect { get; set; }
}