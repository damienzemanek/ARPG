using System;
using Sirenix.OdinInspector;
using UnityEngine;


[Serializable]
[CreateAssetMenu(fileName = "MouseLookSettingsSO", menuName = "ScriptableObjects/Settings/Mouselook")]
public class MouseLookSettingsSO : ScriptableObject
{
    const int SENSITIVITY_ADJUSTMENT = 5;

    [BoxGroup("ReadOnly")] [SerializeField, ReadOnly] Vector2 look;
    [BoxGroup("ReadOnly")] [SerializeField, ReadOnly] Vector2 rot;
    
    [BoxGroup("Settings")] [SerializeField] public bool clampXRotation = false;
    [BoxGroup("Settings")] [SerializeField] public bool clampYRotation = true;
    [BoxGroup("Settings")] [SerializeField] bool invertX = false;
    [BoxGroup("Settings")] [SerializeField] bool invertY = true;
            
    [BoxGroup("Settings")] [SerializeField] Vector2 sensitivity = new Vector2(1, 1);
    [BoxGroup("Settings")] public float lookSmoothness = 20f;

    [BoxGroup("Settings")] [SerializeField] [ShowIf("clampXRotation")] Vector2 clampX = new Vector2(-90f, 90f);
    [BoxGroup("Settings")] [SerializeField] [ShowIf("clampYRotation")] Vector2 clampY = new Vector2(-90f, 90f);

    public void LockMouse(bool v) => Cursor.lockState = v ? CursorLockMode.Locked : CursorLockMode.None;

    public void DetermineMouselook(Vector2 input, out Quaternion bodyRot, out Quaternion headRot)
    {
        // Grab the input
        Vector2 mouseInput = input; //Debug.Log("Mouse Input: " + mouseInput);
                
        // Apply sensitivity
        look = mouseInput * sensitivity / SENSITIVITY_ADJUSTMENT;

        look.x *= invertX ? -1f : 1f;
        look.y *= invertY ? -1f : 1f;

        // Apply the rotation to the variable
        rot.x += look.x;
        rot.y += look.y;
        if(clampXRotation) rot.x = Mathf.Clamp(rot.x, clampX.x, clampX.y);
        if(clampYRotation) rot.y = Mathf.Clamp(rot.y, clampY.x, clampY.y);
                
        // Use the variable on the transforms
        bodyRot = Quaternion.Euler(0, rot.x, 0);
        headRot = Quaternion.Euler(rot.y, rot.x, 0);
    }
}
