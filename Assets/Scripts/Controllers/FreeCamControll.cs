using System.Collections;
using
 UnityEngine;
using UnityEngine.InputSystem;


public class FreeCamControll : MonoBehaviour
{
    public InputActionAsset inputActionsAsset;
    private Camera cam;
    private InputAction MOVE;
    private InputAction LOOK;
    
    // Start is called before the first frame update
    void OnEnable()
    {
        cam = Camera.main;
        inputActionsAsset.FindActionMap("FreeCam", true).Enable();
        MOVE = inputActionsAsset.FindAction("Pos", true);
        Pointer_HELD = inputActionsAsset.FindAction("Pointer_HELD", true);
        LOOK = inputActionsAsset.FindAction("Vista", true);
    }
    void OnDisable()
    {
        cam = Camera.main;
        inputActionsAsset.Disable();
    }

    
    void FixedUpdate()
    {
        if (cam == null) return;
        RotateCam();
        MoveCam();
    }
    private float yaw = 0f;
    private float pitch = 0f;
    private InputAction Pointer_HELD;
    void RotateCam()
    {
        // Obtener el dispositivo que está generando el input
        Vector2 lookInput = LOOK.ReadValue<Vector2>();

        if (lookInput == Vector2.zero) return;

        // Detectar si el input viene del Pointer
        var lastDevice = LOOK.activeControl?.device;

        bool isPointer = lastDevice is Pointer;
        bool PointerIsHeld = Pointer_HELD.ReadValue<float>() > 0f;
        //Debug.Log(PointerIsHeld.ToString()); ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ;
        // Si el input es del Pointer y NO está presionado el botón izquierdo, no rotar
        if (isPointer && !PointerIsHeld) return;

        // Aplicar rotación
        yaw += lookInput.x;
        pitch -= lookInput.y;

        pitch = Mathf.Clamp(pitch, -89f, 89f);
        cam.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void MoveCam()
    {
        Vector2 input2D = MOVE.ReadValue<Vector2>();
        Vector3 forward = cam.transform.forward.normalized;
        Vector3 right = cam.transform.right;
        Vector3 movimiento = forward * input2D.y + right * input2D.x;
        cam.transform.position += movimiento * 1f * Time.fixedDeltaTime;
    }
}
