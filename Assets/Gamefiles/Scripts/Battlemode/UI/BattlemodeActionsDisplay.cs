using System;
using System.Collections.Generic;
using System.Linq;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattlemodeActionsDisplay : MonoBehaviour
{
    const string k_LevelFormat = "Lvl {0}/60";
    
    [Required] public GameObject GUI;
    
    [Required] public GameObject actionDisplayPrefab;
    [Required] public Transform actionDisplayParent;
    
    [BoxGroup("Status Effects")] [Required] public GameObject effectDisplayPrefab;
    [BoxGroup("Status Effects")] [Required] public Transform effectDisplayParent;
    [BoxGroup("Status Effects")] [Required] public RectTransform effectContentRectTransform;
    [BoxGroup("Status Effects")] [ReadOnly, ShowInInspector] Pooled<BattlemodeEffect> effectDisplayPool;
    
    [BoxGroup("Special Effects")] [Required] public GameObject specialEffectDisplayPrefab;
    [BoxGroup("Special Effects")] [Required] public Transform specialEffectDisplayParent;
    [BoxGroup("Special Effects")] [Required] public RectTransform specialEffectContentRectTransform;
    [BoxGroup("Special Effects")] [ReadOnly, ShowInInspector] Pooled<BattlemodeSpecialEffect> specialEffectDisplayPool;

    [ReadOnly] public BattleTile currentlySelectedTile = null;
    [ReadOnly] public BattlemodeActionCtx? currentlySelectedAction = null;

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

    [BoxGroup("BattleInfo")] [Required] public RectTransform combatDisplayRectTransform;
    [BoxGroup("BattleInfo")] [Required] public GameObject display_Enemy;
    [BoxGroup("BattleInfo")] [Required] public GameObject display_Player;

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
    
    
    [BoxGroup("BattleInfo: Action")] [Required] public TextMeshProUGUI txt_PlayerActionName;
    [BoxGroup("BattleInfo: Action")] [Required] public TextMeshProUGUI txt_PlayerActionDescription;
    [BoxGroup("BattleInfo: Action")] [Required] public TextMeshProUGUI txt_PlayerAPnum;
    // Future: Button For Disabling Attacks
    
    [BoxGroup("BattleInfo: Enemy")] [Required] public TextMeshProUGUI txt_EnemyActionName;
    [BoxGroup("BattleInfo: Enemy")] [Required] public TextMeshProUGUI txt_EnemyActionDescription;
    [BoxGroup("BattleInfo: Enemy")] [Required] public TextMeshProUGUI txt_EnemyActionIntentionsNum;
    [BoxGroup("BattleInfo: Enemy")] [Required] public TextMeshProUGUI txt_EnemyVisableActionIntentionsNum;
    
    [ReadOnly] public BattlemodeAction[] actionSlots = Array.Empty<BattlemodeAction>();

    void Awake()
    {
        // Status Effects Setup
        effectDisplayPool = new(
            () =>
            {
                var ret = Instantiate(effectDisplayPrefab, effectDisplayParent).Get<BattlemodeEffect>();
                ret.gameObject.SetActive(false);
                return ret;
            },
            (effect) => effect.gameObject.SetActive(true),
            effectDisplay => effectDisplay.Hide(),
            12
        );
        effectDisplayPool.Prewarm(5);
        effectDisplayPool.ReleaseAll();

        // Special Effects Setup
        specialEffectDisplayPool = new(() =>
            {
                var ret = Instantiate(specialEffectDisplayPrefab, specialEffectDisplayParent)
                    .Get<BattlemodeSpecialEffect>();
                ret.gameObject.SetActive(false);
                return ret;
            },
            (effect) => effect.gameObject.SetActive(true),
            specialEffectDisplay => specialEffectDisplay.Hide(),
            3
            );
        specialEffectDisplayPool.Prewarm(3);
        specialEffectDisplayPool.ReleaseAll();
        
        
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
    }
    

    // Future: Reset most recetly selected action on battle complete
    public void ShowDisplay(BattleTile tile)
    {
        if (tile.occupantCtx.cfg is not BattlerConfig battlerConfig) return;

        currentlySelectedTile = tile; // Grab
        effectDisplayPool.ReleaseAll(); // Reset Effect Display
        specialEffectDisplayPool.ReleaseAll();
        GUI.SetActive(true); // Show GUI
        
        // Action Slot Enabling
        for (int i = 0; i < maxAmountOfTotalActionsAvaliable; i++)
        {
            bool hasActionSlot = i < battlerConfig.equippedActions.Length;
            bool withinHandSize = i < battlerConfig.maxEquippableActions;

            if (!hasActionSlot || !withinHandSize) continue;

            var action = battlerConfig.equippedActions[i];
            if (action == null) continue;

            if(tile.occupantCtx is not CharacterOccupantCtx characterOccupantCtx) continue;
            
            var actionCtx = action.GenerateActionCtx(
                BattlemodeActionCtx.Status.Acting,
                characterOccupantCtx,
                null);
            
            actionSlots[i].gameObject.SetActive(true);
            actionSlots[i].InitAction(actionCtx);
        }
        
        
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
            characterCtx.PreResolveActingEffects(actionSlots);

        }
        else if (tile.occupantCtx is BattlerOccupantCtx battlerCtx)
        {
            apValues.gameObject.SetActive(false);
            txt_CharacterArmorNum.text = battlerCtx.currentArmor.ToString();
            txt_CharacterMaxArmorNum.text = "/" + battlerCtx.maxArmor.ToString();
            txt_CharacterHpNum.text = battlerCtx.currentHp.ToString();
            txt_CharacterMaxHpNum.text = "/" + battlerCtx.maxHp.ToString();
            ShowSpecialEffects(battlerCtx);
            battlerCtx.PreResolveActingEffects(actionSlots);
        }
        
        // Recently Selected Action Setup
        ShowAction(tile, actionSlots.FirstOrDefault()!.actionCtx);
        LayoutRebuilder.MarkLayoutForRebuild(combatDisplayRectTransform);
    }

    public void ShowAction(BattleTile tile, BattlemodeActionCtx actionCtx)
    {
        if (actionCtx == null) return;
        
        // Populate Action Display Info
        if(tile.IsEnemy())
        {
            display_Enemy.SetActive(true);
            display_Player.SetActive(false);
            if (tile.occupantCtx.cfg is not EnemyConfig ec) {
                Debug.LogError("Trying to display enemy action info on non-enemy occupant : " + tile.occupantCtx.cfg.occupantName);
                return; }
            UpdateEnemyActionInfo(actionCtx, ec);
        }
        else
        {
            display_Enemy.SetActive(false);
            display_Player.SetActive(true);
            if (tile.occupantCtx.cfg is not CharacterConfig cc) {
                Debug.LogError("Trying to display player action info on non-player occupant : " + tile.occupantCtx.cfg.occupantName);
                return; }
            UpdatePlayerActionInfo(actionCtx, cc);
            Debug.Log("Updated Player Action Info.");
        }
        
        ShowEffects(actionCtx?.cfg);
        tile.occupantCtx.mostRecentSelectedAction = actionSlots.FirstOrDefault(a => a.actionCtx?.cfg == actionCtx?.cfg);
        currentlySelectedAction = tile.occupantCtx.mostRecentSelectedAction?.actionCtx;
        LayoutRebuilder.MarkLayoutForRebuild(combatDisplayRectTransform);
    }


    public void UpdateEnemyActionInfo(BattlemodeActionCtx actionCtx, EnemyConfig enemyConfig)
    {
        txt_EnemyActionName.text = actionCtx.cfg.actionName;
        txt_EnemyActionDescription.text = actionCtx.cfg.description;
        txt_EnemyActionIntentionsNum.text = enemyConfig.actionIntentionsCount.ToString();
        txt_EnemyVisableActionIntentionsNum.text = enemyConfig.visableActionCount.ToString();
    }
    
    
    public void UpdatePlayerActionInfo(BattlemodeActionCtx actionCtx, CharacterConfig characterConfig)
    {
        var resolvedApCost = actionCtx.cfg.apCost + actionCtx.apDelta;
        
        txt_PlayerActionName.text = actionCtx.cfg.actionName;
        txt_PlayerActionDescription.text = actionCtx.cfg.description;
        txt_PlayerAPnum.text = resolvedApCost.ToString();
        Debug.Log($"Updated Player Action Info: {actionCtx.cfg.actionName}, AP Cost: {resolvedApCost}");
    }
    
    
    public void ShowEffects(BattlemodeActionConfig actionConfig)
    {
        // Populate Effects
        effectDisplayPool.ReleaseAll();
        
        // Little bit unoptimized oh well, this creates a new effect ctx for the effects of an attack
        // effect ctx is a class so this churns gc. fine for now.
        // effect ctx holds state (stacks amount), could separate later. although locality of info is desired
        for (int i = 0; i < actionConfig.effectsToApplyToTarget.Count; i++)
            effectDisplayPool.Get().PopulateEffect(actionConfig.effectsToApplyToTarget[i].GenerateEffectCtx());
        LayoutRebuilder.MarkLayoutForRebuild(effectContentRectTransform);
    }

    public void ShowSpecialEffects(BattlerOccupantCtx battlerOccupantCtx)
    {
        specialEffectDisplayPool.ReleaseAll();
        foreach (var effect in battlerOccupantCtx.specialEffects)
            specialEffectDisplayPool.Get().PopulateEffect(effect);
        LayoutRebuilder.MarkLayoutForRebuild(specialEffectContentRectTransform);
    }

    
    public void HideDisplay()
    {
        currentlySelectedTile = null;
        effectDisplayPool.ReleaseAll();
        foreach (var slot in actionSlots) slot.Hide();
        GUI.SetActive(false);
    }

    public void UseAction()
    {
        if (currentlySelectedAction != null) 
            BattleTracker.Instance.UseAndQueueAction(
                currentlySelectedTile, 
                (BattlemodeActionCtx)currentlySelectedAction
            );
    }

    public int GetCurrentlySelectedAPCost()
    {
        if (currentlySelectedAction == null) return 0;
        return currentlySelectedAction.cfg.apCost + currentlySelectedAction.apDelta;
    }
}
