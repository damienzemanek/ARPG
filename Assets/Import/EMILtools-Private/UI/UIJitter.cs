using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIJitter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] float duration;
    [SerializeField] bool onMouseOver = true;
    [SerializeField] bool isDifferentTranform;
    [SerializeField, ShowIf("isDifferentTranform")] Transform differentTransformn;

    public Transform jitterTransform
    {
        get
        {
            if (isDifferentTranform)
                return differentTransformn;
            else
                return transform;
        }
    }

    [BoxGroup("Transforms")] public bool Size;
    [BoxGroup("Transforms")] public bool Rot;
    [BoxGroup("Transforms")] public bool Pos;


    [BoxGroup("Size")][ShowIf("Size")][ShowInInspector, ReadOnly] Vector3 initialSize;
    [BoxGroup("Size")][ShowIf("Size")][SerializeField] Vector3 sizeJitterIncrease;

    [BoxGroup("Rot")][ShowIf("Rot")][ShowInInspector, ReadOnly] Quaternion initialRot;
    [BoxGroup("Rot")][ShowIf("Rot")][SerializeField] Quaternion rotTo;

    [BoxGroup("Pos")][ShowIf("Pos")][ShowInInspector, ReadOnly] Vector3 initialPos;
    [BoxGroup("Pos")][ShowIf("Pos")][SerializeField] Vector3 posTo;

    private void OnValidate()
    {
        SaveInitials();
    }
    void SaveInitials()
    {
        initialSize = jitterTransform != null ? jitterTransform.localScale : Vector3.zero;
        initialRot = jitterTransform != null ? jitterTransform.localRotation : Quaternion.identity;
        initialPos = jitterTransform != null ? jitterTransform.localPosition : Vector3.zero;
    }
    public void OnPointerEnter(PointerEventData data)
    {
        SaveInitials();
        if (onMouseOver)
            Jitter();
        print("over");
    }


    public void OnPointerExit(PointerEventData data)
    {
        if (onMouseOver)
            ResetJitter();
        print("off");

    }

    private void OnEnable()
    {
        SaveInitials();
    }

    private void OnDisable()
    {
        SaveInitials();
        if (Pos) jitterTransform.localPosition = initialPos;
        if(Rot) jitterTransform.localRotation = initialRot;
        if(Size) jitterTransform.localScale = initialSize;
    }


    public void Jitter()
    {
        StopAllCoroutines();
        if (Size) jitterTransform.LerpScale(sizeJitterIncrease, duration, this);
        if (Rot) jitterTransform.LerpRot(rotTo, duration, this);
        if (Pos) jitterTransform.Lerp(posTo, duration, this);
    }

    public void ResetJitter()
    {
        StopAllCoroutines();
        if (Size) jitterTransform.LerpScale(initialSize, duration, this);
        if (Rot) jitterTransform.LerpRot(initialRot, duration, this);
        if (Pos) jitterTransform.Lerp(initialPos, duration, this);
    }

    [Button]
    public void JitterThenReset()
    {
        bool moving = false;

        void Done()
        {
            if (!moving) return;
            moving = false;      
            ResetJitter();
        }

        if (Size) { moving = true; jitterTransform.LerpScale(sizeJitterIncrease, duration, this, Done); }
        if (Rot) { moving = true; jitterTransform.LerpRot(rotTo, duration, this, Done); }
        if (Pos) { moving = true; jitterTransform.Lerp(posTo, duration, this, Done); }

        if (!moving)
            ResetJitter();
    }

}
