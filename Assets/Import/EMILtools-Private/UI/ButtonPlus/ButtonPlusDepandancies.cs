using System;
using System.Threading;
using ProArchitecture.Data;
using ProArchitecture.Logic;
using ProArchitecture.Predicates;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public static class ButtonPlusDepandancies
{
    public enum BtnStates { Default, Hover, Pressed }

    [Flags]
    public enum Callbacks
    {
        None = 0,
        SetActive = 1 << 0,
        ChildsDeactivateKeepSelfActive = 1 << 1,
        ButtonUnityEvent = 1 << 2,
        Animate = 1 << 3,
        PlaySound = 1 << 4
    }
    
    [Flags]
    public enum BtnEvent
    {
        None = 0,
        Enter = 1 << 0,
        Exit = 1 << 1,
        Click = 1 << 2,
    }

    
    [Serializable] 
    public struct SetActive
    {
        public bool active;
        public GameObject target;
    }
    [Serializable] 
    public struct ChildsDeactivateKeepSelfActive
    {
        public GameObject activeChild;
    }

    [Serializable]
    public struct Animate
    {
        public Animator animator;
        public string animName;
    }
    
    [Serializable]
    public struct BtnUnityEvent
    {
        public UnityEvent unityEvent;
    }
    
    [Serializable]
    public struct PlaySound
    {
        public AudioSource audioSource;
        public AudioClip audioClip;
        public bool looping;
    }
    
    public struct BtnData
    {
        public struct SharedBtnState
        {
            public ByteBool isHovered;    
            public ByteBool isClicked;     
            public ByteBool clickedToDefault;  
            public BlittableReference<ButtonPlus> ManagedBtn;
        }
        public BlittableReference<ButtonPlus> managedBtn;
        public ref SharedBtnState sharedBtnStateData => ref managedBtn.Target.sharedBtnState;
        public BtnEvent btnEventType;
    }
    
    [Serializable]
    public struct BtnReferences
    {
        public Callbacks Callbacks;

        bool ShowSetActive => (Callbacks & Callbacks.SetActive) != 0;
        bool ShowDeactive => (Callbacks & Callbacks.ChildsDeactivateKeepSelfActive) != 0;
        bool ShowAnimate => (Callbacks & Callbacks.Animate) != 0;
        bool ShowBtnUnityEvent => (Callbacks & Callbacks.ButtonUnityEvent) != 0;
        bool ShowPlaySound => (Callbacks & Callbacks.PlaySound) != 0;

        [ShowIf(nameof(ShowSetActive))] public SetActive SetActiveData;
        [ShowIf(nameof(ShowDeactive))] public ChildsDeactivateKeepSelfActive childsDeactivateKeepSelfActiveData;
        [ShowIf(nameof(ShowAnimate))] public Animate AnimateData;
        [ShowIf(nameof(ShowBtnUnityEvent))] public BtnUnityEvent BtnUnityEvent;
        [ShowIf(nameof(ShowPlaySound))] public PlaySound PlaySound;
    }
    
    public static unsafe class ButtonPredicates
    {   
        // Little verbose, but oh well
        public static Predicate IsNotHovered = new(&isHovered, true);
        public static Predicate IsHovered = new(&isHovered);
        static bool isHovered(void* ptr)
        {
            BtnData* data = (BtnData*)ptr;
            return data->sharedBtnStateData.isHovered.active;
        }
        
        public static Predicate IsClickedToDefault = new(&isClickedToDefault);
        static bool isClickedToDefault(void* ptr)
        {
            BtnData* data = (BtnData*)ptr;
            return data->sharedBtnStateData.clickedToDefault.active;
        }

        public static Predicate IsClicked = new(&isClicked);
        static bool isClicked(void* ptr)
        {
            BtnData* data = (BtnData*)ptr;
            return data->sharedBtnStateData.isClicked.active;
        }
        
    }
    
    public static unsafe class ButtonPlusOperations
    {
        public static Operation<BtnData> SetActive = new(&SetActiveRun, &SetActiveShouldRun);
        static bool SetActiveShouldRun(BtnData* data) => true;
        static void SetActiveRun(BtnData* data)
        {
            var btn = data->sharedBtnStateData.ManagedBtn.Target;
            BtnReferences refs = data->btnEventType switch
            {
                BtnEvent.Enter => btn.enterRefs,
                BtnEvent.Exit => btn.exitRefs,
                BtnEvent.Click => btn.clickRefs,
                _ => btn.exitRefs
            };
            if (refs.SetActiveData.target != null)
                refs.SetActiveData.target.SetActive(refs.SetActiveData.active);
        }
        
        public static Operation<BtnData> DeactiveAllChildrenButKeepOAnective = new(&DeactiveAllChildrenButKeepOAnectiveRun, &DeactiveAllChildrenButKeepOAnectiveShouldRun);
        static bool DeactiveAllChildrenButKeepOAnectiveShouldRun(BtnData* data) => true;
        static void DeactiveAllChildrenButKeepOAnectiveRun(BtnData* data)
        {
            var btn = data->sharedBtnStateData.ManagedBtn.Target;
            BtnReferences refs = data->btnEventType switch
            {
                BtnEvent.Enter => btn.enterRefs,
                BtnEvent.Exit => btn.exitRefs,
                BtnEvent.Click => btn.clickRefs,
                _ => btn.exitRefs
            };
            
            var dataRef = refs.childsDeactivateKeepSelfActiveData;
            if (dataRef.activeChild == null) return;
            
            var parent = dataRef.activeChild.transform.parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                child.gameObject.SetActive(child.gameObject == dataRef.activeChild);
            }
        }
        
        public static Operation<BtnData> Animate = new(&AnimateRun, &AnimateShouldRun);
        static bool AnimateShouldRun(BtnData* data) => true;
        static void AnimateRun(BtnData* data)
        {
            var btn = data->sharedBtnStateData.ManagedBtn.Target;
            BtnReferences refs = data->btnEventType switch
            {
                BtnEvent.Enter => btn.enterRefs,
                BtnEvent.Exit => btn.exitRefs,
                BtnEvent.Click => btn.clickRefs,
                _ => btn.exitRefs
            };
            
            var dataRef = refs.AnimateData;
            if (dataRef.animator == null) throw new Exception("Animator is null");
            Debug.Log("Playing animation " + dataRef.animName);
            dataRef.animator.Play(dataRef.animName);
        }
        
        public static Operation<BtnData> BtnUnityEvent = new(&BtnUnityEventRun, &BtnUnityEventShouldRun);
        static bool BtnUnityEventShouldRun(BtnData* data) => true;
        static void BtnUnityEventRun(BtnData* data)
        {
            var btn = data->sharedBtnStateData.ManagedBtn.Target;
            BtnReferences refs = data->btnEventType switch
            {
                BtnEvent.Enter => btn.enterRefs,
                BtnEvent.Exit => btn.exitRefs,
                BtnEvent.Click => btn.clickRefs,
                _ => btn.exitRefs
            };
            
            var dataRef = refs.BtnUnityEvent;
            if (dataRef.unityEvent == null) throw new Exception("UnityEvent is null");
            dataRef.unityEvent.Invoke();
        }
        
        public static Operation<BtnData> PlaySound = new(&PlaySoundRun, &PlaySoundShouldRun);
        static bool PlaySoundShouldRun(BtnData* data) => true;
        static void PlaySoundRun(BtnData* data)
        {
            var btn = data->sharedBtnStateData.ManagedBtn.Target;
            BtnReferences refs = data->btnEventType switch
            {
                BtnEvent.Enter => btn.enterRefs,
                BtnEvent.Exit => btn.exitRefs,
                BtnEvent.Click => btn.clickRefs,
                _ => btn.exitRefs
            };
            
            var dataRef = refs.PlaySound;
            if (dataRef.audioSource == null) throw new Exception("AudioSource is null");
            if (dataRef.audioClip == null) throw new Exception("AudioClip is null");
            dataRef.audioSource.loop = dataRef.looping;
            if(!dataRef.looping) dataRef.audioSource.PlayOneShot(dataRef.audioClip);
            else
            {
                dataRef.audioSource.clip = dataRef.audioClip;
                dataRef.audioSource.Play();
            }
        }
    }
}
