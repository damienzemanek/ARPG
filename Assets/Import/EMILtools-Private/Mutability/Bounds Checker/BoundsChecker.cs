using System;
using System.Collections.Generic;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class BoundsChecker<TContext> : MonoBehaviour
{
    [Header("What type of Bounds Checker?")]
    public bool is2D;

    [FoldoutGroup("Who will receive the Message?")] [SerializeField] private bool ThingCollidedWith;
    [FoldoutGroup("Who will receive the Message?")] [SerializeField] private bool SelectedReceiver;

    [FoldoutGroup("Who will receive the Message?")] [SerializeField][ShowIf("@SelectedReceiver && !is2D")]
    private InterfaceRef<IBoundsCheckMsgReceiver<Collider, TContext>, MonoBehaviour> selectedReceiver3D;

    [FoldoutGroup("Who will receive the Message?")] [SerializeField][ShowIf("@SelectedReceiver && is2D")]
    private InterfaceRef<IBoundsCheckMsgReceiver<Collider2D, TContext>, MonoBehaviour> selectedReceiver2D;

    [field: FoldoutGroup("Which trigger callbacks are active?")] [field: SerializeField] public bool enter { get; private set; } = true;
    [field: FoldoutGroup("Which trigger callbacks are active?")] [field: SerializeField] public bool exit { get; private set; } = true;
    [field: FoldoutGroup("Which trigger callbacks are active?")] [field: SerializeField] public bool stay { get; private set; }
    [FoldoutGroup("Which trigger callbacks are active?")] [ShowIf("@!exit")] [SerializeField] private bool exitOnDisableEvenIfExitFalse = true;

        
    [FoldoutGroup("Layer filtering")] [SerializeField] private LayerMask layerMask = ~0;
    
    
    [FoldoutGroup("Context")] [Header("What Message is being sent?")]
    [FoldoutGroup("Context")] [ShowIf("enter")] [SerializeField] public TContext enterContext;
    [FoldoutGroup("Context")] [ShowIf("stay")]  [SerializeField] public TContext stayContext;
    [FoldoutGroup("Context")] [ShowIf("exit")]  [SerializeField] public TContext exitContext;
    [FoldoutGroup("Context")] [ShowIf("exitOnDisableEvenIfExitFalse")]  [SerializeField] public TContext disableContext;

    
    [HideInInspector] public IPredicate injectedEnterPredicate;
    [HideInInspector] public IPredicate injectedExitPredicate;
    [HideInInspector] public IPredicate injectedStayPredicate;

    private HashSet<IBoundsCheckMsgReceiver<Collider, TContext>> collisions3D;
    private HashSet<IBoundsCheckMsgReceiver<Collider2D, TContext>> collisions2D;

    void Awake()
    {
        if (is2D)
        {
            this.Get<Collider2D>().isTrigger = true;
            if (ThingCollidedWith)
                collisions2D = new HashSet<IBoundsCheckMsgReceiver<Collider2D, TContext>>();

            if (SelectedReceiver && selectedReceiver2D.Value == null)
                Debug.LogError("No 2D Receiver Selected");
        }
        else
        {
            this.Get<Collider>().isTrigger = true;
            if (ThingCollidedWith)
                collisions3D = new HashSet<IBoundsCheckMsgReceiver<Collider, TContext>>();

            if (SelectedReceiver && selectedReceiver3D.Value == null)
                Debug.LogError("No 3D Receiver Selected");
        }
    }

    bool PassesLayerMask(GameObject go) =>
        (layerMask.value & (1 << go.layer)) != 0;

    void OnEnable()
    {
        if (!is2D || !enter) return;

        var col = this.Get<Collider2D>();
        if (col == null) return;

        var filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = layerMask,
            useTriggers = true
        };

        var hits = new Collider2D[32];
        int count = col.Overlap(filter, hits);

        if (ThingCollidedWith)
        {
            collisions2D ??= new HashSet<IBoundsCheckMsgReceiver<Collider2D, TContext>>();
            collisions2D.Clear();

            for (int i = 0; i < count; i++)
            {
                var other = hits[i];
                if (other == null) continue;
                if (!PassesLayerMask(other.gameObject)) continue;
                
                
                if (!other.TryGetComponent(out IBoundsCheckMsgReceiver<Collider2D, TContext> receiver)) continue;
                if (!collisions2D.Add(receiver)) continue;
                receiver.OnEnterBounds(other, this, enterContext);
            }
        }

        if (SelectedReceiver && count > 0)
        {
            for (int i = 0; i < count; i++)
                selectedReceiver2D?.Value?.OnEnterBounds(hits[0], this, enterContext);
        }
    }


    void OnDisable()
    {
        if (!exit && !exitOnDisableEvenIfExitFalse) return;

        if (is2D)
        {
            if (ThingCollidedWith && collisions2D != null)
            {
                foreach (var c in collisions2D)
                    c?.OnExitBounds(null, this, exitContext);
                collisions2D.Clear();
            }

            if (SelectedReceiver)
                selectedReceiver2D.Value?.OnExitBounds(null, this, disableContext);
        }
        else
        {
            if (ThingCollidedWith && collisions3D != null)
            {
                foreach (var c in collisions3D)
                    c?.OnExitBounds(null, this, exitContext);
                collisions3D.Clear();
            }
            
            if (SelectedReceiver)
                selectedReceiver3D.Value?.OnExitBounds(null, this, disableContext);
        }
    }

    // -------------------- 3D --------------------

    private void OnTriggerEnter(Collider other)
    {
        if (is2D || !enter || !PassesLayerMask(other.gameObject)) return;

        if (ThingCollidedWith)
        {
            if (!other.TryGetComponent(out IBoundsCheckMsgReceiver<Collider, TContext> receiver)) return;
            if (!collisions3D.Add(receiver)) return;
            receiver.OnEnterBounds(other, this, enterContext);
        }

        if (SelectedReceiver)
            selectedReceiver3D.Value?.OnEnterBounds(other, this, enterContext);
    }

    private void OnTriggerExit(Collider other)
    {
        if (is2D || !PassesLayerMask(other.gameObject)) return;

        if (ThingCollidedWith)
        {
            if (!other.TryGetComponent(out IBoundsCheckMsgReceiver<Collider, TContext> receiver)) return;
            if (!collisions3D.Remove(receiver)) return;
            if (!exit) return;
            receiver.OnExitBounds(other, this, exitContext);
        }

        if (!exit) return;
        if (SelectedReceiver)
            selectedReceiver3D.Value?.OnExitBounds(other, this, exitContext);
    }

    private void OnTriggerStay(Collider other)
    {
        if (is2D || !stay || !PassesLayerMask(other.gameObject)) return;

        if (ThingCollidedWith)
        {
            if (!other.TryGetComponent(out IBoundsCheckMsgReceiver<Collider, TContext> receiver)) return;
            receiver.OnStayBounds(other, this, stayContext);
        }

        if (SelectedReceiver)
            selectedReceiver3D.Value?.OnStayBounds(other, this, stayContext);
    }

    // -------------------- 2D --------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!is2D || !enter || !PassesLayerMask(other.gameObject)) return;
        
        if (ThingCollidedWith)
        {
            if (!other.TryGetComponent(out IBoundsCheckMsgReceiver<Collider2D, TContext> receiver)) return;
            if (!collisions2D.Add(receiver)) return;
            receiver.OnEnterBounds(other, this, enterContext);
        }

        if (SelectedReceiver)
            selectedReceiver2D.Value?.OnEnterBounds(other, this, enterContext);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!is2D || !PassesLayerMask(other.gameObject)) return;

        if (ThingCollidedWith && !SelectedReceiver)
        {
            if (!other.TryGetComponent(out IBoundsCheckMsgReceiver<Collider2D, TContext> receiver)) return;
            if (!collisions2D.Remove(receiver)) return;
            if (!exit) return;
            receiver.OnExitBounds(other, this, exitContext);
        }
        else if (ThingCollidedWith && SelectedReceiver)
        {
            if (!other.TryGetComponent(out IBoundsCheckMsgReceiver<Collider2D, TContext> receiver)) return;
            if (!collisions2D.Remove(receiver)) return;
            if (!exit) return;
            receiver.OnExitBounds(other, this, exitContext);
            selectedReceiver2D.Value?.OnExitBounds(other, this, exitContext);
        }
        else if (!ThingCollidedWith && SelectedReceiver)
        {
            if (!exit) return;
            selectedReceiver2D.Value?.OnExitBounds(other, this, exitContext);
        }
        else
            Debug.Log("BoundsChecker: OnTriggerExit2D called with no receiver or thing collided with");
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!is2D || !stay || !PassesLayerMask(other.gameObject)) return;

        if (ThingCollidedWith)
        {
            if (!other.TryGetComponent(out IBoundsCheckMsgReceiver<Collider2D, TContext> receiver)) return;
            receiver.OnStayBounds(other, this, stayContext);
        }

        if (SelectedReceiver)
            selectedReceiver2D.Value?.OnStayBounds(other, this, stayContext);
    }
}