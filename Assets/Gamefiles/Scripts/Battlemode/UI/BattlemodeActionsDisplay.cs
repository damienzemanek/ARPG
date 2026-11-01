using System;
using System.Collections.Generic;
using System.Linq;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BattlemodeActionsDisplay : MonoBehaviour
{
    public enum ActionDisplayType
    {
        Hand,
        Exhausted
    }
    
    const string k_LevelFormat = "Lvl {0}/60";
    
    [BoxGroup("UI Refs")] [Required] public GameObject GUI;
    [BoxGroup("UI Refs")] [Required] public GameObject actionDisplayPrefab;
    [Required] public Transform actionsHandDisplayParent;
    [Required] public Transform actionsExhaustedDisplayParent;
    [Required] public GameObject displ_ActionInfo;
    [BoxGroup("Action Info Display")] [Required] public Transform parent_roleTargetInfo;
    [FormerlySerializedAs("pr_hlg_targetingDetail")] [BoxGroup("Action Info Display")] [Required] public GameObject pr_hlg_infoDetail;
    [BoxGroup("Action Info Display")] [Required] public GameObject pr_vlg_roleTargetInfo;



    

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
    [BoxGroup("BattleInfo: Action")] [Required] public TextMeshProUGUI txt_PlayerAPnum;
    [BoxGroup("BattleInfo: Action")] [Required] public List<ActionRangeTile> actionRangeTiles = new();

    
    // Future: Button For Disabling Attacks
    [BoxGroup("BattleInfo: Enemy")] [Required] public TextMeshProUGUI txt_EnemyActionIntentionsNum;
    [BoxGroup("BattleInfo: Enemy")] [Required] public TextMeshProUGUI txt_EnemyPredictedsNum;
    [BoxGroup("BattleInfo: Enemy")] [Required] public GameObject displ_IntentionsVLG;
    [BoxGroup("BattleInfo: Enemy")] [Required] public GameObject displ_IntentionsNumGO;
    [BoxGroup("BattleInfo: Enemy")] [Required] public GameObject displ_PredictedNumGO;
    [BoxGroup("BattleInfo: Enemy")] [Required] public EnemyIntentions enemyIntentions;
    
    [ReadOnly] public BattlemodeAction[] actionSlots = Array.Empty<BattlemodeAction>();
    [ReadOnly] public List<BattlemodeAction> exhuastedActionSlots = new();

    [Required, SerializeField] BattlemodeAction moveAction;
    [Required, SerializeField] public BattlemodeActionConfig moveActionConfig;

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
        actionsHandDisplayParent.Children().DestroyAll();
        // Populate Fresh Actions
        for (int i = 0; i < maxAmountOfTotalActionsAvaliable; i++)
            actionSlots[i] = InitActionSlot(ActionDisplayType.Hand);
        
        moveAction.actionsDisplay.Inject(this);
        var moveActionCtx = moveActionConfig.GenerateActionCtx(
            BattlemodeActionCtx.Status.Acting,
            null,
            null,
            1);
        moveAction.InitAction(moveActionCtx);
    }

    BattlemodeAction InitActionSlot(ActionDisplayType type)
    {
        var parent = type == ActionDisplayType.Hand ? actionsHandDisplayParent : actionsExhaustedDisplayParent;
        var actionSlot = Instantiate(actionDisplayPrefab, parent).Get<BattlemodeAction>();
        actionSlot.actionsDisplay.Inject(this);
        return actionSlot;
    }
    

    // Future: Reset most recetly selected action on battle complete
    public void ShowDisplay(BattleTile tile)
    {
        if (tile.occupantCtx.cfg is not BattlerConfig battlerConfig) return;
        Debug.Log("Showing Display for Occupant: " + tile.occupantCtx.cfg.name);

        currentlySelectedTile = tile; // Grab
        effectDisplayPool.ReleaseAll(); // Reset Effect Display
        specialEffectDisplayPool.ReleaseAll();
        ClearExhuastedActionSlots();
        ShowActionsDisplay(ActionDisplayType.Hand);
        GUI.SetActive(true); // Show GUI
        displ_ActionInfo.SetActive(true);
        endTurnBtn.SetActive(false);
        bool foundFirstHandAction = false;
        BattlemodeAction firstAction = null;
        
        // Action Slot Enabling
        for (int i = 0; i < maxAmountOfTotalActionsAvaliable; i++)
        {
            bool hasActionSlot = i < battlerConfig.equippedActions.Length;
            bool withinHandSize = i < battlerConfig.maxEquippableActions;

            if (!hasActionSlot || !withinHandSize) continue;

            var actionCfg = battlerConfig.equippedActions[i];
            if (actionCfg == null) continue;

            if(tile.occupantCtx is not BattlerOccupantCtx battlerOccupantCtx) continue;
            if (battlerOccupantCtx.exhuastedActionCfgs.Contains(actionCfg))
            {
                var exhuastedAction = InitActionSlot(ActionDisplayType.Exhausted);
                exhuastedActionSlots.Add(exhuastedAction);
                var exhuastedActionCtx = actionCfg.GenerateActionCtx(
                    BattlemodeActionCtx.Status.Acting,
                    battlerOccupantCtx,
                    null);
                exhuastedAction.gameObject.SetActive(true);
                exhuastedAction.InitAction(exhuastedActionCtx);
                continue;
            }
            
            var actionCtx = actionCfg.GenerateActionCtx(
                BattlemodeActionCtx.Status.Acting,
                battlerOccupantCtx,
                null);

            actionSlots[i].gameObject.SetActive(true);
            actionSlots[i].InitAction(actionCtx);
            CheckRangeMutation(actionCtx, tile, actionSlots[i], out _, out _);

            if (foundFirstHandAction) continue;
            foundFirstHandAction = true;
            firstAction = actionSlots[i];
        }
        
        // Move Action Reset
        var moveActionCtx = moveActionConfig.GenerateActionCtx(
            BattlemodeActionCtx.Status.Acting,
            null,
            null,
            1);
        moveAction.InitAction(moveActionCtx);
        

        // Info
        txt_CharacterName.text = battlerConfig.occupantName;
        txt_CharacterLevel.text = string.Format(k_LevelFormat, battlerConfig.currentLevel);
        txt_CharacterDmgNum.text = battlerConfig.damage.ToString();
        if (tile.occupantCtx is CharacterOccupantCtx characterCtx)
        {
            apValues.gameObject.SetActive(true);
            txt_CharacterAPNum.text = characterCtx.currentAP.ToString();
            txt_CharacterMaxAPNum.text = "/" + characterCtx.maxAP.ToString();
            txt_CharacterArmorNum.text = characterCtx.currentArmor.ToString();
            txt_CharacterMaxArmorNum.text = "/" + characterCtx.maxArmor.ToString();
            txt_CharacterHpNum.text = characterCtx.currentHp.ToString();
            txt_CharacterMaxHpNum.text = "/" + characterCtx.maxHp.ToString();
            ShowSpecialEffects(characterCtx);
            characterCtx.PreResolveBeforeActingEffectsPlayer(actionSlots);
            ShowCurrentStatusEffectsFromOccupantCtx(characterCtx);
            
            displ_IntentionsVLG.SetActive(false);
            displ_IntentionsNumGO.SetActive(false);
            displ_PredictedNumGO.SetActive(false);
            moveAction.gameObject.SetActive(true);
        }
        else if (tile.occupantCtx is EnemyOccupantCtx enemyOccupantCtx)
        {
            apValues.gameObject.SetActive(false);
            txt_CharacterArmorNum.text = enemyOccupantCtx.currentArmor.ToString();
            txt_CharacterMaxArmorNum.text = "/" + enemyOccupantCtx.maxArmor.ToString();
            txt_CharacterHpNum.text = enemyOccupantCtx.currentHp.ToString();
            txt_CharacterMaxHpNum.text = "/" + enemyOccupantCtx.maxHp.ToString();
            ShowSpecialEffects(enemyOccupantCtx);
            enemyOccupantCtx.PreResolveBeforeActingEffectsPlayer(actionSlots);
            ShowCurrentStatusEffectsFromOccupantCtx(enemyOccupantCtx);
            
            enemyIntentions.DisplayIntentions(enemyOccupantCtx.queuedActions.ToList());
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
            battlerCtx.PreResolveBeforeActingEffectsPlayer(actionSlots);
            ShowCurrentStatusEffectsFromOccupantCtx(battlerCtx);
            moveAction.gameObject.SetActive(false);
        }
        
        // Recently Selected Action Setup
        if(firstAction != null && firstAction.actionCtx != null)
            ShowAction(tile, firstAction);
    }

    void ClearExhuastedActionSlots()
    {
        if(exhuastedActionSlots.Count <= 0) return;
        foreach (var exhuastedActionSlot in exhuastedActionSlots) exhuastedActionSlot.Hide();
        exhuastedActionSlots.Clear();
    }

    public void ShowAction(BattleTile tile, BattlemodeAction actionSlot)
    {
        var actionCtx = actionSlot.actionCtx;
        if (actionCtx == null) { Debug.Log("No action context provided"); return; }
        
        Debug.Log("Showing action: " + actionCtx.cfg.actionName);
        // Populate Action Display Info
        if(tile.occupantCtx.cfg is EnemyConfig ec)          
            UpdateEnemyActionInfo(actionCtx, ec, tile.occupantCtx);
        else if(tile.occupantCtx.cfg is CharacterConfig cc) 
            UpdatePlayerActionInfo(actionCtx, cc, tile.occupantCtx);

        txt_ActionName.text = actionCtx.cfg.actionName;
        
        // Note: Targeting Details before effects
        // hardocoding this avoiding desctruction bc o well need to just ge it working
        foreach (var child in parent_roleTargetInfo.Children())
        {
            child.gameObject.SetActive(false);
            Destroy(child);
        }
        
        CreateRoleTargetInfoAndDetails();
        
        ShowEffects(actionCtx?.cfg);
        currentlySelectedAction = actionCtx;
        
        
        CheckRangeMutation(actionCtx, tile, actionSlot, 
            out var allInRangeTiles, out var allInUsableTiles);
        
        foreach (var t in actionRangeTiles)
        {
            t.ResetTiles();
            if (t.row == tile.row && t.col == tile.col) t.SetUserIndication(true);
            if (t.col == 3) t.SetActionRangeTileState(ActionRangeTile.TileState.Contested);
            if (allInUsableTiles.Any(usable => usable.row == t.row && usable.col == t.col))
            {
                t.SetActionRangeTileState(ActionRangeTile.TileState.Usable);
                Debug.Log("Usable Tile: " + t.row + ", " + t.col);
                continue;
            }
            if (allInRangeTiles.Any(inRange => inRange.row == t.row && inRange.col == t.col))
            {
                t.SetActionRangeTileState(ActionRangeTile.TileState.InRange);
                Debug.Log("InRange Tile: " + t.row + ", " + t.col);
                continue;
            }
        }
        
        combatDisplayRect.RefreshLayoutGroupsImmediateAndRecursive();

        void CreateRoleTargetInfoAndDetails()
        {
            var newRoleTargetInfo = Instantiate(pr_vlg_roleTargetInfo, parent_roleTargetInfo);
            foreach (var targInfoChild in newRoleTargetInfo.transform.Children())
            {
                if (newRoleTargetInfo.transform.GetChild(0).gameObject == targInfoChild) continue;
                targInfoChild.gameObject.SetActive(false);
                Destroy(targInfoChild.gameObject);
            }
            
            var stringTargetingDetails = actionCtx.cfg.GetTargetDetails();
            foreach (var targDetail in stringTargetingDetails)
            {
                var newTargDetail = Instantiate(pr_hlg_infoDetail, newRoleTargetInfo.transform);
                newTargDetail.transform.GetChild(1).Get<TextMeshProUGUI>().text = targDetail; // hardoding this bc o well idc, its the second child
            }
            newRoleTargetInfo.Get<BattlemodeRoleTargetInfo>().SetRoleName("Target: " + actionCtx.cfg.roleTarget);
            
            // Check if there is anything to self if not just ret
            var stringSelfDetails = actionCtx.cfg.GetSelfDetails();
            if(stringSelfDetails.Count == 0) return;
            var newSelfInfo = Instantiate(pr_vlg_roleTargetInfo, parent_roleTargetInfo);
            foreach (var selfInfoChild in newSelfInfo.transform.Children())
            {
                if(newSelfInfo.transform.GetChild(0).gameObject == selfInfoChild) continue;
                selfInfoChild.gameObject.SetActive(false);
                Destroy(selfInfoChild.gameObject);
            }
            
            foreach (var selfDetail in stringSelfDetails)
            {
                var newSelfDetail = Instantiate(pr_hlg_infoDetail, newSelfInfo.transform);
                newSelfDetail.transform.GetChild(1).Get<TextMeshProUGUI>().text = selfDetail;
            }
            newSelfInfo.Get<BattlemodeRoleTargetInfo>().SetRoleName("Self:");
            
        }
    }

    public void CheckRangeMutation(BattlemodeActionCtx actionCtx, BattleTile tile, BattlemodeAction actionSlot,
        out List<BattleTile> allInRangeTiles,
        out List<BattleTile> allInUsableTiles)
    {
        bool inRange = BattleTracker.Instance.IsInTargettableRange(actionCtx, tile, out allInRangeTiles);
        BattleTracker.Instance.GetUsableTiles(actionCtx, tile, out allInUsableTiles);
        
        if (BattleTracker.Instance.IsInUsableTile(actionCtx, tile))
        {
            if(!inRange) 
                actionSlot.RangeMutateAction(BattlemodeAction.RangeMutateActionDisplay.OutOfRange);
        }
        else 
            actionSlot.RangeMutateAction(BattlemodeAction.RangeMutateActionDisplay.Unusable);
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
    
    // TO:DO Ordered Effects so the list is always in the same order
    public void ShowEffects(BattlemodeActionConfig actionConfig)
    {
        // Populate Effects
        effectDisplayPool.ReleaseAll();
        
        // Little bit unoptimized oh well, this creates a new effect ctx for the effects of an attack
        // effect ctx is a class so this churns gc. fine for now.
        // effect ctx holds state (stacks amount), could separate later. although locality of info is desired
        for (int i = 0; i < actionConfig.effectsToApplyToTarget.Count; i++)
            effectDisplayPool.Get().PopulateEffect(actionConfig.effectsToApplyToTarget[i].CreateNewEffectInstance());
        
        for (int i = 0; i < actionConfig.effectsToApplyToSelf.Count; i++)
            effectDisplayPool.Get().PopulateEffect(actionConfig.effectsToApplyToSelf[i].CreateNewEffectInstance());
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
        bool hasSpecialEffects = battlerOccupantCtx.specialEffects.Count > 0;
        specialEffectDisplayPool.ReleaseAll();
        if (hasSpecialEffects)
        {
            specialEffectDisplayParent.gameObject.SetActive(true);
            foreach (var effect in battlerOccupantCtx.specialEffects)
                specialEffectDisplayPool.Get().PopulateEffect(effect);
        }
        else
            specialEffectDisplayParent.gameObject.SetActive(false);
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

        if (currentlySelectedAction == null) {
            Debug.LogError("[BattlemodeActionDisplay] Cannot queue action when no action is selected");
            return; }
        if (currentlySelectedTile.occupantCtx is not CharacterOccupantCtx characterOccupantCtx) {
            Debug.Log("[BattlemodeActionDisplay] Cannot use action on non-character occupant");
            return; }

        if (exhuastedActionSlots.Any(s => s.actionCtx.cfg == currentlySelectedAction.cfg))
            return;
        
        BattleTracker.Instance.QueuePlayerAction(currentlySelectedTile, currentlySelectedAction);
    }

    public int GetCurrentlySelectedAPCost()
    {
        if (currentlySelectedAction == null) return 0;
        return currentlySelectedAction.ap + currentlySelectedAction.apDelta;
    }
    
    public void ShowEndTurnBtn(bool v) => endTurnBtn.SetActive(v);

    public void ShowHandActionsDisplay() => ShowActionsDisplay(ActionDisplayType.Hand);
    public void ShowExhaustedActionsDisplay() => ShowActionsDisplay(ActionDisplayType.Exhausted);
    
    public void ShowActionsDisplay(ActionDisplayType v)
    {
        if (v == ActionDisplayType.Hand)
        {
            actionsHandDisplayParent.parent.gameObject.SetActive(true);
            actionsExhaustedDisplayParent.parent.gameObject.SetActive(false);
            if (actionSlots.Any(s => s.actionCtx != null))
            {
                displ_ActionInfo.SetActive(true);
                ShowAction(currentlySelectedTile, actionSlots.First(s => s.actionCtx != null));
            }
            else 
                displ_ActionInfo.SetActive(false);

        }
        else 
        {
            actionsHandDisplayParent.parent.gameObject.SetActive(false);
            actionsExhaustedDisplayParent.parent.gameObject.SetActive(true);
            if (exhuastedActionSlots.Any(s => s.actionCtx != null))
            {
                displ_ActionInfo.SetActive(true);
                ShowAction(currentlySelectedTile, exhuastedActionSlots.First(s => s.actionCtx != null));
            }
            else 
                displ_ActionInfo.SetActive(false);
        }
    }
}
