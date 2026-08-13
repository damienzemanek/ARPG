using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class DetectorTrigger : Detector
{
    public override DetectorType type => DetectorType.Trigger;
    
    public TagRequirement tagRequirement;
    
    public void OnTriggerEnter(Collider other)
    {
        if (DoesntHandle(DetectionState.Enter)) return;
        if (tagRequirement.NotMet(other)) return;
        Detect(other.gameObject);
    }
    
    public void OnTriggerExit(Collider other)
    {
        if (DoesntHandle(DetectionState.Exit)) return;
        if (tagRequirement.NotMet(other)) return;
        LoseDetect(other.gameObject);
    }
    
    public void OnTriggerStay(Collider other)
    {
        if(DoesntHandle(DetectionState.Stay)) return;
        if (tagRequirement.NotMet(other)) return;
        Detect(other.gameObject);
    }
    
    public override bool IsDetected(GameObject go) => detections.Any(d => d == go);

}