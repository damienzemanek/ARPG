using Sirenix.OdinInspector;
using UnityEngine;
using DependencyInjection;
using Extensions;
using static EMILtools.Extensions.PhysEX;

[DefaultExecutionOrder(1)]
[RequireComponent(typeof(EntityControls))]
public class EntityMove : MonoBehaviour
{
    // [Inject] EntityControls Controls;
    // PlayerDataHolder player;
    //
    // Rigidbody rb;
    //
    // [SerializeField] public bool canMove = true;
    //
    // private void Awake()
    // {
    //     player = PlayerDataHolder.Instance;
    //
    //     if(rb == null) rb = GetComponent<Rigidbody>();
    //     if (Controls == null) Debug.LogError("No Controls found");
    // }
    //
    //
    //
    // private void FixedUpdate()
    // {
    //     // if (canMove)
    //     // {
    //     //     Vector2 moveInput = Controls.move?.Invoke() ?? Vector2.zero;
    //     //     rb.InputDirectionalMove(moveInput, player.data.move);
    //     // }
    // }





}
