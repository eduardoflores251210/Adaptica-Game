using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class SC_RigidbodyWalker : MonoBehaviour
{ 
	[Header("Movimiento")]
	public float speed = 15f;
	public float lookSpeed = 120f; // grados por segundo cuando rotas el jugador hacia la cámara
	public Vector2 lookXLimit = new Vector2(-60f, 60f);

	private Rigidbody rb;
	private Vector2 rotation = Vector2.zero;

	[Header("Input System")]
	public InputActionAsset inputActions;
	private InputAction moveAction;
	private InputAction rotateAction;
	private InputAction enableRotateAction;
	private InputAction zoomAction;
	private InputActionMap map;

	[Header("Cámara Orbital")]
	public Camera orbitCamera;
	public float currentZoom = 6f;
	public float zoomSpeed = 5f;
	public Vector2 zoomLimits = new Vector2(2f, 30f);
	public Vector2 sensitivity = Vector2.one;
	public Vector2 verticalClamp = new Vector2(-80f, 80f);
	public float cameraCollisionRadius = 0.15f;

	public LayerMask cameraCollisionMask; // capas que bloquean la cámara

	private float camYaw;
	private float camPitch;
	private Vector2 lookInput; // valor leído cada frame desde InputSystem

	[Header("Gravedad")]
	public Transform gravityCenter;
	public float gravityStrength = 25f;
	public float gravityAlignSpeed = 8f;

	[Header("Estado & utilidades")]
	public float ZoomOutput = 0f;
	bool usingPointer = false;
	bool isRotating = false;
	bool mouseFingerPen = false;

	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true;           // controlamos rotación manualmente
		rb.useGravity = false;              // usamos nuestra gravedad personalizada
		rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

		if (orbitCamera == null)
		{
			Debug.LogWarning("My Child you need A camera.");
		}
	}

	// ---------- Input subscription ----------
	private void OnEnable()
	{
		if (inputActions == null)
		{
			Debug.LogError("My Child, You need to assing controls");
			return;
		}

		map = inputActions.FindActionMap("OrbCam", true);
		map.Enable();

		moveAction = map.FindAction("L", true);        // movimiento (left stick / WASD)
		rotateAction = map.FindAction("R", true);     // look/rotate
		enableRotateAction = map.FindAction("EnableR", true); // habilitar rotación con botón
		zoomAction = map.FindAction("Zoom", false);   // eje de zoom (mouse wheel / triggers)

		// subscribe
		if (rotateAction != null)
		{
			rotateAction.performed += OnRotatePerformed;
			rotateAction.canceled += OnRotateCanceled;
		}

		if (enableRotateAction != null)
		{
			enableRotateAction.started += OnEnableRotateStarted;
			enableRotateAction.canceled += OnEnableRotateCanceled;
		}

		if (zoomAction != null)
			zoomAction.performed += OnZoomPerformed;
	}

	private void OnDisable()
	{
		if (rotateAction != null)
		{
			rotateAction.performed -= OnRotatePerformed;
			rotateAction.canceled -= OnRotateCanceled;
		}

		if (enableRotateAction != null)
		{
			enableRotateAction.started -= OnEnableRotateStarted;
			enableRotateAction.canceled -= OnEnableRotateCanceled;
		}

		if (zoomAction != null)
			zoomAction.performed -= OnZoomPerformed;

		map?.Disable();
	}

	// ---------- Input callbacks ----------
	private void OnRotatePerformed(InputAction.CallbackContext ctx)
	{
		usingPointer = ctx.control.device is Pointer;
		// Si el dispositivo no es pointer (gamepad), siempre rotamos.
		if (ctx.control.device is not Pointer)
			isRotating = true;
		else
			isRotating = enableRotateAction == null ? true : enableRotateAction.IsPressed();

		lookInput = ctx.ReadValue<Vector2>();
		mouseFingerPen = ctx.control.device is Pointer;
	}

	private void OnRotateCanceled(InputAction.CallbackContext ctx)
	{
		lookInput = Vector2.zero;
		// si es pointer, dejamos de rotar (salvo que EnableR esté presionado)
		if (ctx.control.device is Pointer)
			isRotating = enableRotateAction != null && enableRotateAction.IsPressed();
		else
			isRotating = false;
	}

	private void OnEnableRotateStarted(InputAction.CallbackContext ctx)
	{
		// solo tiene sentido para pointer; si es pointer y aprietas el botón, habilita rotar
		if (ctx.control.device is Pointer)
			isRotating = true;
	}

	private void OnEnableRotateCanceled(InputAction.CallbackContext ctx)
	{
		if (ctx.control.device is Pointer)
			isRotating = false;
	}

	private void OnZoomPerformed(InputAction.CallbackContext ctx)
	{
		ZoomCamera(ctx.ReadValue<float>());
	}

	// ---------- Físicas: mover & gravedad ----------
	private void FixedUpdate()
	{
		// asegurar que el mapa está activo (defensivo)
		if (map != null && !map.enabled) map.Enable();

		// Movimiento basado en transform.forward/right (transform ya alineado con gravedad)
		Vector2 input = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
		Vector3 desiredMove = (transform.forward * input.y + transform.right * input.x) * speed;

		// Mantener componente Y actual (para gravedad)
		Vector3 vel = rb.velocity;
		Vector3 targetVel = new Vector3(desiredMove.x, vel.y, desiredMove.z);
		rb.velocity = Vector3.Lerp(vel, targetVel, Time.fixedDeltaTime * 10f);

		// Rotar jugador: si hay input de movimiento, orientarlo hacia la dirección de la cámara (su proyección)
		if (input.sqrMagnitude > 0.001f && orbitCamera != null)
		{
			Vector3 gravityUp = (transform.position - (gravityCenter ? gravityCenter.position : Vector3.zero)).normalized;
			// dirección hacia adelante de cámara, proyectada en plano local (sin componente en gravedadUp)
			Vector3 camForward = Vector3.ProjectOnPlane(orbitCamera.transform.forward, gravityUp).normalized;
			if (camForward.sqrMagnitude > 0.001f)
			{
				Quaternion targetRot = Quaternion.LookRotation(camForward, -gravityUp); // "up" del jugador es -gravityDir
				transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, lookSpeed * Time.fixedDeltaTime);
			}
		}
		else
		{
			// si no hay input, no forzamos rotación (puedes cambiar esto si prefieres rotación con stick X)
		}

		// Gravedad personalizada hacia gravityCenter
		if (gravityCenter != null)
		{
			Vector3 gravityDir = (gravityCenter.position - transform.position).normalized;
			rb.AddForce(gravityDir * gravityStrength, ForceMode.Acceleration);

			// Alineamos el "up" del jugador (su transform.up) hacia -gravityDir (porque gravityDir apunta hacia el centro)
			Quaternion targetAlign = Quaternion.FromToRotation(transform.up, -gravityDir) * transform.rotation;
			transform.rotation = Quaternion.Slerp(transform.rotation, targetAlign, Time.fixedDeltaTime * gravityAlignSpeed);
		}
	}

	// ---------- Cámara orbital (LateUpdate para evitar jitter) ----------
	private void LateUpdate()
	{
		if (orbitCamera == null || gravityCenter == null)
			return;

		// gravedad "up" real local respecto al planeta
		Vector3 gravityUp = (transform.position - gravityCenter.position).normalized;

		// decide si permitimos rotación: si usamos pointer, respetamos isRotating; si es gamepad, siempre true
		bool allowRotate = true;
		if (usingPointer)
			allowRotate = isRotating;

		Quaternion finalRot = ApplyCameraRotationFromInput(lookInput, gravityUp, allowRotate);

		// posición deseada (orbital) tomando en cuenta el "up" real del planeta
		Vector3 desiredPos = transform.position - finalRot * Vector3.forward * currentZoom;

		// colisión de cámara (spherecast desde jugador hacia desiredPos)
		Vector3 dir = (desiredPos - transform.position);
		float dist = dir.magnitude;
		if (dist > 0.001f)
		{
			dir /= dist;
			if (Physics.SphereCast(transform.position, cameraCollisionRadius, dir, out RaycastHit hit, dist, cameraCollisionMask, QueryTriggerInteraction.Ignore))
			{
				// poner la cámara justo antes del impacto
				desiredPos = hit.point - dir * 0.05f;
			}
		}

		orbitCamera.transform.position = desiredPos;
		orbitCamera.transform.rotation = finalRot;
	}

	/// <summary>
	/// Aplica lookInput (Vector2) para generar la rotación de cámara alrededor de gravityUp.
	/// Mismo comportamiento que expliqué en la versión plana, pero adaptado a esfera.
	/// </summary>
	public Quaternion ApplyCameraRotationFromInput(
		Vector2 lookInput,
		Vector3 gravityUp,
		bool allowRotate = true)
	{
		const float deadzone = 0.001f;

		if (allowRotate && lookInput.sqrMagnitude > deadzone * deadzone)
		{
			float scale = Time.deltaTime * 60f;
			camYaw += lookInput.x * sensitivity.x * scale;
			camPitch -= lookInput.y * sensitivity.y * scale;
			camPitch = Mathf.Clamp(camPitch, verticalClamp.x, verticalClamp.y);
		}

		// 1️⃣ Forward base: perpendicular al up del planeta
		Vector3 forward = Vector3.ProjectOnPlane(Vector3.forward, gravityUp).normalized;

		// 2️⃣ Yaw alrededor del up real del planeta
		Quaternion yawRot = Quaternion.AngleAxis(camYaw, gravityUp);
		forward = yawRot * forward;

		// 3️⃣ Right REAL (ortogonal garantizado)
		Vector3 right = Vector3.Cross(gravityUp, forward).normalized;

		// 4️⃣ Pitch alrededor del right real
		Quaternion pitchRot = Quaternion.AngleAxis(camPitch, right);
		forward = pitchRot * forward;

		// 5️⃣ Reconstruir rotación SIN roll
		return Quaternion.LookRotation(forward, gravityUp);
	}


	// ---------- Zoom ----------
	private void ZoomCamera(float input)
	{
		currentZoom -= input * zoomSpeed;
		if (currentZoom > zoomLimits.y) ZoomOutput = input;
		else ZoomOutput = 0f;
		currentZoom = Mathf.Clamp(currentZoom, zoomLimits.x, zoomLimits.y);
		if (Mathf.Approximately(currentZoom, zoomLimits.x))
		{
		}
	}
}
