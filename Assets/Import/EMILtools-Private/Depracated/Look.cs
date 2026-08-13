using UnityEngine;
using DependencyInjection;

[RequireComponent(typeof(EntityControls))]
public class Look : MonoBehaviour
{
    // [Inject] EntityControls controls;
    // [SerializeField] float xSens, ySens;
    // [SerializeField] float lookX, lookY, xRot, yRot;
    // [SerializeField] bool updateMouseLook = true;
    //
    //
    // private void Start()
    // {
    //     Cursor.lockState = CursorLockMode.Locked;
    //     Cursor.visible = false;
    // }
    //
    // private void Update()
    // {
    //     MouseLook();
    // }
    //
    // void MouseLook()
    // {
    //     if (!updateMouseLook) return;
    //
    //     Vector2 mouseInput = controls.look.Invoke();
    //
    //     lookX = mouseInput.x * (xSens / 5);
    //     lookY = mouseInput.y * (ySens / 5) * -1;
    //
    //     xRot += lookX;
    //     yRot += lookY;
    //
    //     yRot = Mathf.Clamp(yRot, -90f, 90f);
    //
    //
    //     transform.rotation = Quaternion.Euler(0, xRot, 0);
    //     //print(Controls.head);
    //     controls.headDirection.transform.rotation = Quaternion.Euler(yRot, xRot, 0);
    //
    // }
    //
    // public void ToggleUpdateMouseLooking(bool val) => updateMouseLook = val;
    //
    // public void ToggleCursorUsability(bool val)
    // {
    //     if(val == true)
    //     {
    //         Cursor.lockState = CursorLockMode.Confined;
    //         Cursor.visible = true;
    //     }
    //     else
    //     {
    //         Cursor.lockState = CursorLockMode.Locked;
    //         Cursor.visible = false;
    //     }
    // }
}
