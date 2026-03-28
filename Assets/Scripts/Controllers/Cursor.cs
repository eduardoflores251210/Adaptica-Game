using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

public class Cursor : MonoBehaviour
{
	public GizmoManager Mgr;
	public InputActionAsset inputActions;

	public event Action ClickA;
	public event Action ClickB;

	private InputAction LClick;
	private InputAction RClick;


	private Camera la_camara;


	void Start()
	{
		la_camara = Camera.main;
	}

	void OnEnable()
	{
		inputActions.FindActionMap("Cursor", true).Enable();
		LClick = inputActions.FindAction("Click A");
		RClick = inputActions.FindAction("Click B");
		LClick.performed += ctx => { OnClickA(); };
		RClick.performed += ctx => { OnClickB(); };
	}

	void OnDisable()
	{

		inputActions.FindActionMap("Cursor", true).Disable();
	}

	void OnClickA()
	{
		if (Mgr == null) { }
		else
		{
			GameObject clickedObject = RaycastClick(true);

			if (clickedObject != null && Mgr != null)
			{
				Mgr.objetoActivo = clickedObject.transform;
			}
			else if (clickedObject == null)
			{
				Mgr.objetoActivo = null;
			}
		}
		ClickA?.Invoke();
	}

	void OnClickB()
	{
		// vacio temproalmente
		ClickB?.Invoke();
	}

	GameObject RaycastClick(Boolean IsLClick)
	{
		// Obtener la posición en Mundo del cursor UI
		Vector2 sdfsdfsdfsdf = ((RectTransform)transform).position;

		// Crear un rayo desde la cámara usando esa posición
		Ray ray = la_camara.ScreenPointToRay(sdfsdfsdfsdf);


		if (IsLClick)
		{
			if (Mgr == null) return null;
			// Ejecutar raycast 3D
			if (Physics.Raycast(ray, out RaycastHit hitInfo))
			{
				// si es un objeto no sin el componente mobible NO ASIGNAR al mover
				if (!hitInfo.collider.gameObject.TryGetComponent<GizSelectable>(out var a))
				{
					if (Mgr.objetoActivo == null) { return null; }
					return Mgr.objetoActivo.gameObject;
				}
				return hitInfo.collider.gameObject;
			}


		}

		return null;
	}
	public GameObject GetObjInCursor()
	{
		// Obtener la posición en Mundo del cursor UI
		Vector2 sdfsdfsdfsdf = ((RectTransform)transform).position;

		// Crear un rayo desde la cámara usando esa posición
		Ray ray = la_camara.ScreenPointToRay(sdfsdfsdfsdf);





		// Ejecutar raycast 3D
		if (Physics.Raycast(ray, out RaycastHit hitInfo))
		{

			return hitInfo.collider.gameObject;
		}




		return null;
	}






}
