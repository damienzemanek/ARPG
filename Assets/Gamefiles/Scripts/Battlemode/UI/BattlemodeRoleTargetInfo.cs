using TMPro;
using UnityEngine;

public class BattlemodeRoleTargetInfo : MonoBehaviour
{
    public TextMeshProUGUI roleName;
    public Transform detailParent;
    
    public void SetRoleName(string roleName)
    {
        this.roleName.text = roleName;
    }
}
