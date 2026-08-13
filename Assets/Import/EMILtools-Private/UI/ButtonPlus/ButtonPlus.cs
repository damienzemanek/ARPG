using ProArchitecture.Data;
using ProArchitecture.Logic;
using ProArchitecture.Predicates;
using ProSM;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using static ButtonPlusDepandancies;
using static ButtonPlusDepandancies.BtnEvent;

public class ButtonPlus : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler  
{
    
    // Privis
    ProSM<BtnData> sm;
    DynamicLogics<BtnData> enterLogics, exitLogics, clickLogics;
    
    BtnData enterStateData;
    BtnData clickStateData;
    BtnData exitStateData;
    public BtnData.SharedBtnState sharedBtnState;

    bool ShowEnter => (buttonBtnEvents & Enter) != 0;
    bool ShowExit => (buttonBtnEvents & Exit) != 0;
    bool ShowClick => (buttonBtnEvents & Click) != 0;
    
    // Pubbis
    public bool disabled;
    public bool requireClickToDefault = false;
    public BtnEvent buttonBtnEvents;
    [ShowIf(nameof(ShowEnter))] public BtnReferences enterRefs;
    [ShowIf(nameof(ShowExit))] public BtnReferences exitRefs;
    [ShowIf(nameof(ShowClick))] public BtnReferences clickRefs;
    

    void Awake()
    {
        sharedBtnState.ManagedBtn = BlittableReference<ButtonPlus>.Allocate(this);

        sm.Initialize(1);
        sm.InitLayer<BtnStates, BtnData>(0);
        InitValues();
        DynamicallyPackLogics();
        EstablishTransitions();
        sm.Entry(ref exitStateData);
    }

    void DynamicallyPackLogics()
    {
        enterLogics = CreateLogics(buttonBtnEvents, Enter, ref enterRefs.Callbacks);
        exitLogics = CreateLogics(buttonBtnEvents, Exit, ref exitRefs.Callbacks);
        clickLogics = CreateLogics(buttonBtnEvents, Click, ref clickRefs.Callbacks);
        sm.Enter(0, BtnStates.Hover) = enterLogics;
        sm.Enter(0, BtnStates.Default) = exitLogics;
        sm.Enter(0, BtnStates.Pressed) = clickLogics;
    }

    DynamicLogics<BtnData> CreateLogics(BtnEvent btnEvents, BtnEvent target, ref Callbacks callbacks)
    {
        if (!btnEvents.HasFlag(target) || callbacks == Callbacks.None) return default;
        
        LogicBuilder<BtnData>.Instance.EnsureClear();
        if (callbacks.HasFlag(Callbacks.SetActive)) LogicBuilder<BtnData>.Instance.Add(ButtonPlusOperations.SetActive);
        if (callbacks.HasFlag(Callbacks.ChildsDeactivateKeepSelfActive)) LogicBuilder<BtnData>.Instance.Add(ButtonPlusOperations.DeactiveAllChildrenButKeepOAnective); 
        if (callbacks.HasFlag(Callbacks.Animate)) LogicBuilder<BtnData>.Instance.Add(ButtonPlusOperations.Animate);
        if (callbacks.HasFlag(Callbacks.ButtonUnityEvent)) LogicBuilder<BtnData>.Instance.Add(ButtonPlusOperations.BtnUnityEvent);
        if (callbacks.HasFlag(Callbacks.PlaySound)) LogicBuilder<BtnData>.Instance.Add(ButtonPlusOperations.PlaySound);
        return LogicBuilder<BtnData>.Instance.Build();
    }

    void InitValues()
    {
        if (buttonBtnEvents.HasFlag(Exit))
        {
            exitStateData.btnEventType = Exit;
            exitStateData.managedBtn = sharedBtnState.ManagedBtn;
        }

        if (buttonBtnEvents.HasFlag(Enter))
        {
            enterStateData.btnEventType = Enter;
            enterStateData.managedBtn = sharedBtnState.ManagedBtn;
        }

        if (buttonBtnEvents.HasFlag(Click))
        {
            clickStateData.btnEventType = Click;
            clickStateData.managedBtn = sharedBtnState.ManagedBtn;
        }
    }


    void EstablishTransitions()
    {
        sm.AddDirectTransition(0, BtnStates.Default, BtnStates.Hover, ButtonPredicates.IsHovered);
        sm.AddDirectTransition(0, BtnStates.Hover, BtnStates.Default, ButtonPredicates.IsNotHovered);
        sm.AddDirectTransition(0, BtnStates.Hover, BtnStates.Pressed, ButtonPredicates.IsClicked);
        if (requireClickToDefault)
            sm.AddDirectTransition(0, BtnStates.Pressed, BtnStates.Default, ButtonPredicates.IsClickedToDefault);
        else sm.AddDirectTransition(0, BtnStates.Pressed, BtnStates.Default, ButtonPredicates.IsNotHovered);

        //sm.AddDirectTimedTransition(0, BtnStates.Pressed, BtnStates.Default, 0.5f);
    }
    

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (disabled) return;
        if (ShowEnter)
        {
            enterStateData.sharedBtnStateData.isHovered.Set(true);
            sm.TryPollTransitions(ref enterStateData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (disabled) return;
        if(ShowClick) clickStateData.sharedBtnStateData.isClicked.Set(false);
        if (ShowExit)
        {
            exitStateData.sharedBtnStateData.isHovered.Set(false);
            if(ShowExit) sm.TryPollTransitions(ref exitStateData);   
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)          
    {                     
        if (disabled) return;
        if (ShowClick)
        {
            clickStateData.sharedBtnStateData.isClicked.Set(true);
            sm.TryPollTransitions(ref clickStateData);  
            clickStateData.sharedBtnStateData.isClicked.Set(false);
        }
    }
    
    
    
    public void ClickedToDefaultManual()
    {
        if (disabled) return;
        if (!requireClickToDefault)
        {
            Debug.LogWarning("ClickedToDefaultManual was called, but requireClickToDefault is false. This method should only be used if requireClickToDefault is true, as it relies on the IsClickedToDefault predicate to transition back to the default state.");
            return;
        }
        
        exitStateData.sharedBtnStateData.clickedToDefault.Set(true);
        sm.TryPollTransitions(ref exitStateData);   
        exitStateData.sharedBtnStateData.clickedToDefault.Set(false);
    }
    
    
    void OnDestroy()
    {
        sharedBtnState.ManagedBtn.Free();
        sm.Dispose();

        // MUST dispose these to free the persistent pointer arrays
        enterLogics.Dispose();
        exitLogics.Dispose();
        clickLogics.Dispose();
    }
    
}
