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
        Debug.Log("Entered");
        onEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!exit) return;
        Debug.Log("Exited");
        onExit?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!click) return;
        Debug.Log("Clicked");
        onClick?.Invoke();
    }
}
