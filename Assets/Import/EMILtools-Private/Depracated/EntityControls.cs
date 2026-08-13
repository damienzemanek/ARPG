using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DependencyInjection;
using System.Collections;
using EMILtools.Extensions;

[DefaultExecutionOrder(order: -10000)]
public class EntityControls : MonoBehaviour, IDependencyProvider
{
    // [Provide]
    // public EntityControls Provide()
    // {
    //     print("Individual DI: Providing EntityControls");
    //     return this;
    // }
    //
    //
    // [field: SerializeField] public GameObject headDirection { get; private set; }
    // [field:SerializeField] public GameObject bodyDirection { get; private set; }
    //
    // public InputAction ia_move;
    // public Func<Vector2> move;
    // public Action startMove;
    // public Action stopMove;
    //
    // public InputAction ia_look;
    // public Func<Vector2> look;
    //
    // public InputAction ia_interact;
    // public Action interact;
    // public Action interactHold;
    // public Action interactHoldCancel;
    // public bool holding;
    //
    // public InputAction ia_mouse1;
    // public Action mouse1;
    // public bool _canMove = true;
    // public bool canMove
    // {
    //     get => _canMove;
    //     set
    //     {
    //         _canMove = value;
    //         if (_canMove) ia_move.Enable();
    //         else ia_move.Disable();
    //     }
    //
    // }
    //
    // private void Awake()
    // {
    //     IA = new IA_PLAYER();
    //     IA.Enable();
    //     canMove = true;
    // }
    //
    //
    // private void OnEnable()
    // {
    //     ia_move = IA.Player.Move;
    //     ia_move.Enable();
    //     move = () => ia_move.ReadValue<Vector2>();
    //     ia_move.started += ctx => startMove?.Invoke();
    //     ia_move.canceled += ctx => stopMove?.Invoke();
    //
    //
    //     ia_look = IA.Player.Look;
    //     ia_look.Enable();
    //     look = () => ia_look.ReadValue<Vector2>();
    //
    //     ia_interact = IA.Player.Interact;
    //     ia_interact.Enable();
    //     ia_interact.performed += ctx => Interact();
    //     interact = () => { }; //empty action defaults
    //
    //     ia_interact.started += ctx => StartHold();
    //     ia_interact.canceled += ctx => StopHold();
    //
    //     ia_mouse1 = IA.Player.Mouse1;
    //     ia_mouse1.Enable();
    //     ia_mouse1.performed += ctx => mouse1?.Invoke();
    //     mouse1 = () => { }; //empty action defaults
    //
    //     Assign();
    // }
    //
    // private void OnDisable()
    // {
    //     move = null;
    //     ia_move.Disable();
    //
    //     look = null;
    //     ia_look.Disable();
    //
    //     interact = null;
    //     ia_interact.Disable();
    //
    //     ia_interact.performed -= ctx => Interact();
    //     ia_interact.started -= ctx => StartHold();
    //     ia_interact.canceled -= ctx => StopHold();
    //
    //     mouse1 = null;
    //     ia_mouse1.Disable();
    //
    //
    // }
    //
    // public void Assign()
    // {
    //     if(this.Has(out AudioStepper footsteps))
    //     {
    //         startMove += footsteps.AudioStart;
    //         stopMove += footsteps.AudioStop;
    //     }
    // }
    //
    // public void DeAssign()
    // {
    //     if (this.Has(out AudioStepper footsteps))
    //     {
    //         startMove -= footsteps.AudioStart;
    //         stopMove -= footsteps.AudioStop;
    //     }
    // }
    //
    // void Interact()
    // {
    //     this.Log("Player pressed E");
    //     interact?.Invoke();
    // }
    //
    // void StartHold()
    // {
    //     holding = true;
    //     this.Log("Player HOLDING... ");
    //     StopCoroutine(InteractHoldValueIncrease(0.1f));
    //     StartCoroutine(routine: InteractHoldValueIncrease(0.1f));
    // }
    // public void ForceStopHold() => StopHold();
    // void StopHold()
    // {
    //     this.Log("Player HOLDING CANCLED ");
    //     holding = false;
    // }
    //
    // IEnumerator InteractHoldValueIncrease(float delay)
    // {
    //     this.Log("Attempting to increase HOLD value");
    //     while (holding)
    //     {
    //         this.Log("Controls: Interact Holding...");
    //         yield return new WaitForSeconds(delay);
    //         interactHold?.Invoke();
    //     }
    //     StopCoroutine(routine: InteractHoldValueIncrease(0.1f));
    //     interactHoldCancel?.Invoke();
    // }

}