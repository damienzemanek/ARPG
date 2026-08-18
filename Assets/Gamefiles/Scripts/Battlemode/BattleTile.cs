using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;


public class BattleTile : MonoBehaviour
{
    public bool occupied => occupantCtx != null && occupantCtx.cfg != null;

    [ShowInInspector, BoxGroup("OccupantCtx")]
    public OccupantCtx occupantCtx;

    [FormerlySerializedAs("hiddenCompletly")] public bool unselectable = false;
    public bool alreadySelected = false;
    bool isInRange = false;
    [ReadOnly] public int col, row;
    public Vector3 occupantSpawnOffset = new Vector3(3.5f, 0, -3.5f);
    public float maxHPBarScale = 3.5f;

    [Required] public GameObject display;
    [Required] public GameObject hpBar;
    [Required] public GameObject display_AP;
    [Required] public TextMeshPro txt_AP;
    [Required] public TextMeshPro txt_remainingHP;


    [Required] public DetectorMouseClick mouseClickDetector;
    [Required] public GameObject @default;
    [Required] public GameObject hover;
    [Required] public GameObject selected;
    [Required] public GameObject inRange;


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
        selected.SetActive(false);
        isInRange = false;
        inRange.SetActive(false);
        mouseClickDetector.detections.Clear();
        alreadySelected = false;
    }
    

    public void HideAndMakeUnSelectable()
    {
        Clear();
        @default.SetActive(false);
        unselectable = true;
    }
    
    public void Unhide()
    {
        Clear();
        @default.SetActive(true);
        unselectable = false;
    }

    public void Hover()
    {
        if (!occupied) return;
        if (unselectable) return;
        if (selected.activeInHierarchy) return;
        hover.SetActive(true);
        inRange.SetActive(false);
    }
    

    public void UnHover()
    {
        hover.SetActive(false);
        if (isInRange)
        {
            inRange.SetActive(true);
            @default.SetActive(false);
        }
    }
    
    // Called from the event
    public void Select()
    {
        if (alreadySelected) return;
        if (!occupied) return;
        if (unselectable) return;
        BattleTracker.Instance.SelectTile(this);
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
        @default.SetActive(false);
        isInRange = true;
        inRange.SetActive(true);
    }

    public void NotInRange()
    {
        if (unselectable) return; // if already unselectable just return
        HideAndMakeUnSelectable();
    }
    
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

    public void ShowOnly(BattlemodeActionConfig.Role useTarget, OccupantCfg user)
    {
        bool shouldShow =
            (useTarget == BattlemodeActionConfig.Role.Enemy && IsEnemy()) ||
            (useTarget == BattlemodeActionConfig.Role.Self && IsSelf(user)) ||
            (useTarget == BattlemodeActionConfig.Role.Ally && IsAlly(user));

        if (!shouldShow) HideAndMakeUnSelectable();
    }

    public bool IsEnemy() => occupantCtx.cfg is EnemyConfig;
    public bool IsSelf(OccupantCfg selfConfig) => occupantCtx.cfg == selfConfig;
    public bool IsAlly(OccupantCfg selfConfig) => occupantCtx.cfg is CharacterConfig && !IsSelf(selfConfig);

    [Button]
    public void TestKill()
    {
        display.SetActive(false);
        if (occupantCtx.obj != null)
        {
            if (Application.isPlaying)
                Destroy(occupantCtx.obj);
            else
                DestroyImmediate(occupantCtx.obj);
        }
        occupantCtx = null;
        Debug.Assert(occupantCtx == null, "OccupantCtx should be null after destroying");
    }
}
