using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderRuntime : MonoBehaviour
{
    public Slider slider;
    public float increment = 0.01f;
    public float delay = 0.05f;

    public void GainValue(float value, Action postHook) => StartCoroutine(C_GainValue(value, postHook));

    IEnumerator C_GainValue(float value, Action postHook)
    {
        //Can you make value an increment of increment
        int steps = (int)(value / increment);

        while(steps > 0)
        {
            steps -= 1;
            if(slider.value < 1)
                slider.value += increment;

            yield return new WaitForSeconds(delay);

        }

        postHook?.Invoke();
    }

    public void Display(float val)
    {
        if(slider.value < 1)
            slider.value = val;
    }
}
