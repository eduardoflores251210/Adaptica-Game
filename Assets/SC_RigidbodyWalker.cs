using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
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
	private InputActionMap map;
	private bool isRotating = false;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true;
		rb.useGravity = true;
		rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		rotation.y = transform.eulerAngles.y;
	}

	private void OnEnable()
	{
		map = inputActions.FindActionMap("OrbCam", true);
		map.Enable();

		moveAction = map.FindAction("L", true);
		rotateAction = map.FindAction("R", true);
		enableRotateAction = map.FindAction("EnableR", true);

		enableRotateAction.started += ctx => isRotating = true;
		enableRotateAction.canceled += ctx => isRotating = false;

		rotateAction.performed += ctx =>
		{
			if (isRotating)
				RotatePlayer(ctx.ReadValue<Vector2>());
		};
	}

	private void OnDisable()
	{
		map.Disable();
		enableRotateAction.started -= ctx => isRotating = true;
		enableRotateAction.canceled -= ctx => isRotating = false;
		rotateAction.performed -= ctx => { if (isRotating) RotatePlayer(ctx.ReadValue<Vector2>()); };
	}

	private void FixedUpdate()
	{
		Vector2 input = moveAction.ReadValue<Vector2>();
		Vector3 forward = transform.forward;
		Vector3 right = transform.right;
		Vector3 targetVelocity = (forward * input.y + right * input.x) * speed;

		Vector3 velocity = transform.InverseTransformDirection(rb.velocity);
		velocity.y = 0;
		velocity = transform.TransformDirection(velocity);

		Vector3 velocityChange = transform.InverseTransformDirection(targetVelocity - velocity);
		velocityChange.x = Mathf.Clamp(velocityChange.x, -10f, 10f);
		velocityChange.z = Mathf.Clamp(velocityChange.z, -10f, 10f);
		velocityChange.y = 0;
		velocityChange = transform.TransformDirection(velocityChange);

		rb.AddForce(velocityChange, ForceMode.VelocityChange);
	}

	private void RotatePlayer(Vector2 look)
	{
		rotation.x += look.x * lookSpeed;
		rotation.y -= look.y * lookSpeed;
		rotation.y = Mathf.Clamp(rotation.y, lookXLimit.x, lookXLimit.y);

		Quaternion rot = Quaternion.Euler(rotation.y, rotation.x, 0f);
		transform.rotation = rot;
	}
}
