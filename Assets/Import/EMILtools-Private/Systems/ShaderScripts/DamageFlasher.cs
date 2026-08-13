using System;
using System.Collections;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

public class DamageFlasher : MonoBehaviour
{
    [Serializable]
    public struct FlashData
    {
        public FlashType flashType;
        [Range(0, 1)] public float flashTime; // 0.5f
        [ColorUsage(true, true)] public Color flashColor; // white
        public AnimationCurve flashCurve;
        public AnimationCurve alphaCurve;
    }

    public enum FlashType { Damage, Heal, Stun, Invunerable, Blocked }
    
    static readonly int FlashColor = Shader.PropertyToID("_FlashColor");
    static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");
    static readonly int AlphaScalar = Shader.PropertyToID("_AlphaScalar");

    public FlashData[] flashData;
    public SpriteRenderer[] sprites;
    Material[] mats;
    Coroutine flashCoroutine;

    [Button]
    public void InitFlashDataArray()
    {
        var length = System.Enum.GetValues(typeof(FlashType)).Length;
        flashData = new FlashData[length];
        for(int i = 0; i < length; i++)       
        {
            flashData[i].flashType = (FlashType)i;
            flashData[i].flashTime = 0.5f;
            flashData[i].flashColor = Color.white;
            flashData[i].flashCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
            flashData[i].alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        }
    }

    void Awake()
    {
        if(sprites == null || sprites.Length == 0) { Debug.LogError("No Sprites Assigned to Damage Flasher"); return; }
        mats = new Material[sprites.Length];
        
        for(int i = 0; i < sprites.Length; i++) 
            mats[i] = sprites[i].material;
        

        var length = System.Enum.GetValues(typeof(FlashType)).Length;
        if(flashData.Length != length) Debug.LogError($"Flash Data Length {flashData.Length} does not match Flash Type Length {length}");
    }

    public int GetFlashDataFromEnum(FlashType flashType)
    {
        for(int i = 0; i < flashData.Length; i++)
        {
            if(flashData[i].flashType == flashType) return i;
        }
        Debug.LogError($"Flash Type {flashType} not found in flash data");
        return -1;
    }

    [Button]
    public void Flash(FlashType flashType) => flashCoroutine = StartCoroutine(C_Flash(flashType));

    IEnumerator C_Flash(FlashType flashType)
    {
        int i = GetFlashDataFromEnum(flashType);
        if (i < 0) yield break;

        SetFlashColor(i);

        float elapsedTime = 0f;
        while (elapsedTime < flashData[i].flashTime)
        {
            elapsedTime += Time.deltaTime;
            float eval = Mathf.Clamp01(elapsedTime / flashData[i].flashTime);
            float currAmount = flashData[i].flashCurve.Evaluate(eval);
            float alpha = flashData[i].alphaCurve.Evaluate(eval);
            SetFlashAmount(currAmount);
            SetAlpha(alpha);
            yield return null;
        }
        
    }

    void SetFlashColor(int flashindex)
    {
        Color color = flashData[flashindex].flashColor;
        for (int i = 0; i < sprites.Length; i++)
            mats[i].SetColor(FlashColor, color);
    }

    void SetFlashAmount(float currAmount)
    {
        for(int i = 0; i < sprites.Length; i++)
            mats[i].SetFloat(FlashAmount, currAmount);
    }

    void SetAlpha(float alpha)
    {
        for(int i = 0; i < sprites.Length; i++)
            mats[i].SetFloat(AlphaScalar, alpha);
    }
}
