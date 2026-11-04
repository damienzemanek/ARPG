using System;
using System.Collections.Generic;
using System.Linq;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using static ActionsMenuActionBtn;

public class ActionsMenuActionsDisplay : MonoBehaviour
{
    [ReadOnly] public CharacterConfig inspectingCharacterActions;
    [ReadOnly] public ActionsMenuActionBtn currentlySelectedActionBtn = null;
    [Required] public GameObject go_equipDequipBtn;
    [Required] public TextMeshProUGUI text_buttonLabelEquipDequip;
    
    [BoxGroup("Action Info Display")] [Required] public RectTransform rectActionsDisplay;
    [BoxGroup("Action Info Display")] [Required] public Transform parent_roleTargetInfo;
    [BoxGroup("Action Info Display")] [Required] public GameObject pr_hlg_infoDetail;
    [BoxGroup("Action Info Display")] [Required] public GameObject pr_vlg_roleTargetInfo;
    [BoxGroup("Status Effects")] [Required] public GameObject effectDisplayPrefab;
    [BoxGroup("Status Effects")] [Required] public Transform effectDisplayParent;
    [BoxGroup("Status Effects")] [ReadOnly, ShowInInspector] Pooled<BattlemodeEffect> effectDisplayPool;
    [BoxGroup("BattleInfo: Action")] [Required] public TextMeshProUGUI txt_ActionName;
    [BoxGroup("BattleInfo: Action")] [Required] public TextMeshProUGUI txt_PlayerAPnum;
    [BoxGroup("BattleInfo: Action")] [Required] public List<ActionRangeTile> actionRangeTiles;
    public List<ActionsMenuActionBtn> actionBtns = new();
    
    [Button]
    public void GrabAllBtns(Transform parent) => actionBtns.AddRange(parent.GetComponentsInChildren<ActionsMenuActionBtn>());

    void Awake()
    {
        rectActionsDisplay.gameObject.SetActive(false);
        go_equipDequipBtn.SetActive(false);
        
        // Status Effects Setup
        effectDisplayPool = new(
            createFunc:    () => 
            {
                var ret = Instantiate(effectDisplayPrefab, effectDisplayParent).Get<BattlemodeEffect>();
                ret.gameObject.SetActive(false);
                return ret;
            },
            onGet:         (effect) => effect.gameObject.SetActive(true),
            onRelease:     (effect) => effect.Hide(),
            initialSize:    12);
        effectDisplayPool.Prewarm(5);
        effectDisplayPool.ReleaseAll();
    }

    void OnEnable()
    {
        rectActionsDisplay.gameObject.SetActive(false);
        go_equipDequipBtn.SetActive(false);
        effectDisplayPool.ReleaseAll();
    }   

    public void DisplayActionsFor(CharacterConfig cfg)
    {
        if(actionBtns == null || actionBtns.Count <= 0) this.Log("No ActionBtns are saved to actionsBtn list");
        if(cfg.AllAvaliableActions.Count > actionBtns.Count) Debug.LogError("Max equippable actions greater than action btns amount");

        this.Log("Displaying actions for: " + cfg.occupantName + "");
        actionBtns.ForEach(b => b.ResetSlot());
        currentlySelectedActionBtn = null;
        inspectingCharacterActions = cfg;
        SaverService.Instance.GetSaverAndData<CharactersData_SavedDataSO>(out _, out var charData);
        SaverService.Instance.GetSaverAndData<Items_SavedDataSO>(out _, out var itemData);
        var currEquippedActions = charData.charactersData.Find(c => c.characterConfigName == inspectingCharacterActions.occupantName).equippedActions;
        this.Log("Current equipped actions: " + string.Join(", ", currEquippedActions));

        for (int i = 0; i < inspectingCharacterActions.equippedActions.Length; i++)
            inspectingCharacterActions.equippedActions[i] = null;
        
        for (int i = 0; i < inspectingCharacterActions.AllAvaliableActions.Count; i++)
        {
            actionBtns[i].gameObject.SetActive(true);
            actionBtns[i].InjectAndSetup(inspectingCharacterActions.AllAvaliableActions[i]);
            
            var thisActionsName = inspectingCharacterActions.AllAvaliableActions[i].actionName;
            if (currEquippedActions.Contains(thisActionsName))
            {
                this.Log("Equipping action: " + thisActionsName);
                actionBtns[i].Equip();
                TryEquipActionOnBtn(inspectingCharacterActions.AllAvaliableActions[i], actionBtns[i]);
            }
            else
            {
                this.Log("Dequipping action: " + thisActionsName);
                actionBtns[i].Dequip();
                DequipActionOnBtn(inspectingCharacterActions.AllAvaliableActions[i], actionBtns[i]);
            }
            
            var actionData = itemData.GetItemData(Items_SavedDataSO.ItemType.CharacterAction, thisActionsName);
            if(actionData.amount <= 0)
            {
                this.Log("Locking action: " + thisActionsName + " its value is " + actionData.amount);
                actionBtns[i].Lock();
            }
            
        }
    }

    
    public void SelectAction(ActionsMenuActionBtn actionBtn)
    {
        if(currentlySelectedActionBtn != null) currentlySelectedActionBtn.ForceDefault();
        currentlySelectedActionBtn = actionBtn;
        go_equipDequipBtn.gameObject.SetActive(true);
        if (currentlySelectedActionBtn.state == BtnMenuUIState.Equipped) text_buttonLabelEquipDequip.text = "Dequip";
        else if(currentlySelectedActionBtn.state == BtnMenuUIState.Default) text_buttonLabelEquipDequip.text = "Equip";
        ShowAction(currentlySelectedActionBtn.cfg);
    }

    public void PressedEquipDequip()
    {
        if (currentlySelectedActionBtn.state != BtnMenuUIState.Equipped) TryEquipSelected();
        else DequipSelected();
    }

    public void TryEquipSelected()
    {
        TryEquipActionOnBtn(currentlySelectedActionBtn.cfg, currentlySelectedActionBtn);
        
        SaverService.Instance.GetSaverAndData<CharactersData_SavedDataSO>(out var saver, out var charData);
        charData.charactersData.Find(c => c.characterConfigName == inspectingCharacterActions.occupantName)
            .equippedActions
            .Add(currentlySelectedActionBtn.cfg.actionName);
        
        saver.Save();
    }
    
    public void TryEquipActionOnBtn(BattlemodeActionConfig actionCfg, ActionsMenuActionBtn actionBtn)
    {
        this.Log("Trying to equip action: " + actionCfg.name);
        bool successfullyEquipped = inspectingCharacterActions.TryEquipActionOnNextAvaliableSlot(actionCfg);
        if (!successfullyEquipped) return;
        actionBtn.Equip();
        this.Log("Equipped action: " + actionCfg.name);
    }

    public void DequipSelected()
    {
        DequipActionOnBtn(currentlySelectedActionBtn.cfg, currentlySelectedActionBtn);
        
        SaverService.Instance.GetSaverAndData<CharactersData_SavedDataSO>(out var saver, out var charData);
        charData.charactersData.Find(c => c.characterConfigName == inspectingCharacterActions.occupantName)
            .equippedActions
            .Remove(currentlySelectedActionBtn.cfg.actionName);
        
        saver.Save();
    }
    
    public void DequipActionOnBtn(BattlemodeActionConfig actionCfg, ActionsMenuActionBtn actionBtn)
    {
        inspectingCharacterActions.DequipAction(actionCfg);
        actionBtn.Dequip();
        this.Log("Dequipped action: " + actionCfg.name);
    }
    
    public void ShowAction(BattlemodeActionConfig actionCfg)
    {
        if (actionCfg == null) { Debug.Log("No action provided"); return; }
        rectActionsDisplay.gameObject.SetActive(true);
        effectDisplayPool.ReleaseAll(); // Reset Effect Display
        
        Debug.Log("Showing action: " + actionCfg.actionName);

        txt_PlayerAPnum.text = actionCfg.apCost.ToString();
        txt_ActionName.text = actionCfg.actionName;
        
        foreach (var child in parent_roleTargetInfo.Children())
        {
            child.gameObject.SetActive(false);
            Destroy(child);
        }
        
        CreateRoleTargetInfoAndDetails();
        
        ShowEffects(actionCfg);
        
        bool targetsACurrent = actionCfg.targetingCfg.targetCurrentRow || actionCfg.targetingCfg.targetCurrentCol;
        foreach (var t in actionRangeTiles)
        {
            t.ResetTiles();
            if (t.col == 3) t.SetActionRangeTileState(ActionRangeTile.TileState.Contested);
            BattleTile.ColRank myColRank 
                = t.col is 0 ? BattleTile.ColRank.Left1 
                : t.col is 1 ? BattleTile.ColRank.Left2 
                : t.col is 2 ? BattleTile.ColRank.Left3 
                : t.col is 3 ? BattleTile.ColRank.Center 
                : t.col is 4 ? BattleTile.ColRank.Right1 
                : t.col is 5 ? BattleTile.ColRank.Right2 
                : t.col is 6 ? BattleTile.ColRank.Right3 
                : BattleTile.ColRank.None;
            
            BattleTile.RowRank myRowRank 
                = t.row is 0 ? BattleTile.RowRank.Top
                : t.row is 1 ? BattleTile.RowRank.Middle
                : t.row is 2 ? BattleTile.RowRank.Bottom
                : BattleTile.RowRank.None;
            
            if(actionCfg.targetingCfg.usableInColRanks.HasFlag(myColRank) || actionCfg.targetingCfg.usableInRowRanks.HasFlag(myRowRank))
            {
                t.SetActionRangeTileState(ActionRangeTile.TileState.Usable);
                Debug.Log("Usable Tile: " + t.row + ", " + t.col);
                continue;
            }
            
            if(actionCfg.targetingCfg.targetColRanks.HasFlag(myColRank) || actionCfg.targetingCfg.targetRowRanks.HasFlag(myRowRank))
            {
                t.SetActionRangeTileState(ActionRangeTile.TileState.InRange);
                Debug.Log("InRange Tile: " + t.row + ", " + t.col);
                continue;
            }

            if (actionCfg.targetingCfg.targetCurrentRow)
            {
                if(t.row is 1 or 0 or 2 && (t.col > 3)) t.SetActionRangeTileState(ActionRangeTile.TileState.InRange);
            }

            if (actionCfg.targetingCfg.targetCurrentCol)
            {
                if(t.col is 1 && t.row is 1) t.SetActionRangeTileState(ActionRangeTile.TileState.InRange);
            }
            
            if(targetsACurrent && t.row is 1 && t.col is 1) t.SetUserIndication(true);
        }
        
        rectActionsDisplay.RefreshLayoutGroupsImmediateAndRecursive();

        void CreateRoleTargetInfoAndDetails()
        {
            var newRoleTargetInfo = Instantiate(pr_vlg_roleTargetInfo, parent_roleTargetInfo);
            foreach (var targInfoChild in newRoleTargetInfo.transform.Children())
            {
                if (newRoleTargetInfo.transform.GetChild(0).gameObject == targInfoChild) continue;
                targInfoChild.gameObject.SetActive(false);
                Destroy(targInfoChild.gameObject);
            }
            
            var stringTargetingDetails = actionCfg.GetTargetDetails();
            foreach (var targDetail in stringTargetingDetails)
            {
                var newTargDetail = Instantiate(pr_hlg_infoDetail, newRoleTargetInfo.transform);
                newTargDetail.transform.GetChild(1).Get<TextMeshProUGUI>().text = targDetail; // hardoding this bc o well idc, its the second child
            }
            newRoleTargetInfo.Get<BattlemodeRoleTargetInfo>().SetRoleName("Target: " + actionCfg.roleTarget);
            
            // Check if there is anything to self if not just ret
            var stringSelfDetails = actionCfg.GetSelfDetails();
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
        
        // TO:DO Ordered Effects so the list is always in the same order
        void ShowEffects(BattlemodeActionConfig actionConfig)
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
    }
}