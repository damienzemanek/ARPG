using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class DetectorMouseClick : Detector
{
    public override DetectorType type => DetectorType.MouseOver;

    // bool IsPointerOverUITaggedObject()
    // {
    //     PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
    //     var results = new List<RaycastResult>();
    //     EventSystem.current.RaycastAll(pointerData, results);
    //     return results.Any(result =>
    //         result.gameObject.GetComponentInParent<Transform>()?.CompareTag("UI") == true);
    // }

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (DoesntHandle(DetectionState.Enter)) return;

        Detect(gameObject);
    }

    void OnMouseUp()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (DoesntHandle(DetectionState.Exit)) return;

        LoseDetect(gameObject);
    }

    public void OnMouseDrag()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (DoesntHandle(DetectionState.Stay)) return;

        Detect(gameObject);
    }

    public override bool IsDetected(GameObject go) => detections.Count > 0;
}