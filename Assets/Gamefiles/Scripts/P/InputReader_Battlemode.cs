using System;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BattlemodePlayerInstance))]
[DefaultExecutionOrder(-999)]
public class InputReader_Battlemode : MonoBehaviour, IA_Battlemode.IPlayerActions
{
    IA_Battlemode ia;
    
    [ShowInInspector, ReadOnly] BattlemodePlayerInstance receiver;

    void Awake()
    {
        ia = new IA_Battlemode();
        ia.Player.SetCallbacks(this);
        receiver = this.Get<BattlemodePlayerInstance>();
    }

    void OnEnable()
    {
        ia = new IA_Battlemode();
        ia.Player.SetCallbacks(this);
        ia.Enable();
    }

    void OnDisable()
    {
        ia.Disable();
        ia.Dispose();
    }
    

    public void OnScroll(InputAction.CallbackContext context)
    {
        var scrollVal = context.ReadValue<Vector2>();
        if(scrollVal.y > 0) receiver.ScrollUpMoveRight(true);
        else if(scrollVal.y < 0) receiver.ScrollUpMoveLeft(true);
    }

    int moveVal;
    public void OnMove(InputAction.CallbackContext context)
    {
        moveVal = (int)context.ReadValue<float>();
    }

    public void OnUnZoom(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        receiver.BackZoomEsc();
    }

    void FixedUpdate()
    {
        if(moveVal > 0) receiver.ScrollUpMoveRight(false);
        else if(moveVal < 0) receiver.ScrollUpMoveLeft(false);
    }
}