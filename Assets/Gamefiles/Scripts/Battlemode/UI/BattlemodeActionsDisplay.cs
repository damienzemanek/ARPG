using System;
using System.Collections.Generic;
using System.Linq;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BattlemodeActionsDisplay : MonoBehaviour
{
    const string k_LevelFormat = "Lvl {0}/60";
    
    [Required] public GameObject GUI;
    
    [Required] public GameObject actionDisplayPrefab;
    [Required] public Transform actionDisplayParent;

    public RectTransform combatDisplayRect;

    [BoxGroup("Special Effects")] [Required] public GameObject endTurnBtn;
    
    [BoxGroup("Status Effects")] [Required] public GameObject statusEffectCurrentDisplayPrefab;
    [BoxGroup("Status Effects")] [Required] public Transform statusEffectCurrentDisplayParentTransform;
    [BoxGroup("Status Effects")] [ReadOnly, ShowInInspector] Pooled<BattlemodeEffect> statusEffectCurrentDisplayPool;
    
    [BoxGroup("Status Effects")] [Required] public GameObject statusEffectHPBarDisplayPrefab;
    [BoxGroup("Status Effects")] [Required] public Transform statusEffectHPBarDisplayParent;
    [BoxGroup("Status Effects")] [ReadOnly, ShowInInspector] Pooled<BattlemodeStatusEffectHPBar> statusEffectHPBarDisplayPool;
    
    [BoxGroup("Status Effects")] [Required] public GameObject effectDisplayPrefab;
    [BoxGroup("Status Effects")] [Required] public Transform effectDisplayParent;
    [BoxGroup("Status Effects")] [ReadOnly, ShowInInspector] Pooled<BattlemodeEffect> effectDisplayPool;
    
    [BoxGroup("Special Effects")] [Required] public GameObject specialEffectDisplayPrefab;
    [BoxGroup("Special Effects")] [Required] public Transform specialEffectDisplayParent;
    [BoxGroup("Special Effects")] [ReadOnly, ShowInInspector] Pooled<BattlemodeSpecialEffect> specialEffectDisplayPool;

    [ReadOnly, ShowInInspector] public BattleTile currentlySelectedTile = null;
    [ReadOnly, ShowInInspector] public BattlemodeActionCtx currentlySelectedAction = null;

    [SerializeField, HideInInspector] int _maxAmountOfTotalActionsAvaliable;
    [ShowInInspector] public int maxAmountOfTotalActionsAvaliable 
    { 
        get => _maxAmountOfTotalActionsAvaliable;
        set 
        {
            _maxAmountOfTotalActionsAvaliable = value;
            Array.Resize(ref actionSlots, value);
        }
    }
    
    [BoxGroup("BattleInfo: Info")] [Required] public TextMeshProUGUI txt_CharacterName;
    [BoxGroup("BattleInfo: Info")] [Required] public TextMeshProUGUI txt_CharacterLevel;
    [BoxGroup("BattleInfo: Info")] [Required] public TextMeshProUGUI txt_CharacterHpNum;
    [BoxGroup("BattleInfo: Info")] [Required] public TextMeshProUGUI txt_CharacterMaxHpNum;
    [BoxGroup("BattleInfo: Info")] [Required] public GameObject apValues;
    [BoxGroup("BattleInfo: Info")] [Required] public TextMeshProUGUI txt_CharacterAPNum;
    [BoxGroup("BattleInfo: Info")] [Required] public TextMeshProUGUI txt_CharacterMaxAPNum;
    [BoxGroup("BattleInfo: Info")] [Required] public TextMeshProUGUI txt_CharacterArmorNum;
    [BoxGroup("BattleInfo: Info")] [Required] public TextMeshProUGUI txt_CharacterMaxArmorNum;
    [BoxGroup("BattleInfo: Info")] [Required] public TextMeshProUGUI txt_CharacterDmgNum;
    
    [BoxGroup("BattleInfo: Action")] [Required] public TextMeshProUGUI txt_ActionName;
    [BoxGroup("BattleInfo: Action")] [Required] public TextMeshProUGUI txt_ActionDescription;
    [BoxGroup("BattleInfo: Action")] [Required] public TextMeshProUGUI txt_PlayerAPnum;
    // Future: Button For Disabling Attacks
    [BoxGroup("BattleInfo: Enemy")] [Required] public TextMeshProUGUI txt_EnemyActionIntentionsNum;
    [BoxGroup("BattleInfo: Enemy")] [Required] public TextMeshProUGUI txt_EnemyPredictedsNum;
    [BoxGroup("BattleInfo: Enemy")] [Required] public GameObject displ_IntentionsVLG;
    [BoxGroup("BattleInfo: Enemy")] [Required] public GameObject displ_IntentionsNumGO;
    [BoxGroup("BattleInfo: Enemy")] [Required] public GameObject displ_PredictedNumGO;
    [BoxGroup("BattleInfo: Enemy")] [Required] public EnemyIntentions enemyIntentions;
    
    [ReadOnly] public BattlemodeAction[] actionSlots = Array.Empty<BattlemodeAction>();
    [Required, SerializeField] BattlemodeAction moveAction;
    [Required, SerializeField] BattlemodeActionConfig moveActionConfig;

    void Awake()
    {
        // Status Effects Setup
        effectDisplayPool = new(() => {
                var ret = Instantiate(effectDisplayPrefab, effectDisplayParent).Get<BattlemodeEffect>();
                ret.gameObject.SetActive(false);
                return ret;
            },
            (effect) => effect.gameObject.SetActive(true),
            effectDisplay => effectDisplay.Hide(),
            12);
        effectDisplayPool.Prewarm(5);
        effectDisplayPool.ReleaseAll();

        // Special Effects Setup
        specialEffectDisplayPool = new(() => {
                var ret = Instantiate(specialEffectDisplayPrefab, specialEffectDisplayParent)
                    .Get<BattlemodeSpecialEffect>();
                ret.gameObject.SetActive(false);
                return ret;
            },
            (effect) => effect.gameObject.SetActive(true),
            specialEffectDisplay => specialEffectDisplay.Hide(),
            3);
        specialEffectDisplayPool.Prewarm(3);
        specialEffectDisplayPool.ReleaseAll();
        
        // Status Effect HP Bar Setup
        statusEffectHPBarDisplayPool = new(() => {
            var ret = Instantiate(statusEffectHPBarDisplayPrefab, statusEffectHPBarDisplayParent).Get<BattlemodeStatusEffectHPBar>();
            ret.gameObject.SetActive(false);
            return ret; 
            },
            (effect) => effect.gameObject.SetActive(true),
            statusEffectHPBarDisplay => statusEffectHPBarDisplay.Hide(),
            8);
        statusEffectHPBarDisplayPool.Prewarm(3);
        statusEffectHPBarDisplayPool.ReleaseAll();
        
        // Status Effect Current Display Setup
        statusEffectCurrentDisplayPool = new(() => {
            var ret = Instantiate(statusEffectCurrentDisplayPrefab, statusEffectCurrentDisplayParentTransform).Get<BattlemodeEffect>();
            ret.gameObject.SetActive(false);
            return ret; 
            },
            (effect) => effect.gameObject.SetActive(true),
            statusEffectCurrentDisplay => statusEffectCurrentDisplay.Hide(),
            8);
        statusEffectCurrentDisplayPool.Prewarm(5);
        statusEffectCurrentDisplayPool.ReleaseAll();
        
        
        GUI.SetActive(false);
        currentlySelectedTile = null;

        // Reset Action Slots
        actionDisplayParent.Children().DestroyAll();
        // Populate Fresh Actions
        for (int i = 0; i < maxAmountOfTotalActionsAvaliable; i++)
        {
            actionSlots[i] = Instantiate(actionDisplayPrefab, actionDisplayParent).Get<BattlemodeAction>();
            actionSlots[i].actionsDisplay.Inject(this);
        }

        moveAction.actionsDisplay.Inject(this);
        var moveActionCtx = moveActionConfig.GenerateActionCtx(
            BattlemodeActionCtx.Status.Acting,
            null,
            null,
            1);
        moveAction.InitAction(moveActionCtx);
    }
    

    // Future: Reset most recetly selected action on battle complete
    public void ShowDisplay(BattleTile tile)
    {
        Debug.Log("Showing display");
        if (tile.occupantCtx.cfg is not BattlerConfig battlerConfig) return;

        currentlySelectedTile = tile; // Grab
        effectDisplayPool.ReleaseAll(); // Reset Effect Display
        specialEffectDisplayPool.ReleaseAll();
        GUI.SetActive(true); // Show GUI
        endTurnBtn.SetActive(false);
        bool foundFirstAction = false;
        BattlemodeAction firstAction = null;
        
        Debug.Log("A");
        
        // Action Slot Enabling
        for (int i = 0; i < maxAmountOfTotalActionsAvaliable; i++)
        {
            bool hasActionSlot = i < battlerConfig.equippedActions.Length;
            bool withinHandSize = i < battlerConfig.maxEquippableActions;

            if (!hasActionSlot || !withinHandSize) continue;

            var action = battlerConfig.equippedActions[i];
            if (action == null) continue;

            if(tile.occupantCtx is not BattlerOccupantCtx battlerOccupantCtx) continue;
            
            var actionCtx = action.GenerateActionCtx(
                BattlemodeActionCtx.Status.Acting,
                battlerOccupantCtx,
                null);

            actionSlots[i].gameObject.SetActive(true);
            actionSlots[i].InitAction(actionCtx);

            if (foundFirstAction) continue;
            foundFirstAction = true;
            firstAction = actionSlots[i];
        }
        
        Debug.Log("B");

        // Info
        txt_CharacterName.text = battlerConfig.occupantName;
        txt_CharacterLevel.text = string.Format(k_LevelFormat, battlerConfig.currentLevel);
        txt_CharacterDmgNum.text = battlerConfig.damage.ToString();
        if (tile.occupantCtx is CharacterOccupantCtx characterCtx)
        {
            Debug.Log("B1");

            apValues.gameObject.SetActive(true);
            txt_CharacterAPNum.text = characterCtx.currentAP.ToString();
            txt_CharacterMaxAPNum.text = "/" + characterCtx.maxAP.ToString();
            txt_CharacterArmorNum.text = characterCtx.currentArmor.ToString();
            txt_CharacterMaxArmorNum.text = "/" + characterCtx.maxArmor.ToString();
            txt_CharacterHpNum.text = characterCtx.currentHp.ToString();
            txt_CharacterMaxHpNum.text = "/" + characterCtx.maxHp.ToString();
            ShowSpecialEffects(characterCtx);
            characterCtx.ResolveBeforeActingEffects(actionSlots);
            ShowCurrentStatusEffectsFromOccupantCtx(characterCtx);
            
            displ_IntentionsVLG.SetActive(false);
            displ_IntentionsNumGO.SetActive(false);
            displ_PredictedNumGO.SetActive(false);
            moveAction.gameObject.SetActive(true);
            Debug.Log("B2");

        }
        else if (tile.occupantCtx is EnemyOccupantCtx enemyOccupantCtx)
        {
            apValues.gameObject.SetActive(false);
            txt_CharacterArmorNum.text = enemyOccupantCtx.currentArmor.ToString();
            txt_CharacterMaxArmorNum.text = "/" + enemyOccupantCtx.maxArmor.ToString();
            txt_CharacterHpNum.text = enemyOccupantCtx.currentHp.ToString();
            txt_CharacterMaxHpNum.text = "/" + enemyOccupantCtx.maxHp.ToString();
            ShowSpecialEffects(enemyOccupantCtx);
            enemyOccupantCtx.ResolveBeforeActingEffects(actionSlots);
            ShowCurrentStatusEffectsFromOccupantCtx(enemyOccupantCtx);
            
            enemyIntentions.DisplayIntentions(enemyOccupantCtx.queuedActions);
            enemyIntentions.HideUnpredictedIntentions(enemyOccupantCtx.currentIntentUsageCtx.intentionsAmount, enemyOccupantCtx.currentPredicteds);
            txt_EnemyActionIntentionsNum.text = enemyOccupantCtx.currentIntentUsageCtx.intentionsAmount.ToString();
            txt_EnemyPredictedsNum.text = enemyOccupantCtx.currentPredicteds.ToString();
            displ_IntentionsVLG.SetActive(true);
            displ_IntentionsNumGO.SetActive(true);
            displ_PredictedNumGO.SetActive(true);
            moveAction.gameObject.SetActive(false);
        }
        else if (tile.occupantCtx is BattlerOccupantCtx battlerCtx)
        {
            apValues.gameObject.SetActive(false);
            txt_CharacterArmorNum.text = battlerCtx.currentArmor.ToString();
            txt_CharacterMaxArmorNum.text = "/" + battlerCtx.maxArmor.ToString();
            txt_CharacterHpNum.text = battlerCtx.currentHp.ToString();
            txt_CharacterMaxHpNum.text = "/" + battlerCtx.maxHp.ToString();
            ShowSpecialEffects(battlerCtx);
            battlerCtx.ResolveBeforeActingEffects(actionSlots);
            ShowCurrentStatusEffectsFromOccupantCtx(battlerCtx);
            moveAction.gameObject.SetActive(false);
        }
        
        Debug.Log("Display 1");
        // Recently Selected Action Setup
        ShowAction(tile, firstAction.actionCtx);
        combatDisplayRect.RefreshLayoutGroupsImmediateAndRecursive();
    }

    public void ShowAction(BattleTile tile, BattlemodeActionCtx actionCtx)
    {
        Debug.Log("Trying to show an action");
        if (actionCtx == null)
        {
            Debug.Log("No action context provided");
            return;
        }
        Debug.Log("Showing action: " + actionCtx.cfg.actionName);
        // Populate Action Display Info
        if(tile.occupantCtx.cfg is EnemyConfig ec)          
            UpdateEnemyActionInfo(actionCtx, ec, tile.occupantCtx);
        else if(tile.occupantCtx.cfg is CharacterConfig cc) 
            UpdatePlayerActionInfo(actionCtx, cc, tile.occupantCtx);

        txt_ActionName.text = actionCtx.cfg.actionName;
        txt_ActionDescription.text = actionCtx.cfg.description;
        
        ShowEffects(actionCtx?.cfg);
        combatDisplayRect.RefreshLayoutGroupsImmediateAndRecursive();
        currentlySelectedAction = actionCtx;
    }


    public void UpdateEnemyActionInfo(BattlemodeActionCtx actionCtx, EnemyConfig enemyConfig, OccupantCtx occupantCtx)
    {
        txt_EnemyActionIntentionsNum.text = enemyConfig.intentions.ToString();
        txt_EnemyPredictedsNum.text = enemyConfig.defaultPredicted.ToString();
    }
    
    
    public void UpdatePlayerActionInfo(BattlemodeActionCtx actionCtx, CharacterConfig characterConfig, OccupantCtx occupantCtx)
    {
        var resolvedApCost = actionCtx.ap + actionCtx.apDelta;
        txt_PlayerAPnum.text = resolvedApCost.ToString();
    }
    
    
    public void ShowEffects(BattlemodeActionConfig actionConfig)
    {
        // Populate Effects
        effectDisplayPool.ReleaseAll();
        
        // Little bit unoptimized oh well, this creates a new effect ctx for the effects of an attack
        // effect ctx is a class so this churns gc. fine for now.
        // effect ctx holds state (stacks amount), could separate later. although locality of info is desired
        for (int i = 0; i < actionConfig.effectsToApplyToTarget.Count; i++)
            effectDisplayPool.Get().PopulateEffect(actionConfig.effectsToApplyToTarget[i].CreateNewEffectInstance());
        combatDisplayRect.RefreshLayoutGroupsImmediateAndRecursive();
    }

    public void ShowCurrentStatusEffectsFromOccupantCtx(BattlerOccupantCtx battlerOccupantCtx)
    {
        statusEffectHPBarDisplayPool.ReleaseAll();
        statusEffectCurrentDisplayPool.ReleaseAll();
        if (battlerOccupantCtx.currentEffects.Count == 0)
        {
            combatDisplayRect.RefreshLayoutGroupsImmediateAndRecursive();
            return;
        }
        
        // Hp bar
        foreach (var statusEffect in battlerOccupantCtx.currentEffects)
            statusEffectHPBarDisplayPool.Get().PopulateEffect(statusEffect);
        
        // Current Effects
        foreach (var statusEffect in battlerOccupantCtx.currentEffects)
            statusEffectCurrentDisplayPool.Get().PopulateEffect(statusEffect);
        
        combatDisplayRect.RefreshLayoutGroupsImmediateAndRecursive();
    }

    public void ShowSpecialEffects(BattlerOccupantCtx battlerOccupantCtx)
    {
        specialEffectDisplayPool.ReleaseAll();
        foreach (var effect in battlerOccupantCtx.specialEffects)
            specialEffectDisplayPool.Get().PopulateEffect(effect);
        combatDisplayRect.RefreshLayoutGroupsImmediateAndRecursive();
    }

    
    public void HideDisplay()
    {
        currentlySelectedTile = null;
        effectDisplayPool.ReleaseAll();
        effectDisplayPool.ReleaseAll();
        specialEffectDisplayPool.ReleaseAll();
        statusEffectHPBarDisplayPool.ReleaseAll();
        foreach (var slot in actionSlots) slot.Hide();
        GUI.SetActive(false);
    }
    
    public void QueueAction()
    {
        Debug.Log("[BattlemodeActionDisplay] Queueing action");
        if (currentlySelectedTile.occupantCtx is not CharacterOccupantCtx characterOccupantCtx) {
            Debug.Log("[BattlemodeActionDisplay] Cannot use action on non-character occupant");
            return; }
        
        if (currentlySelectedAction != null) 
            BattleTracker.Instance.QueueAction(currentlySelectedTile, currentlySelectedAction);
        else Debug.LogError("[BattlemodeActionDisplay] Using a null currentlySelectedAction");
    }

    public int GetCurrentlySelectedAPCost()
    {
        if (currentlySelectedAction == null) return 0;
        return currentlySelectedAction.ap + currentlySelectedAction.apDelta;
    }
    
    public void ShowEndTurnBtn(bool v) => endTurnBtn.SetActive(v);
}
