using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;


public class BattleTile : MonoBehaviour
{
    public bool occupied => occupantCtx != null && occupantCtx.cfg != null;
    [ShowInInspector, BoxGroup("OccupantCtx")] public OccupantCtx occupantCtx;

    public bool hiddenCompletly = false;
    bool alreadySelected = false;
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
    

    public void Init(int _col, int _row, BattlePositionOccupantConfig occupantConfig = null)
    {
        this.col = _col;
        this.row = _row;
        UnHover();
        
        if (occupantConfig == null) return;
        
        switch (occupantConfig)
        {
            case CharacterConfig cc: occupantCtx = new CharacterOccupantCtx(cc); break;
            case EnemyConfig ec: occupantCtx = new EnemyOccupantCtx(ec); break;
            default: occupantCtx = new BattlerOccupantCtx(occupantConfig as BattlerConfig); break;
        }
        
        if (!occupied) return;
        Spawn(occupantConfig);
        display_AP.SetActive(occupantConfig is CharacterConfig);
    }
    

    public void Unhide()
    {
        Clear();
        @default.SetActive(true);
        hiddenCompletly = false;
    }
    
    public void Clear()
    {
        UnHover();
        selected.SetActive(false);
        mouseClickDetector.detections.Clear();
        alreadySelected = false;
    }

    public void UnHover() => hover.SetActive(false);

    public void Hover()
    {
        if (!occupied) return;
        if (hiddenCompletly) return;
        if(selected.activeInHierarchy) return;
        hover.SetActive(true);
    }

    public void Select()
    {
        if (alreadySelected) return;
        if (!occupied) return;
        if (hiddenCompletly) return;
        BattleTracker.Instance.SelectTile(this);
        hover.SetActive(false);
        selected.SetActive(true);
        alreadySelected = true;
    }

    public void UpdateTransientStats()
    {
        if(!occupied) return;
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
    void Spawn(BattlePositionOccupantConfig config)
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
        SetHealth(battlerConfig.maxHP, battlerConfig.maxHP);
        display.SetActive(true);
    }

    public void ShowOnly(BattlemodeActionConfig.Role useTarget, BattlePositionOccupantConfig user)
    {
        bool shouldShow =
            (useTarget == BattlemodeActionConfig.Role.Enemy && IsEnemy()) ||
            (useTarget == BattlemodeActionConfig.Role.Self  && IsSelf(user)) ||
            (useTarget == BattlemodeActionConfig.Role.Ally  && IsAlly(user));

        if (!shouldShow) HideCompletely();
    }
    
    public bool IsEnemy() => occupantCtx.cfg is EnemyConfig;
    public bool IsSelf(BattlePositionOccupantConfig selfConfig) => occupantCtx.cfg == selfConfig;
    public bool IsAlly(BattlePositionOccupantConfig selfConfig) => occupantCtx.cfg is CharacterConfig && !IsSelf(selfConfig);
    public void HideCompletely()
    {
        Clear();
        @default.SetActive(false);
        hiddenCompletly = true;
    }
    

    [Button]
    public void TestKill()
    {
        display.SetActive(false);
        occupantCtx.cfg = null;
        if (occupantCtx.obj != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(occupantCtx.obj);
            return;
#endif
            Destroy(occupantCtx.obj);
        }
        occupantCtx.obj = null;
        occupantCtx = null;
    }
    
    
}
