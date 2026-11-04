using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum BtnMenuUIState
{
    Default,
    Locked,
    Equipped
}


public abstract class MenuUISlot<TDependancy, TConfig> : MonoBehaviour
    where TDependancy : class
    where TConfig : class
{

    public Color darkened;
    public Color defaultColor;

    public BtnMenuUIState state;
    [Required] public TDependancy dependancy;
    [ShowInInspector] public TConfig cfg;
    [Required] public Image bgImg;
    [Required] public Image img;
    [Required] public Sprite defaultSprite;
    [FormerlySerializedAs("hoverSprite")] [Required] public Sprite selectSprite;
    [Required] public GameObject go_lock;
    [Required] public GameObject go_check;

    public void InjectAndSetup(TConfig actionCfg)
    {
        this.cfg = actionCfg;
        InjectAndSetupImplementation(actionCfg);
        ForceDefault();
    }

    protected abstract void InjectAndSetupImplementation(TConfig actionCfg);

    public void Lock()
    {
        state = BtnMenuUIState.Locked;
        img.color = darkened;
        bgImg.sprite = defaultSprite;
        go_check.SetActive(false);
        go_lock.SetActive(true);
    }

    public void ResetSlot()
    {
        img.sprite = null;
        cfg = null;
        gameObject.SetActive(false);
    }

    public void ForceDefault()
    {
        bgImg.sprite = defaultSprite;
        bgImg.color = darkened;
        img.color = darkened;
    }

    public abstract void Default();
    public abstract void Hover();
    public abstract void Select();

    public void Equip()
    {
        state = BtnMenuUIState.Equipped;
        go_check.SetActive(true);
        go_lock.SetActive(false);
    }

    public void Dequip()
    {
        state = BtnMenuUIState.Default;
        go_check.SetActive(false);
        go_lock.SetActive(false);
    }
}
