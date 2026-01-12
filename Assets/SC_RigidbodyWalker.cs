using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class SC_RigidbodyWalker : MonoBehaviour
{
	[Header("Movimiento")]
	public float speed = 15f;
	public float lookSpeed = 2f;
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

	private float camYaw;
	private float camPitch;
	private Vector2 lookInput; // valor leído cada frame

	[Header("Gravedad")]
	public Transform gravityCenter;
	public float gravityAlignSpeed = 8f;

	[Header("Capas")]
	public LayerMask cameraCollisionMask; // SOLO planeta / relieve

	private Rigidbody camRb;
	private Collider camCollider;
	private Collider playerCollider;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true;
		rb.useGravity = false;
		rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

		playerCollider = GetComponent<Collider>();

		if (orbitCamera == null)
		{
			Debug.LogWarning(
				"🐐 Toriel:\n" +
				"Olvidaste asignar la cámara.\n" +
				"Está bien.\n" +
				"Pero no puedo proteger lo que no veo."
			);
			return;
		}

		camRb = orbitCamera.GetComponent<Rigidbody>();
		if (camRb == null)
		{
			camRb = orbitCamera.gameObject.AddComponent<Rigidbody>();
			Debug.Log(
				"🐐 Toriel:\n" +
				"He añadido un Rigidbody a la cámara.\n" +
				"Algunos efectos lo necesitan."
			);
		}

		camRb.useGravity = false;
		camRb.isKinematic = true;

		camCollider = orbitCamera.GetComponent<Collider>();
		if (camCollider != null)
		{
			Physics.IgnoreCollision(camCollider, playerCollider, true);
			Debug.Log(
				"🐐 Toriel:\n" +
				"La cámara y tú ya no pueden hacerse daño.\n" +
				"A veces, la distancia es protección."
			);
		}

		Debug.Log(
			"🐐 Toriel:\n" +
			"Todo está listo.\n" +
			"Puedes moverte con seguridad."
		);
	}

	// --- OnEnable (reemplaza la sección actual) ---
	private void OnEnable()
	{
		map = inputActions.FindActionMap("OrbCam", true);

		map.Enable();

		moveAction = map.FindAction("L", true);
		rotateAction = map.FindAction("R", true);
		enableRotateAction = map.FindAction("EnableR", true);
		zoomAction = map.FindAction("Zoom", false);



		// Guardamos el valor del input (performed) pero NO aplicamos la rotación aquí.
		rotateAction.performed += ctx =>
		{
			lookInput = ctx.ReadValue<Vector2>();
			Mouse_Finger_Pen = (ctx.control.device is Pointer)
			;
		};
		rotateAction.canceled += ctx =>
		{
			lookInput = Vector2.zero;
		};

		if (zoomAction != null)
			zoomAction.performed += ctx => ZoomCamera(ctx.ReadValue<float>());
	}
	bool Mouse_Finger_Pen;
	// --- OnDisable (limpiar handlers bien) ---
	private void OnDisable()
	{


		if (rotateAction != null)
		{
			rotateAction.performed -= ctx => { lookInput = ctx.ReadValue<Vector2>(); };
			rotateAction.canceled -= ctx => { lookInput = Vector2.zero; };
		}

		map?.Disable();
	}

	// --- FixedUpdate (solo el trozo de rotación del jugador) ---
	private void FixedUpdate()
	{
		if (!map.enabled)
			map.Enable();//ACTIVAR EL MAPA POR SI A DON SELECTOR SE HACE SU TRABAJO TARDE Y CAUSA QUIE SE DESACTIVE E MAPA

		Vector2 input = moveAction.ReadValue<Vector2>();
		Vector3 move = (transform.forward * input.y + transform.right * input.x) * speed;

		// Girar jugador según input.x SI isRotating (tu decisión)
		if (true)
		{
			float yawInput = input.x; // raw input.x
			RotatePlayer(yawInput);
			Debug.Log("🐐 Toriel — ROTANDO JUGADOR");
		}

		Vector3 vel = rb.velocity;
		Vector3 targetVel = new Vector3(move.x, vel.y, move.z);
		rb.velocity = Vector3.Lerp(vel, targetVel, Time.fixedDeltaTime * 10f);
		// --- Gravedad personalizada hacia el planeta ---
		if (gravityCenter != null)
		{
			Vector3 gravityDir =
				(gravityCenter.position - transform.position).normalized;

			float gravityStrength = 25f; // ajusta a gusto
			rb.AddForce(gravityDir * gravityStrength, ForceMode.Acceleration);

			// Alinear el "up" del jugador con la gravedad
			Quaternion targetRotation =
				Quaternion.FromToRotation(transform.up, -gravityDir) * transform.rotation;

			transform.rotation = Quaternion.Slerp(
				transform.rotation,
				targetRotation,
				Time.fixedDeltaTime * gravityAlignSpeed
			);
		}

	}

	// --- LateUpdate (reemplaza completamente tu LateUpdate con esto) ---
	// --- LateUpdate: cámara orbital correcta para planeta (yaw alrededor de gravityUp)
	// --- LateUpdate: cámara orbital limpia, solo InputActions ---
	private void LateUpdate()
	{
		if (!orbitCamera || !gravityCenter)
			return;

		// 1) Arriba real del planeta
		Vector3 gravityUp = (transform.position - gravityCenter.position).normalized;

		// 2) Leer input SOLO desde InputAction
		Vector2 look = Vector2.zero;
		if (rotateAction != null)
			look = rotateAction.ReadValue<Vector2>();

		// Deadzone mínima (evita drift)
		const float deadzone = 0.001f;
		if (look.sqrMagnitude < deadzone * deadzone)
			look = Vector2.zero;

		// 3) Decidir si se permite rotar
		//    EnableR SOLO filtra mouse si así lo configuraste en el InputActionAsset
		bool shouldRotate = true;
		if (enableRotateAction != null && !enableRotateAction.IsPressed())
			shouldRotate = false;
		Quaternion finalRot = ApplyCameraRotationFromInput(look, gravityUp, shouldRotate);
		// 6) Posición orbital
		Vector3 desiredPos = transform.position
						   - finalRot * Vector3.forward * currentZoom;

		// 7) Colisión de cámara
		Vector3 dir = (desiredPos - transform.position).normalized;
		if (Physics.SphereCast(
			transform.position,
			cameraCollisionRadius,
			dir,
			out RaycastHit hit,
			currentZoom,
			cameraCollisionMask,
			QueryTriggerInteraction.Ignore))
		{
			desiredPos = hit.point - dir * 0.05f;
		}

		// 8) Aplicar
		orbitCamera.transform.position = desiredPos;
		orbitCamera.transform.rotation = finalRot;
	}



	/// <summary>
	/// Aplica un Vector2 de input (look) a la rotación de la cámara respetando gravityUp.
	/// - No lee dispositivos: recibe el vector de input (ej: rotateAction.ReadValue<Vector2()).
	/// - Si allowRotate == false no cambia camYaw/camPitch pero devuelve la rotación actual.
	/// - Devuelve la rotación final (sin roll) que debes aplicar a la cámara.
	/// </summary>
	public Quaternion ApplyCameraRotationFromInput(Vector2 lookInput, Vector3 gravityUp, bool allowRotate = true)
	{
		// tiny deadzone to avoid tiny drift (action bindings may generate small noise)
		const float deadzone = 0.001f;
		if (allowRotate && lookInput.sqrMagnitude > deadzone * deadzone)
		{
			// scale por frame para que sensitivity sea más intuitiva (opcional)
			float scale = Time.deltaTime * 60f;
			camYaw += lookInput.x * sensitivity.x * scale;
			camPitch -= lookInput.y * sensitivity.y * scale;

			// clamp pitch to avoid flipping over
			camPitch = Mathf.Clamp(camPitch, verticalClamp.x, verticalClamp.y);
		}

		// Yaw: rotación alrededor del "up" real del planeta
		Quaternion yawRot = Quaternion.AngleAxis(camYaw, gravityUp);

		// Right axis POST-yaw — garantiza que pitch ocurra perpendicular al up planetario
		Vector3 rightAxis = (yawRot * Vector3.right).normalized;

		// Pitch: rotación alrededor del eje right real
		Quaternion pitchRot = Quaternion.AngleAxis(camPitch, rightAxis);

		// Orden: yaw * pitch. No hay roll en esta construcción.
		Quaternion finalRot = yawRot * pitchRot;

		return finalRot;
	}




	// Simplifica la API: RotatePlayer acepta YA el input (no un Vector2 confuso)
	private void RotatePlayer(float yawInput)
	{
		float yaw = yawInput * lookSpeed;
		transform.Rotate(transform.up, yaw, Space.World);
	}


	private void RotateCamera(Vector2 input)
	{
		// (No lo usamos directamente si leemos lookInput en LateUpdate; lo dejamos si quieres usarlo)
		camYaw += input.x * sensitivity.x;
		camPitch -= input.y * sensitivity.y;
		camPitch = Mathf.Clamp(camPitch, verticalClamp.x, verticalClamp.y);
	}

	private void ZoomCamera(float input)
	{
		currentZoom -= input * zoomSpeed;
		currentZoom = Mathf.Clamp(currentZoom, zoomLimits.x, zoomLimits.y);

		if (Mathf.Approximately(currentZoom, zoomLimits.x))
		{
			Debug.Log(
				"🐐 Toriel:\n" +
				"Eso es suficiente.\n" +
				"No necesitas acercarte más."
			);
		}
	}

}
