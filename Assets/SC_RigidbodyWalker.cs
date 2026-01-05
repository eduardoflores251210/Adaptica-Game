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

	private bool isRotating = false;
	private bool camIsRotating = false;

	[Header("Cámara Orbital")]
	public Camera orbitCamera;
	public float currentZoom = 6f;
	public float zoomSpeed = 5f;
	public Vector2 zoomLimits = new Vector2(2f, 30f);
	public Vector2 sensitivity = Vector2.one;
	public Vector2 verticalClamp = new Vector2(-80f, 80f);
	public float cameraCollisionRadius = 0.15f;

	private Vector2 camRotation;

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

	private void OnEnable()
	{
		map = inputActions.FindActionMap("OrbCam", true);
		map.Enable();

		moveAction = map.FindAction("L", true);
		rotateAction = map.FindAction("R", true);
		enableRotateAction = map.FindAction("EnableR", true);
		zoomAction = map.FindAction("Zoom", false);

		enableRotateAction.started += _ => { isRotating = camIsRotating = true; };
		enableRotateAction.canceled += _ => { isRotating = camIsRotating = false; };

		rotateAction.performed += ctx =>
		{
			Vector2 value = ctx.ReadValue<Vector2>();

			Debug.Log(
				"🐐 Toriel — INPUT RECIBIDO\n" +
				$"Dispositivo: {ctx.control.device.displayName}\n" +
				$"Valor: {value}\n" +
				$"isRotating: {isRotating}\n" +
				$"camIsRotating: {camIsRotating}"
			);

			if (isRotating)
			{
				Debug.Log("🐐 Toriel — ROTANDO JUGADOR");
				RotatePlayer(value);
			}

			if (camIsRotating)
			{
				Debug.Log("🐐 Toriel — ROTANDO CÁMARA");
				RotateCamera(value);
			}
		};


		if (zoomAction != null)
			zoomAction.performed += ctx => ZoomCamera(ctx.ReadValue<float>());
	}

	private void OnDisable()
	{
		map.Disable();
	}

	private void FixedUpdate()
	{
		map.Enable();

		Vector2 input = moveAction.ReadValue<Vector2>();
		Vector3 move =
			(transform.forward * input.y + transform.right * input.x) * speed;

		Vector3 vel = rb.velocity;
		Vector3 targetVel = new Vector3(move.x, vel.y, move.z);
		rb.velocity = Vector3.Lerp(vel, targetVel, Time.fixedDeltaTime * 10f);
	}

	private void RotatePlayer(Vector2 look)
	{
		rotation.x += look.x * lookSpeed;
		rotation.y -= look.y * lookSpeed;
		rotation.y = Mathf.Clamp(rotation.y, lookXLimit.x, lookXLimit.y);
		transform.rotation = Quaternion.Euler(0, rotation.x, 0);
	}

	private void RotateCamera(Vector2 input)
	{
		camRotation.x += input.x * sensitivity.x;
		camRotation.y -= input.y * sensitivity.y;
		camRotation.y = Mathf.Clamp(camRotation.y, verticalClamp.x, verticalClamp.y);
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

	private void LateUpdate()
	{
		if (orbitCamera == null || gravityCenter == null)
			return;

		Vector3 gravityUp =
			(transform.position - gravityCenter.position).normalized;

		Quaternion orbitRot =
			Quaternion.Euler(camRotation.y, camRotation.x, 0);

		Vector3 desiredPos =
			transform.position + orbitRot * Vector3.back * currentZoom;

		Vector3 dir = (desiredPos - transform.position).normalized;
		float dist = Vector3.Distance(transform.position, desiredPos);

		if (Physics.SphereCast(
			transform.position,
			cameraCollisionRadius,
			dir,
			out RaycastHit hit,
			dist,
			cameraCollisionMask,
			QueryTriggerInteraction.Ignore))
		{
			desiredPos = hit.point - dir * 0.05f;

			Debug.Log(
				"🐐 Toriel:\n" +
				"Hay una montaña delante.\n" +
				"He movido la cámara para protegerte."
			);
		}

		orbitCamera.transform.position = desiredPos;

		Vector3 lookDir =
			(transform.position - orbitCamera.transform.position).normalized;

		Quaternion targetRot =
			Quaternion.LookRotation(lookDir, gravityUp);

		orbitCamera.transform.rotation =
			Quaternion.Slerp(
				orbitCamera.transform.rotation,
				targetRot,
				Time.deltaTime * gravityAlignSpeed
			);

	}
}
