using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMILtools.Core;
using EMILtools.Extensions;
using EMILtools.Systems;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
[HideLabel]
public class ConsumeBool
{
    bool value = false;
    public bool TryConsume()
    {
        if (!value) return false;
        value = false;
        return true;
    }
    public bool MarkAsConsumable() => value = true;
}

public class ButtonPlusDepracted : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler
{
    [Flags]
    public enum ButtonTransitions
    {
        Click = 1 << 0,
        Enter = 1 << 1, 
        Exit = 1 << 2,
        Up = 1 << 3,
        Down = 1 << 4
    }
    [Serializable] public class DefaultButtonState : ButtonState { }
    [Serializable] public class ClickButtonState : ButtonState { }
    [Serializable] public class EnterButtonState : ButtonState { }

    [Serializable]
    public class ExitButtonState : ButtonState
    {
        public bool exitToDefault;
    }
    [Serializable] public class UpButtonState : ButtonState { }
    [Serializable] public class DownButtonState : ButtonState { }
    

    public sealed class NoDataFSM : IContextViewImmutable { }
    public StateMachine<NoDataFSM> FSM { get; private set; }
    public ButtonTransitions triggerTransitions;

    public DefaultButtonState defaultState;
    [SerializeReference][ShowIf("@triggerTransitions.HasFlag(ButtonTransitions.Click)")] public ClickButtonState clickState;
    [SerializeReference][ShowIf("@triggerTransitions.HasFlag(ButtonTransitions.Entry)")] public EnterButtonState enterState;
    [SerializeReference][ShowIf("@triggerTransitions.HasFlag(ButtonTransitions.Exit)")] public ExitButtonState exitState;
    [SerializeReference][ShowIf("@triggerTransitions.HasFlag(ButtonTransitions.Up)")] public UpButtonState upState;
    [SerializeReference][ShowIf("@triggerTransitions.HasFlag(ButtonTransitions.Down)")] public DownButtonState downState;
    
    public ConsumeBool consumeClick = new ConsumeBool();
    public ConsumeBool consumeEnter = new ConsumeBool();
    public ConsumeBool consumeExit = new ConsumeBool();
    public ConsumeBool consumeUp = new ConsumeBool();
    public ConsumeBool consumeDown = new ConsumeBool();
    
    bool click => triggerTransitions.HasFlag(ButtonTransitions.Click);
    bool enter => triggerTransitions.HasFlag(ButtonTransitions.Enter);
    bool exit => triggerTransitions.HasFlag(ButtonTransitions.Exit);
    bool up => triggerTransitions.HasFlag(ButtonTransitions.Up);
    bool down => triggerTransitions.HasFlag(ButtonTransitions.Down);
    
    [Serializable]
    public abstract class ButtonState : IState
    {
        public enum DefaultStateEnum { Enter, Exit }
        
        public DefaultStateEnum defaultState;

        [SerializeReference] public List<ButtonBehaviour> behaviours = new();

        public void DefaultState(IContextViewImmutable ctx)
        {
            if (defaultState == DefaultStateEnum.Enter) OnEnterState(ctx);
            else if (defaultState == DefaultStateEnum.Exit) OnExitState(ctx);
        }

        public void OnEnterState(IContextViewImmutable ctx)
        {
            foreach (var behaviour in behaviours) behaviour.OnEnterState();
        }

        public void OnExitState(IContextViewImmutable ctx)
        {
            foreach (var behaviour in behaviours) behaviour.OnExitState();
        }
    }
    

    [Serializable]
    public abstract class ButtonBehaviour
    {
        public bool useOnEnter;
        public bool useOnExit;

        public abstract void OnEnterState();
        public abstract void OnExitState();
    }
    
    [Serializable]
    public class SetActive : ButtonBehaviour
    {
        [Required] public GameObject target;
        public bool delay;
        [ShowIf("delay")] public float delayTime;
        [ShowIf("useOnEnter")] public bool enterActiveState;
        [ShowIf("useOnExit")] public bool exitActiveState;

        public override void OnEnterState()
        {
            if(useOnEnter && !delay) target.SetActive(enterActiveState);
            if(useOnEnter && delay) SetActiveDelayed(target, enterActiveState, delayTime).Forget("BtnPlus SetActive");
        }
        public override void OnExitState()
        {
            if(useOnExit && !delay) target.SetActive(exitActiveState);
            if(useOnExit && delay) SetActiveDelayed(target, enterActiveState, delayTime).Forget("BtnPlus SetActive");
        }

        async Task SetActiveDelayed(GameObject target, bool active, float delayTime)
        {
            await Task.Delay((int)(delayTime * 1000));
            target.SetActive(active);
        }
    }
    
    
    [Serializable]
    public class DeactiveAllChildrenButKeepOAnective : ButtonBehaviour
    {
        [Required] public GameObject activeChild;

        public override void OnEnterState()
        {
            if (!useOnEnter) return;
            foreach (Transform child in activeChild.transform.parent)
                child.gameObject.SetActive(child.gameObject == activeChild);
        }
        public override void OnExitState()
        {
            if (!useOnExit) return;
            foreach (Transform child in activeChild.transform.parent)
                child.gameObject.SetActive(child.gameObject == activeChild);
        }
    }

    [Serializable]
    public class ButtonUnityEvent : ButtonBehaviour
    {
        [ShowIf("useOnEnter")] public UnityEngine.Events.UnityEvent enterEvent;
        [ShowIf("useOnExit")] public UnityEngine.Events.UnityEvent exitEvent;
        
        public override void OnEnterState()
        {
            if(useOnEnter) enterEvent.Invoke();
        }
        public override void OnExitState()
        {
            if(useOnExit) exitEvent.Invoke();
        }
    }

    [Serializable]
    public class Animate : ButtonBehaviour
    {
        [Required] public Animator targetAnimator;
        [ShowIf("useOnEnter")] public string enterAnimName;
        [ShowIf("useOnExit")] public string exitAnimName;

        public override void OnEnterState()
        {
            if(useOnEnter) targetAnimator.Play(enterAnimName, 0);
        }
        public override void OnExitState()
        {
            if(useOnExit) targetAnimator.Play(exitAnimName, 0);
        }
    }
    
    [Serializable]
    public class AudioPlayFromSoundUser : ButtonBehaviour
    {
        [Required] public SoundUser soundUser;
        
   //     [ValueDropdown("GetSoundOptions")]
        public string soundName;
        public bool loop;

      //   private System.Collections.IEnumerable GetSoundOptions()
      //   {
      // //      if (soundUser == null || soundUser.soundConfig == null) return null;
      // //      return soundUser.soundConfig.GetSoundNames();
      //   }

        public override void OnEnterState()
        {
      //      if(useOnEnter && soundUser != null && soundUser.soundConfig != null)
      //          soundUser.soundConfig.Play(soundUser.audioSource, soundName, 1f, loop);
        }
        public override void OnExitState()
        {
       //     if(useOnExit && soundUser != null && soundUser.soundConfig != null)
        //        soundUser.soundConfig.Play(soundUser.audioSource, soundName, 1f, loop);
        }
    }
    
    [Serializable]
    public class DisapearFade : ButtonBehaviour
    {
        [Required] public Transform target;
        public float time;
        public bool stopRaycasts;
        public bool disableAfter;
        
        public override void OnEnterState()
        {
            if (useOnEnter) Fade(0, time);
        }
        public override void OnExitState()
        {
            if (useOnExit) Fade(0, time);
        }

        void Fade(float alpha, float duration)
        {
            var childrenThatAreImages = target.GetComponentsInChildren<Image>(true);
            foreach (var image in childrenThatAreImages)
            {
                Debug.Log("Fading: " + image.gameObject.name);
                image.CrossFadeAlpha(alpha, duration, true);
                if(stopRaycasts) image.raycastTarget = false;
            }

            if (disableAfter && duration > 0)
            {
                var mono = target.GetComponentInParent<MonoBehaviour>();
                if (mono != null)
                {
                    mono.DelayedCall(() => target.gameObject.SetActive(false), duration);
                }
                else target.gameObject.SetActive(false); 

            }
            else if (disableAfter)
                target.gameObject.SetActive(false);
        }

        public void Reset()
        {
            target.gameObject.SetActive(true);
            var childrenThatAreImages = target.GetComponentsInChildren<Image>(true);
            foreach (var image in childrenThatAreImages)
            {
                image.CrossFadeAlpha(1, 0, true);
                image.raycastTarget = true;
            }
        }
    }

    void OnValidate()
    {
        if(click && clickState == null) clickState = new ClickButtonState();
        else if(click == false) clickState = null;
        
        if(enter && enterState == null) enterState = new EnterButtonState();
        else if(enter == false) enterState = null;
        
        if(exit && exitState == null) exitState = new ExitButtonState();
        else if(exit == false) exitState = null;
        
        if(up && upState == null) upState = new UpButtonState();
        else if(up == false) upState = null;
        
        if(down && downState == null) downState = new DownButtonState();
        else if(down == false) downState = null;
    }

    private void Awake()
    {
        FSM = new StateMachine<NoDataFSM>(new NoDataFSM(), defaultState);
        Resolves noResolves = new Resolves();

        if (click)
        {
            FSM.AddNode(clickState);
            var clickPredicate = new FuncPredicate(() => consumeClick.TryConsume());
            FSM.AddAnyTransition(clickState, clickPredicate, "Clicked");
        }

        if (enter)
        {
            FSM.AddNode(enterState);
            var enterPredicate = new FuncPredicate(() => consumeEnter.TryConsume());
            FSM.AddAnyTransition(enterState, enterPredicate, "Entered");
        }

        if (exit)
        {
            var exitPredicate = new FuncPredicate(() => consumeExit.TryConsume());
            if (!exitState.exitToDefault)
            {
                FSM.AddNode(exitState);
                FSM.AddAnyTransition(exitState, exitPredicate, "Exited");
            }
            else
            {
                FSM.AddAnyTransition(defaultState, exitPredicate, "Exited");
            }
        }

        if (up)
        {
            FSM.AddNode(upState);
            var upPredicate = new FuncPredicate(() => consumeUp.TryConsume());
            FSM.AddAnyTransition(upState, upPredicate, "Up");
        }

        if (down)
        {
            FSM.AddNode(downState);
            var downPredicate = new FuncPredicate(() => consumeDown.TryConsume());
            FSM.AddAnyTransition(downState, downPredicate, "Down");
        }
    }
    
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!click) return;
        Debug.Log("Click!");
        consumeClick.MarkAsConsumable();
        FSM.CheckTransitions();

    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!enter) return;
        consumeEnter.MarkAsConsumable();
        FSM.CheckTransitions();
        Debug.Log("Entered!");
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!exit) return;
        consumeExit.MarkAsConsumable();
        FSM.CheckTransitions();
        Debug.Log("Exited!");
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!down) return;
        consumeDown.MarkAsConsumable();
        FSM.CheckTransitions();
        Debug.Log("Down!");
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!up) return;
        consumeUp.MarkAsConsumable();
        FSM.CheckTransitions();
        Debug.Log("Up!");
    }
}
