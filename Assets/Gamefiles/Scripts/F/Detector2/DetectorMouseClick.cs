using UnityEngine;

public class DetectorMouseClick : Detector
{
    public override DetectorType type => DetectorType.MouseOver;
    
    void OnMouseDown()
    {
        if (DoesntHandle(DetectionState.Enter)) return;
        Detect(gameObject);
    }
    
    void OnMouseUp()
    {
        if (DoesntHandle(DetectionState.Exit)) return;
        LoseDetect(gameObject);
    }

    public void OnMouseDrag()
    {
        if (DoesntHandle(DetectionState.Stay)) return;
        Detect(gameObject);
    }

    // Mouse overs are only checking for one object being moused over, which is ALWAYS this object (the detector)
    public override bool IsDetected(GameObject go) => detections.Count > 0;

}