using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

public class CameraOrbitController : MonoBehaviour
{
    [Header("Referencias")]
    public Transform Target;              // Objeto a observar
    public Camera Camera;                // Cámara principal
    public RectTransform Cursor;         // Cursor UI (en pantalla)

    [Header("Parámetros de órbita")]
    public float ZoomSpeed = 5f;
    public Vector2 ZoomLimits = new Vector2(2f, 20f);
    public Vector2 Sensitivity = new Vector2(1f, 1f);
    public Vector2 VerticalClamp = new Vector2(-80, 80);
    public bool EnableZoom = true;
    public float ZoomOutput = 0;

    [Header("Cursor")]
    public float CursorSpeed = 10f;      // Sensibilidad del cursor UI
	public bool usandoMouse = false;
	public bool DisableCursor = false;

	[Header("Input")]
    public InputActionAsset inputActions;

    private InputAction rotateAction;
    private InputAction zoomAction;
    private InputAction enableRotateAction;
    private InputAction moveCursorAction;

    private Vector2 rotation = new Vector2(0, 0);
    public float currentZoom = 10f;
    private bool isRotating = false;
    private Vector2 cursorPosition = Vector2.zero;
    InputActionMap map;
    private void OnEnable()
    {
        map = inputActions.FindActionMap("OrbCam", true);
        map.Enable();

        rotateAction = map.FindAction("R", true);
        zoomAction = map.FindAction("Zoom", true);
        enableRotateAction = map.FindAction("EnableR", true);
        moveCursorAction = map.FindAction("L", true);

        enableRotateAction.started += ctx =>
        {
            if (ctx.control.device is Pointer)
                isRotating = true;
        };

        enableRotateAction.canceled += ctx =>
        {
            if (ctx.control.device is Pointer)
                isRotating = false;
            else isRotating = true;
        };

        rotateAction.performed += ctx =>
        {
			usandoMouse = ctx.control.device is Pointer;
			if (ctx.control.device is not Pointer)
                isRotating = true;
            else isRotating = enableRotateAction.IsPressed();
            if (isRotating)
                RotateCamera(ctx.ReadValue<Vector2>());
			R_Is_Cursor = ctx.control.device is Pointer;
		};

        zoomAction.performed += ctx => ZoomCamera(ctx.ReadValue<float>());
        moveCursorAction.performed += ctx => MoveCursor(ctx.ReadValue<Vector2>(),ctx.control.device);
    }
    bool R_Is_Cursor = false;

    private void OnDisable()
    {
        var map = inputActions.FindActionMap("OrbCam", true);
        map.Disable();

        enableRotateAction.started -= ctx => { if (ctx.control.device is Pointer) isRotating = true; };
        enableRotateAction.canceled -= ctx => { if (ctx.control.device is Pointer) isRotating = false; };
        rotateAction.performed -= ctx => 
        {
            var s = ctx.ReadValue<Vector2>();

			if (isRotating) 
                RotateCamera(s); 
            R_Is_Cursor = ctx.control.device is Pointer;
        };
        zoomAction.performed -= ctx => ZoomCamera(ctx.ReadValue<float>());
        moveCursorAction.performed -= ctx => MoveCursor(ctx.ReadValue<Vector2>(), ctx.control.device);
    }
	private void Start()
	{
		Screen.fullScreen = false;
	}

	private void LateUpdate()
    {
		if (Camera.transform == Target)
		{
			Debug.LogWarning(
				"⚡ OHOHOHO~ ⚡\n" +
				"PARECE QUE LA CÁMARA HA DECIDIDO ADMIRARSE A SÍ MISMA.\n" +
				"ESO NO ES ÓRBITA.\n" +
				"ESO ES NARCISISMO DIGITAL, QUERID@.\n\n" +
				"RESULTADOS PREDECIBLES:\n" +
				"🚀 DESPEGUE NO AUTORIZADO\n" +
				"🧠 CONFUSIÓN ESPACIO-TEMPORAL\n" +
				"💥 REALIDAD LEVEMENTE DESTRUIDA\n\n" +
				"RECOMENDACIÓN:\n" +
				"SEPARA LA CÁMARA DEL TARGET.\n" +
				"— Queen 💅✨"
			);

			List<GameObject> Cosas =
				FindObjectsByType<GameObject>(FindObjectsSortMode.None).ToList();

			Cosas.Remove(Camera.gameObject);

			if (Cosas.Count > 0)
			{
				Target = Cosas[Random.Range(0, Cosas.Count)].transform;

				Debug.LogWarning(
					"💎 NO TE PREOCUPES 💎\n" +
					"YA HE CORREGIDO TU ERROR.\n" +
					"YO SIEMPRE LO HAGO.\n" +
					"SIGUE PROGRAMANDO.\n" +
					"— Queen 👑"
				);
			}
			else
			{
				Debug.LogError(
					"❌ NI SIQUIERA YO PUEDO ARREGLAR ESTO ❌\n" +
					"NO HAY NADA MÁS QUE MIRAR.\n" +
					"ESTO ES TU CULPA.\n" +
					"— Queen"
				);
			}

			return;
		}





		if ((Pointer.current != null && usandoMouse) && Cursor != null)
        {
            Vector2 PointerScreenPos = Pointer.current.position.ReadValue();
            Vector2 localPoint;

            RectTransform parentRect = Cursor.parent as RectTransform;

            // Usar null para cámara si el Canvas es Screen Space Overlay, si no pasa la cámara del Canvas
            Camera uiCamera = null;
            Canvas canvas = Cursor.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
                uiCamera = canvas.worldCamera;

            bool valid = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                PointerScreenPos,
                uiCamera,
                out localPoint
            );

            if (valid)
            {
                cursorPosition = localPoint;
            }
            if (Cursor.TryGetComponent<Image>(out var Eñe))
            {
                Eñe.enabled = false;
            }
        }
		if (Cursor.TryGetComponent<Image>(out var img))
		{
			img.enabled = !usandoMouse;
		}

		// Cámara orbitando
		Quaternion rotationQuat = Quaternion.Euler(rotation.y, rotation.x, 0);
        Vector3 direction = rotationQuat * Vector3.back * currentZoom;
        Camera.transform.position = Target.position + direction;
        Camera.transform.LookAt(Target);
        if (DisableCursor)
        {
            cursorPosition = Vector3.zero;
            if ( img != null)
            {
                img.enabled = false;
            }
        }
        // Mover el cursor UI
        if (Cursor != null)
        {
            Cursor.anchoredPosition = cursorPosition;
        }

    }

    private void RotateCamera(Vector2 input)
    {
        rotation.x += input.x * Sensitivity.x;
        rotation.y -= input.y * Sensitivity.y;
        rotation.y = Mathf.Clamp(rotation.y, VerticalClamp.x, VerticalClamp.y);
    }

    private void ZoomCamera(float input)
    {
        if (EnableZoom)
        {
            currentZoom -= input * ZoomSpeed;

            if(currentZoom > ZoomLimits.y)
            {
                ZoomOutput = input;
            }
            else
            {
                ZoomOutput = 0;
            }

            currentZoom = Mathf.Clamp(currentZoom, ZoomLimits.x, ZoomLimits.y);
        }
        else
            ZoomOutput = input;
    }

	private void MoveCursor(Vector2 input, InputDevice device)
	{
        if (device is Gamepad)
            usandoMouse = false;
        if (DisableCursor)
            return;
        Debug.Log("Moving Cusor" + input);
		if (usandoMouse)
		{
			Debug.Log("Setting Cusor");

			SetCursor();
		}
		else
		{
			Debug.Log("A Moving Cusor" + input);

			cursorPosition += input * CursorSpeed;

		}
	}


	void SetCursor()
    {
        if (DisableCursor)
            return;
        cursorPosition = Pointer.current.position.value;
       

	}
}
