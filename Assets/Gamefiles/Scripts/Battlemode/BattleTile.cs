using System;
using System.Collections.Generic;
using System.Linq;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;


public class BattleTile : MonoBehaviour
{
    public Vector3 worldOccupantSpawnPos => transform.position + occupantSpawnOffset;
    public bool occupied => occupantCtx != null && occupantCtx.cfg != null;

    [Flags]
    public enum ColRank
    {
        None = 0,
        Left3 = 1 << 0,
        Left2 = 1 << 1,
        Left1 = 1 << 2,
        Center = 1 << 3,
        Right1 = 1 << 4,
        Right2 = 1 << 5,
        Right3 = 1 << 6,
    }
    
    [Flags]
    public enum RowRank
    {
        None = 0,
        Top = 1 << 0,
        Middle = 1 << 1,
        Bottom = 1 << 2,
    }
    
    
    [BoxGroup("Position")] [ReadOnly] public ColRank colRank;
    [BoxGroup("Position")] [ReadOnly] public RowRank rowRank;
    [BoxGroup("Position")] [ReadOnly] public int col, row;
    [BoxGroup("Position")] [ReadOnly] public GridWorld.BattlefieldSection section;
    
    [BoxGroup("Transient State")] [ReadOnly] public bool unselectable = false;
    [BoxGroup("Transient State")] [ReadOnly] public bool alreadySelected = false;
    [BoxGroup("Transient State")] [ReadOnly] bool isInRange = false;
    [BoxGroup("Transient State")] [ReadOnly] public bool tileCanBeSelectedOveride = false;
    [BoxGroup("Transient State")] [ShowInInspector] public OccupantCtx occupantCtx;
    
    [BoxGroup("Settings")] public Vector3 occupantSpawnOffset = new Vector3(3.5f, 0, -3.5f);
    [BoxGroup("Settings")] public float maxHPBarScale = 3.5f;

    [BoxGroup("References")] [Required] public GameObject display;
    [BoxGroup("References")] [Required] public GameObject hpBar;
    [BoxGroup("References")] [Required] public GameObject display_AP;
    [BoxGroup("References")] [Required] public TextMeshPro txt_AP;
    [BoxGroup("References")] [Required] public TextMeshPro txt_remainingHP;


    [BoxGroup("References")] [Required] public Grid3D intentionsGrid;
    [BoxGroup("References")] [Required] public DetectorMouseClick mouseClickDetector;
    [BoxGroup("References")] [Required] public GameObject @default;
    [BoxGroup("References")] [Required] public GameObject contested;
    [BoxGroup("References")] [Required] public GameObject hover;
    [BoxGroup("References")] [Required] public GameObject selected;
    [BoxGroup("References")] [Required] public GameObject inRange;


    [BoxGroup("References")] [Required] public Sprite actionIdentifier_Attack;
    [BoxGroup("References")] [Required] public Sprite actionIdentifier_AttackDebuff;
    [BoxGroup("References")] [Required] public Sprite actionIdentifier_AttackBuff;
    [BoxGroup("References")] [Required] public Sprite actionIdentifier_Debuff;
    [BoxGroup("References")] [Required] public Sprite actionIdentifier_Buff;
    [BoxGroup("References")] [Required] public Sprite actionIdentifier_Move;

    public void Init(int _col, int _row, OccupantCfg occupantCfg = null)
    {
        this.col = _col;
        this.row = _row;
        UnHover();

        if (occupantCfg == null)
        {
            Debug.Log("(EARLY RET) No occupent config given for BattleTile Init() at [" + col + ", " + row + "]");
            return;
        }

        switch (occupantCfg)
        {
            case CharacterConfig cc: occupantCtx = new CharacterOccupantCtx(cc); break;
            case EnemyConfig ec: occupantCtx = new EnemyOccupantCtx(ec); break;
            default: occupantCtx = new BattlerOccupantCtx(occupantCfg as BattlerConfig); break;
        }

        if (occupantCtx.cfg == null)
            Debug.LogError($"Created Occupant Ctx wrongly has null config [{occupantCtx.cfg}]");

        Spawn(occupantCfg);
        display_AP.SetActive(occupantCfg is CharacterConfig);
    }

    public void Clear()
    {
        UnHover();
        unselectable = false;
        selected.SetActive(false);
        isInRange = false;
        inRange.SetActive(false);
        mouseClickDetector.detections.Clear();
        alreadySelected = false;
        tileCanBeSelectedOveride = false;
        display.SetActive(occupantCtx != null);
    }
    
    public void HideDisplay() => display.SetActive(false);
    
    public void HideAndMakeUnSelectable()
    {
        Clear();
        DefaultOrContestedSetActive(false);
        contested.SetActive(false);
        unselectable = true;
    }
    
    public void SetUnselectable(bool v) => unselectable = v;

    public void DefaultOrContestedSetActive(bool v)
    {
        if (section != GridWorld.BattlefieldSection.Contested)
        {
            @default.SetActive(v);
            contested.SetActive(false);
        }
        else
        {
            @default.SetActive(false);
            contested.SetActive(v);
        }
    }
    
    public void Unhide()
    {
        Clear();
        DefaultOrContestedSetActive(true);
    }

    public void Hover()
    {
        if (BattleTracker.Instance.currentTurn != BattleTracker.Turn.Player) return;
        if (!occupied && !tileCanBeSelectedOveride) return;
        if (unselectable) return;
        if (selected.activeInHierarchy) return;
        hover.SetActive(true);
        inRange.SetActive(false);
        Debug.Log("[BattleTile] Player Hovered over tile [" + col + ", " + row + "]");
    }
    

    public void UnHover()
    {
        hover.SetActive(false);
        if (isInRange)
        {
            inRange.SetActive(true);
            DefaultOrContestedSetActive(false);
        }
    }
    
    // Called from the event 
    public void Select()
    {
        Debug.Log("Selecting tile");
        if (BattleTracker.Instance.currentTurn == BattleTracker.Turn.Enemy) return;
        if (BattleTracker.Instance.currentTurn == BattleTracker.Turn.Transitioning) return;
        if (alreadySelected) return;
        if (!occupied && !tileCanBeSelectedOveride) return;
        if (unselectable) return;
        BattleTracker.Instance.SelectTile(this);
        Debug.Log("Selecting tile Successfull");
    }

    // Eventually called via BattlerTracker.Instance.SelectTile(this)
    public void SelectImplementation()
    {
        hover.SetActive(false);
        selected.SetActive(true);
        alreadySelected = true;
    }

    public void InRange()
    {
        Clear();
        DefaultOrContestedSetActive(false);
        isInRange = true;
        inRange.SetActive(true);
    }

    public void NotInRange()
    {
        if (unselectable) return; // if already unselectable just return
        HideAndMakeUnSelectable();
    }

    #region Display Stats

    public void UpdateTransientStats()
    {
        if (!occupied) return;
        if (occupantCtx is not BattlerOccupantCtx battlerCtx) return;
        SetHealth(battlerCtx.currentHp, battlerCtx.maxHp);
        if (occupantCtx is not CharacterOccupantCtx characterCtx) return;
        SetAP(characterCtx.currentAP);

    }

    public void SetHealth(int current, float max)
    {
        var frac = (current / max);
        Vector3 prev = hpBar.transform.localScale;
        hpBar.transform.localScale = new Vector3(frac * maxHPBarScale, prev.y, prev.z);
        txt_remainingHP.text = "" + current.ToString();
        if (current <= 0)
        {
            hpBar.transform.localScale = new Vector3(0, prev.y, prev.z);
            txt_remainingHP.text = "0";
            TestKill();
        }
    }

    public void SetAP(int current)
    {
        if (occupantCtx is not CharacterOccupantCtx characterCtx) return;
        txt_AP.text = current.ToString();
    }

    #endregion
    
    #region Intentons
    
    public void RegenerateAndDisplayGrid(int intentions)
    {
        if (intentions <= 0) { intentionsGrid.gameObject.SetActive(false); return; }
        intentionsGrid.gameObject.SetActive(true);
        if(intentions == intentionsGrid.x) return;
        intentionsGrid.x = intentions;
        Debug.Log($"{occupantCtx.cfg.name}'s tile: Set intentions amount to " + intentions);
    }
    
    public void DisplayIntentionToBattleTile(int intentionIndex, BattlemodeActionConfig.ActionIdentifier actionIdentifier)
    {
        Debug.Log("Setting Intention: " + actionIdentifier + " at " + intentionIndex + " row size is [" + intentionsGrid.GetRow(0).rowPositions.Count + "]");
        var rowPos = intentionsGrid.GetRow(0).rowPositions;
        if (intentionIndex < 0 || intentionIndex >= rowPos.Count) return;
        var spriteRenderer = rowPos[intentionIndex].created.Get<SpriteRenderer>();
        spriteRenderer.sprite = actionIdentifier switch
        {
            BattlemodeActionConfig.ActionIdentifier.Attack => actionIdentifier_Attack,
            BattlemodeActionConfig.ActionIdentifier.AttackBuff => actionIdentifier_AttackBuff,
            BattlemodeActionConfig.ActionIdentifier.AttackDebuff => actionIdentifier_AttackDebuff,
            BattlemodeActionConfig.ActionIdentifier.Buff => actionIdentifier_Buff,
            BattlemodeActionConfig.ActionIdentifier.Debuff => actionIdentifier_AttackDebuff,
            BattlemodeActionConfig.ActionIdentifier.Move => actionIdentifier_Move,
            _ => intentionsGrid.GetRow(0).rowPositions[intentionIndex].created.Get<SpriteRenderer>().sprite
        };
    }

    public void TryDisplayIntentions()
    {
        if (occupantCtx == null || occupantCtx is not EnemyOccupantCtx eoc) return;
        RegenerateAndDisplayGrid(eoc.currentIntentUsageCtx.intentionsAmount);
        
        // For each intention
        for (int intentionsIndex = 0; intentionsIndex < eoc.currentIntentUsageCtx.intentionsAmount; intentionsIndex++)
        {
            var actionIdentifier = eoc.queuedActions.ElementAt(intentionsIndex).actionCtx.cfg.actionIdentifier;
            DisplayIntentionToBattleTile(intentionsIndex, actionIdentifier);
        }
    }
    
    #endregion



    [Button]
    void Spawn(OccupantCfg config)
    {
        if (config is not BattlerConfig battlerConfig)
        {
            Debug.LogError("Trying to spawn a non-battler occupant");
            return;
        }

        occupantCtx.obj = Instantiate(
            config.prefab,
            transform.position + occupantSpawnOffset,
            Quaternion.identity);
        occupantCtx.obj.name = config.name;
        SetHealth(battlerConfig.maxHP, battlerConfig.maxHP);
        display.SetActive(true);

        Debug.Log("Spawned " + config.name + " at [" + col + ", " + row + "]");
    }
    

    public void ResetSpecialTargetConsiderations()
    {
        tileCanBeSelectedOveride = false;
    }

    public void SpecialTargetConsiderations(BattlemodeActionConfig.Role useTarget)
    {
        if (useTarget == BattlemodeActionConfig.Role.EmptyTile && IsEmptyTile())
        {
            unselectable = false;
            tileCanBeSelectedOveride = true;
        }
    }

    public bool IsEnemy() => occupantCtx?.cfg is EnemyConfig;
    public bool IsSelf(OccupantCfg selfConfig) => occupantCtx?.cfg == selfConfig;

    public bool IsAlly(OccupantCfg selfConfig)
    {
        bool ret = occupantCtx?.cfg is CharacterConfig && !IsSelf(selfConfig);
        Debug.Log("[ALLY CHECK] " + ret);
        return ret;
    }
    public bool IsEmptyTile() => occupantCtx == null;

    public bool IsSameRoleTarget(BattlemodeActionConfig.Role roleTarget, OccupantCfg compareCfg)
    {
        switch (roleTarget)
        {
            case BattlemodeActionConfig.Role.Enemy: return IsEnemy();
            case BattlemodeActionConfig.Role.Self: return IsSelf(compareCfg);
            case BattlemodeActionConfig.Role.Ally: return IsAlly(compareCfg);
            case BattlemodeActionConfig.Role.EmptyTile: return IsEmptyTile();
            default: return false;
        }
    }

    [Button]
    public void TestKill()
    {
        ResetTileOccupancy(true);
    }

    public void ResetTileOccupancy(bool destroy)
    {
        display.SetActive(false);
        if (occupantCtx.obj != null)
        {
            if (destroy && Application.isPlaying) Destroy(occupantCtx.obj);
            else if (destroy) DestroyImmediate(occupantCtx.obj);
        }
        occupantCtx = null;
        Debug.Assert(occupantCtx == null, "OccupantCtx should be null after destroying");
    }
    
    

    public void TransferInOccupant(BattleTile previousTile, GridWorld grid)
    {
        if (occupantCtx != null)
            { Debug.LogError("Trying to transfer occupant to a tile that already has an occupant"); return; }
        if (previousTile == null)
            { Debug.LogError("Previous tile is null"); return; }
        if(previousTile.occupantCtx == null)
            { Debug.LogError($"Previous tile [{previousTile.col} , {previousTile.row}] does not have an occupant"); return; }
        occupantCtx = previousTile.occupantCtx;
        occupantCtx.newTilePosition = this;
        previousTile.occupantCtx.obj.transform.position = transform.position + occupantSpawnOffset;
        Clear();
        display.SetActive(true);
        previousTile.ResetTileOccupancy(false);
        grid.UpdateGrid();
        TryDisplayIntentions();
        Debug.Log($"Transferred occupant [{occupantCtx.cfg.occupantName}] from " + previousTile.col + ", " + previousTile.row + " to " + col + ", " + row);
    }
    
    public void SwapOccupants(BattleTile previousTile, GridWorld grid)
    {
        if (previousTile == null) { Debug.LogError("Trying to swap occupants with a null tile"); return; }
        if (occupantCtx == null) { Debug.LogError($"Tile [{col}, {row}] does not have an occupant to swap"); return; }
        if (previousTile.occupantCtx == null) { Debug.LogError($"Tile [{previousTile.col}, {previousTile.row}] does not have an occupant to swap"); return; }

        var previousOccupant = occupantCtx;
        var otherOccupant = previousTile.occupantCtx;

        occupantCtx = otherOccupant;
        previousTile.occupantCtx = previousOccupant;

        occupantCtx.newTilePosition = this;
        previousTile.occupantCtx.newTilePosition = previousTile;

        occupantCtx.obj.transform.position = transform.position + occupantSpawnOffset;
        previousTile.occupantCtx.obj.transform.position = previousTile.transform.position + previousTile.occupantSpawnOffset;
        
        Clear();
        previousTile.Clear();
        grid.UpdateGrid();
        
        previousTile.TryDisplayIntentions();
        TryDisplayIntentions();

        Debug.Log($"Swapped occupants between [{col}, {row}] and [{previousTile.col}, {previousTile.row}]");
    }
}
