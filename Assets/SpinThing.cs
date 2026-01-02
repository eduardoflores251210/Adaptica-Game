using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpinThing : MonoBehaviour
{
	public ObjectPlacer placer;        // Referencia al ObjectPlacer para acceder a selectedObject
	public Camera cam;                  // Cámara para raycast

	// Input System
	public InputActionAsset inputActions;
	private InputActionMap map;
	private InputAction cursorAction;
	private InputAction ClickAction;

	private Vector2 lastCursorPos;
	public bool isRotating = false;

	void OnEnable()
	{
		map = inputActions.FindActionMap("GC", true);
		map.Enable();
		cursorAction = map.FindAction("PPos", true);
		ClickAction = map.FindAction("Click", true);
		cursorAction.Enable();
		ClickAction.Enable();
	}

	void OnDisable()
	{
		cursorAction.Disable();
		ClickAction.Disable();
		map.Disable();
	}

	void Update()
	{
		if (placer.selectedObject == null) return;

		Vector2 cursorPos = cursorAction.ReadValue<Vector2>();
		Ray ray = cam.ScreenPointToRay(cursorPos);
		RaycastHit hit;

		// Solo rotar si el cursor está sobre el gizmo (este objeto)
		if (ClickAction.IsPressed() && Physics.Raycast(ray, out hit))
		{
			if (hit.collider.gameObject == gameObject)
			{
				placer.CantMove = true;
				if (!isRotating)
				{
					isRotating = true;
					lastCursorPos = cursorPos;
					placer.CantMove = true;

				}
				else
				{
					Vector2 delta = cursorPos - lastCursorPos;
					float rotationY = delta.x; // puedes multiplicar por sensibilidad
					placer.selectedObject.transform.Rotate(Vector3.up, rotationY, Space.Self);
					lastCursorPos = cursorPos;
				}
			}
		}
		else
		{
			placer.CantMove =false;

			isRotating = false;
		}
	}
}
