using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace EMILtools
{
    [ExecuteAlways]
    public class SliderUI : MonoBehaviour
    {
        public bool smoothing;
        [ShowIf("smoothing")] public float smoothingTime = 0.1f;
        [ShowIf("smoothing")] Vector2 smoothingVel;
        
        [Required] public RectTransform container;
        [Required] public Image actual;

        
        [ShowInInspector] public float min => container.rect.width;
        float _val;
        
        [ShowInInspector] [Range(0, 1)] 
        public float val
        {
            get => Mathf.InverseLerp(-min, 0, _val);
            set => _val = SetValue01(value);
        }
        
        public float SetValue(float val, float min, float max) => SetValue01(Mathf.InverseLerp(min, max, val));
        
        public float SetValue01(float v)
        {
            _val = -Mathf.Lerp(min, 0, Mathf.Clamp01(v));

            if (!smoothing)
            {
                actual.rectTransform.offsetMax = new Vector2(
                    _val,
                    actual.rectTransform.offsetMax.y
                );
            }

            return _val;
        }
        
        void Update()
        {
            if (!smoothing) return;

            Vector2 current = actual.rectTransform.offsetMax;
            Vector2 target = new(_val, current.y);

            actual.rectTransform.offsetMax = Vector2.Lerp(
                current,
                target,
                Time.fixedDeltaTime / smoothingTime
            );
        }


    }
}
