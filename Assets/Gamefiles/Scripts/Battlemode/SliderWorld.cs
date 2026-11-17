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
    
    void SetSlider(float value)
    {
        go_Slider.localScale = new Vector3(xScaleEmpty + (xScaleFull - xScaleEmpty) * value, 1, 1);
        go_Slider.localPosition = new Vector3(xPosEmpty + (xPosFull - xPosEmpty) * value, 0, 0);
    }
    
    public bool IsFull => value >= 1;
    public bool IsEmpty => value <= 0;

    void OnValidate()
    {
        SetSlider(value);
    }
}
