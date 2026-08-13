using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TopDownPlayer))]
[DefaultExecutionOrder(-999)]
public class InputReader_TopDown : MonoBehaviour, IA_TopDown.IPlayerActions
{
    IA_TopDown ia;
    
    [ShowInInspector, ReadOnly] TopDownPlayer receiver;

    void Awake()
    {
        ia = new IA_TopDown();
        ia.Player.SetCallbacks(this);
        receiver = this.Get<TopDownPlayer>();
    }

    void OnEnable()
    {
        ia = new IA_TopDown();
        ia.Player.SetCallbacks(this);
        ia.Enable();
    }

    void OnDisable()
    {
        ia.Disable();
        ia.Dispose();
    }

    public void OnMove(InputAction.CallbackContext context) 
        => receiver.data.walkInput = context.ReadValue<Vector2>();

    public void OnInteract(InputAction.CallbackContext context)
    {
        if(context.started) receiver.data.interactionInput.Set(true);
        else if(context.canceled) receiver.data.interactionInput.Set(false);
    }

    public void OnScroll(InputAction.CallbackContext context)
    {
        Debug.Log("Scroll: " + context.ReadValue<Vector2>());
    }
    
    
    public void OnJump(InputAction.CallbackContext context) { }
    public void OnPrevious(InputAction.CallbackContext context) { }
    public void OnNext(InputAction.CallbackContext context) { }
    public void OnSprint(InputAction.CallbackContext context) { }

}