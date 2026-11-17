using System;
using UnityEngine;

public class SliderWorld : MonoBehaviour
{
    [Range(0, 1)] public float value = 1;
    public float xPosEmpty;
    public float xPosFull;
    public float xScaleEmpty;
    public float xScaleFull;
    public Transform go_Slider;

    public void MutateValue(float by)
    {
        value = Mathf.Clamp01(value + by);
        SetSlider(value);
    }
    
    public void SetSlider(float value)
    {
        float scale = xScaleEmpty + (xScaleFull - xScaleEmpty) * value;

        go_Slider.localScale = new Vector3(scale, go_Slider.transform.localScale.y, go_Slider.transform.localScale.z);
        go_Slider.localPosition = new Vector3(
            xPosEmpty + (xPosFull - xPosEmpty) * value
                      + (scale - xScaleEmpty) * 0.5f,
            go_Slider.transform.localPosition.y, go_Slider.transform.localPosition.z);
    }
    
    public bool IsFull => value >= 1;
    public bool IsEmpty => value <= 0;

    void OnValidate()
    {
        SetSlider(value);
    }
}
