using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCameraController : MonoBehaviour
{
    [Header("Sensibilidad y límites")]
    public Vector2 Sensitivity = new Vector2(1f, 1f);
    public Vector2 VerticalClamp = new Vector2(-80f, 80f);

    [Header("Input")]
    public InputActionAsset inputActions;

    // Mantener los mismos nombres para compatibilidad con tu esquema anterior
    private InputAction rotateAction;
    private InputAction enableRotateAction;
    private InputActionMap map;
    private Vector2 rotation = Vector2.zero;
    private bool isRotating = false;

    [Header("Spherical world")]
    [Tooltip("Si se asigna, la 'up' local será (camPos - PlanetCenter).normalized. Si queda vacío se usará Vector3.up.")]
    public Transform PlanetCenter;

    // Opcional: referencia 'norte' estable para evitar ambigüedad en el plano tangente.
    [Tooltip("Opcional: referencia para definir la dirección 0° de yaw en el plano ecuatorial. Si está vacío se usa Vector3.forward proyectado.")]
    public Transform NorthReference;

    [Header("Suavizado (opcional)")]
    [Tooltip("Velocidad de interpolación para suavizar la rotación. 0 = sin suavizado.")]
    public float SmoothSpeed = 10f;

    // Delegados para poder desuscribir correctamente
    private System.Action<InputAction.CallbackContext> onEnableStarted;
    private System.Action<InputAction.CallbackContext> onEnableCanceled;
    private System.Action<InputAction.CallbackContext> onRotatePerformed;

    // Guardar la referencia previa para evitar inversiones 180°
    private Vector3 prevForwardOnEquator = Vector3.forward;

    private void Start()
    {
        // Inicializar prevForwardOnEquator en el plano tangente inicial para evitar saltos en el primer frame
        Vector3 initialUp = Vector3.up;
        if (PlanetCenter != null)
        {
            initialUp = (transform.position - PlanetCenter.position).normalized;
            if (initialUp.sqrMagnitude < 1e-6f) initialUp = Vector3.up;
        }

        Vector3 initRef;
        if (NorthReference != null)
            initRef = Vector3.ProjectOnPlane(NorthReference.forward, initialUp);
        else
            initRef = Vector3.ProjectOnPlane(Vector3.forward, initialUp);

        if (initRef.sqrMagnitude < 1e-4f)
            initRef = Vector3.ProjectOnPlane(Vector3.right, initialUp);

        prevForwardOnEquator = (initRef.sqrMagnitude > 1e-6f) ? initRef.normalized : Vector3.forward;
    }

    private void OnEnable()
    {
        map = inputActions.FindActionMap("OrbCam", true);
        map.Enable();

        rotateAction = map.FindAction("R", true);
        enableRotateAction = map.FindAction("EnableR", true);

        // crear delegates para poder desuscribir exactamente los mismos
        onEnableStarted = ctx =>
        {
            if (ctx.control.device is Pointer)
                isRotating = true;
        };

        onEnableCanceled = ctx =>
        {
            if (ctx.control.device is Pointer)
                isRotating = false;
            else
                isRotating = true;
        };

        onRotatePerformed = ctx =>
        {
            // Si el control no es Pointer (gamepad/joy) consideramos que siempre rota.
            if (ctx.control.device is not Pointer)
                isRotating = true;
            else
                isRotating = enableRotateAction.IsPressed();

            if (isRotating)
                RotateCamera(ctx.ReadValue<Vector2>());
        };

        enableRotateAction.started += onEnableStarted;
        enableRotateAction.canceled += onEnableCanceled;
        rotateAction.performed += onRotatePerformed;
    }

    private void OnDisable()
    {
        // desuscribir handlers exactos
        if (enableRotateAction != null)
        {
            enableRotateAction.started -= onEnableStarted;
            enableRotateAction.canceled -= onEnableCanceled;
        }
        if (rotateAction != null)
        {
            rotateAction.performed -= onRotatePerformed;
        }
        if (map != null)
            map.Disable();
    }

    private void LateUpdate()
    {
        // calcular "up" local: hacia afuera desde el centro del planeta si PlanetCenter asignado.
        Vector3 up = Vector3.up;
        if (PlanetCenter != null)
        {
            up = (transform.position - PlanetCenter.position).normalized;
            if (up.sqrMagnitude < 1e-6f) up = Vector3.up;
        }

        // evitar que rotation.x crezca sin control pero mantener continuidad: solo "recortar" si es enorme
        if (Mathf.Abs(rotation.x) > 10000f)
            rotation.x = Mathf.Repeat(rotation.x, 360f);

        // clamp del pitch según VerticalClamp
        rotation.y = Mathf.Clamp(rotation.y, VerticalClamp.x, VerticalClamp.y);

        // construir un forward de referencia en el plano tangente:
        Vector3 forwardRef;
        if (NorthReference != null)
        {
            forwardRef = Vector3.ProjectOnPlane(NorthReference.forward, up);
        }
        else
        {
            forwardRef = Vector3.ProjectOnPlane(Vector3.forward, up);
        }

        // fallback si la proyección es casi cero (p. ej. up ~ forward)
        if (forwardRef.sqrMagnitude < 1e-4f)
            forwardRef = Vector3.ProjectOnPlane(Vector3.right, up);

        // normalizar y mantener continuidad de signo para evitar inversiones 180°
        forwardRef.Normalize();

        // Si la nueva referencia está casi opuesta a la previa, invierte para mantener continuidad
        if (Vector3.Dot(forwardRef, prevForwardOnEquator) < 0f)
        {
            forwardRef = -forwardRef;
        }

        // actualizar prev para el siguiente frame
        prevForwardOnEquator = forwardRef;

        // yaw alrededor del up local (continuo porque rotation.x es acumulativa)
        Quaternion yawQ = Quaternion.AngleAxis(rotation.x, up);
        Vector3 forwardOnEquator = yawQ * forwardRef;

        // right local (perpendicular a up y forwardOnEquator)
        Vector3 right = Vector3.Cross(forwardOnEquator, up).normalized;

        // aplicar pitch alrededor del eje right
        Vector3 direction = Quaternion.AngleAxis(-rotation.y, right) * forwardOnEquator; // nota el signo para que concuerde con la inversión de Y en el input

        // Si direction queda degenerada (muy cerca de up), forzamos un pequeño ajuste para evitar singularidades.
        if (direction.sqrMagnitude < 1e-6f)
        {
            // empujar ligeramente en el plano tangente
            direction = Vector3.ProjectOnPlane(transform.forward, up);
            if (direction.sqrMagnitude < 1e-6f)
                direction = forwardOnEquator; // fallback seguro
            direction.Normalize();
        }

        // construir rotación final mirando en 'direction' con 'up' como up vector
        Quaternion targetRotation = Quaternion.LookRotation(direction, up);

        // aplicar suavizado opcional
        if (SmoothSpeed > 0f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Mathf.Clamp01(Time.deltaTime * SmoothSpeed));
        }
        else
        {
            transform.rotation = targetRotation;
        }
    }

    private void RotateCamera(Vector2 input)
    {
        // mantener los mismos nombres y lógica de sensibilidad
        rotation.x += input.x * Sensitivity.x;
        rotation.y -= input.y * Sensitivity.y;
    }
}
