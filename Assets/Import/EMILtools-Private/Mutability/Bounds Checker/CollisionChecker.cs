using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class CollisionChecker<TContext> : MonoBehaviour
{
    [Header("What type of Collision Checker?")]
    public bool is2D;

    [FoldoutGroup("Who will receive the Message?")] [SerializeField] private bool ThingCollidedWith;
    [FoldoutGroup("Who will receive the Message?")] [SerializeField] private bool SelectedReceiver;

    [FoldoutGroup("Who will receive the Message?")] [ShowIf("@SelectedReceiver && !is2D")] [SerializeField] private InterfaceRef<ICollisionCheckMsgReceiver<Collider, TContext>, MonoBehaviour> selectedReceiver3D;
    [FoldoutGroup("Who will receive the Message?")] [ShowIf("@SelectedReceiver && is2D")] [SerializeField] private InterfaceRef<ICollisionCheckMsgReceiver<Collider2D, TContext>, MonoBehaviour> selectedReceiver2D;

    [field: FoldoutGroup("Which collision callbacks are active?")] [SerializeField] public bool enter = true;
    [field: FoldoutGroup("Which collision callbacks are active?")] [SerializeField] public bool exit = true;
    [field: FoldoutGroup("Which collision callbacks are active?")] [SerializeField] public bool stay;

    [FoldoutGroup("Layer filtering")] [SerializeField] private LayerMask layerMask = ~0;

    [FoldoutGroup("Contexts")] [ShowIf("enter")] [SerializeField] public TContext enterContext;
    [FoldoutGroup("Contexts")] [ShowIf("stay")] [SerializeField] public TContext stayContext;
    [FoldoutGroup("Contexts")] [ShowIf("exit")] [SerializeField] public TContext exitContext;

    private HashSet<ICollisionCheckMsgReceiver<Collider, TContext>> collisions3D;
    private HashSet<ICollisionCheckMsgReceiver<Collider2D, TContext>> collisions2D;

    void Awake()
    {
        if (is2D)
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = false;

            if (ThingCollidedWith)
                collisions2D = new HashSet<ICollisionCheckMsgReceiver<Collider2D, TContext>>();
        }
        else
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = false;

            if (ThingCollidedWith)
                collisions3D = new HashSet<ICollisionCheckMsgReceiver<Collider, TContext>>();
        }
    }

    bool PassesLayerMask(GameObject go) =>
        (layerMask.value & (1 << go.layer)) != 0;

    // -------------------- 3D --------------------

    private void OnCollisionEnter(Collision other)
    {
        if (is2D || !enter || !PassesLayerMask(other.gameObject)) return;

        var col = other.collider;

        if (ThingCollidedWith)
        {
            if (!col.TryGetComponent(out ICollisionCheckMsgReceiver<Collider, TContext> receiver)) return;
            if (!collisions3D.Add(receiver)) return;
            receiver.OnEnterBounds(col, this, enterContext);
        }

        if (SelectedReceiver)
            selectedReceiver3D.Value?.OnEnterBounds(col, this, enterContext);
    }

    private void OnCollisionExit(Collision other)
    {
        if (is2D || !PassesLayerMask(other.gameObject)) return;

        var col = other.collider;

        if (ThingCollidedWith)
        {
            if (!col.TryGetComponent(out ICollisionCheckMsgReceiver<Collider, TContext> receiver)) return;
            if (!collisions3D.Remove(receiver)) return;
            if (!exit) return;
            receiver.OnExitBounds(col, this, exitContext);
        }

        if (!exit) return;
        if (SelectedReceiver)
            selectedReceiver3D.Value?.OnExitBounds(col, this, exitContext);
    }

    private void OnCollisionStay(Collision other)
    {
        if (is2D || !stay || !PassesLayerMask(other.gameObject)) return;

        var col = other.collider;

        if (ThingCollidedWith)
        {
            if (!col.TryGetComponent(out ICollisionCheckMsgReceiver<Collider, TContext> receiver)) return;
            receiver.OnStayBounds(col, this, stayContext);
        }

        if (SelectedReceiver)
            selectedReceiver3D.Value?.OnStayBounds(col, this, stayContext);
    }

    // -------------------- 2D --------------------

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!is2D || !enter || !PassesLayerMask(other.gameObject)) return;

        var col = other.collider;

        if (ThingCollidedWith)
        {
            if (!col.TryGetComponent(out ICollisionCheckMsgReceiver<Collider2D, TContext> receiver)) return;
            if (!collisions2D.Add(receiver)) return;
            receiver.OnEnterBounds(col, this, enterContext);
        }

        if (SelectedReceiver)
            selectedReceiver2D.Value?.OnEnterBounds(col, this, enterContext);
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (!is2D || !PassesLayerMask(other.gameObject)) return;

        var col = other.collider;

        if (ThingCollidedWith)
        {
            if (!col.TryGetComponent(out ICollisionCheckMsgReceiver<Collider2D, TContext> receiver)) return;
            if (!collisions2D.Remove(receiver)) return;
            if (!exit) return;
            receiver.OnExitBounds(col, this, exitContext);
        }

        if (!exit) return;
        if (SelectedReceiver)
            selectedReceiver2D.Value?.OnExitBounds(col, this, exitContext);
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (!is2D || !stay || !PassesLayerMask(other.gameObject)) return;

        var col = other.collider;

        if (ThingCollidedWith)
        {
            if (!col.TryGetComponent(out ICollisionCheckMsgReceiver<Collider2D, TContext> receiver)) return;
            receiver.OnStayBounds(col, this, stayContext);
        }

        if (SelectedReceiver)
            selectedReceiver2D.Value?.OnStayBounds(col, this, stayContext);
    }
}