using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Btn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public bool enter;
    public bool exit;
    public bool click;
    
    [ShowIf(nameof(enter))] public UnityEvent onEnter;
    [ShowIf(nameof(exit))] public UnityEvent onExit;
    [ShowIf(nameof(click))] public UnityEvent onClick;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!enter) return;
        Debug.Log($"[{gameObject.name}] Btn Hovered Over");
        onEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!exit) return;
        Debug.Log($"[{gameObject.name}] Btn Hovered Out");
        onExit?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!click) return;
        Debug.Log($"[{gameObject.name}] Btn Clicked");
        onClick?.Invoke();
    }
}
