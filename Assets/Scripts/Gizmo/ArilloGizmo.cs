using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// AY NO TYPO es AnilloGizmo no ArilloGizmo XD 
/// Componente para el anillo de rotación de un gizmo de transformación
/// </summary>
[RequireComponent(typeof(Collider))]
public class ArilloGizmo : MonoBehaviour
{
    //espacio en blanco sospechoso



    public MultiEje eje;
    private GizmoManager manager;
    private bool arrastrando = false;
    private Vector3 ultimaPosicionPointer;
    public InputActionAsset inputActionsAsset;
    private InputAction dragAction;
    private InputAction pointerPositionAction;
	private InputDevice dispositivoActivo;
	private Vector2 ultimoDelta;


	private void Awake()
    {
        manager = GetComponentInParent<GizmoManager>();
        if (manager == null)
        {
            Debug.LogError("FlechaGizmo requiere que el GizmoManager esté en el padre.");
        }

        // Busca el mapa y las acciones dentro del asset (por nombre)
        var map = inputActionsAsset.FindActionMap("GC", true);
        dragAction = map.FindAction("Drag", true);
        pointerPositionAction = map.FindAction("PPos", true);
		dragAction.performed += ctx =>
		{
			//vacio como la empatia de los politicos 
		};

		dragAction.started += ctx => ComenzarArrastre(ctx.control.device);
        dragAction.canceled += ctx => TerminarArrastre();
    }
    private void ComenzarArrastre(InputDevice Dev)
    {

        Vector2 PointerPos = pointerPositionAction.ReadValue<Vector2>();
        if (Dev is Gamepad)
            PointerPos = manager.Cursor.position;
		dispositivoActivo = Dev;

		Ray ray = Camera.main.ScreenPointToRay(PointerPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider == GetComponent<Collider>())
            {
                arrastrando = true;
                Debug.Log($"Gizmo {this.eje} Girando");
                ultimaPosicionPointer = PointerPos;
            }
            else
            {
                arrastrando = false;
            }
        }
        else
        {
            arrastrando = false;
        }
    }

    private void TerminarArrastre()
    {
        arrastrando = false;
    }
    private void OnEnable()
    {
        dragAction.Enable();
        pointerPositionAction.Enable();
    }

    private void OnDisable()
    {
        dragAction.Disable();
        pointerPositionAction.Disable();
    }

    private void Update()
    {
        if (!arrastrando) return;

		Vector2 deltaPantalla;

		if (dispositivoActivo is Gamepad)
		{
			deltaPantalla = ultimoDelta * Time.deltaTime * 100f;
		}
		else
		{
			Vector2 posActual = pointerPositionAction.ReadValue<Vector2>();
			deltaPantalla = posActual - (Vector2)ultimaPosicionPointer;
			ultimaPosicionPointer = posActual;
		}



        manager.RotarPorEje(eje, deltaPantalla, 30);
    }
    private void OnPointerDown()
    {

        arrastrando = true;
        ultimaPosicionPointer = pointerPositionAction.ReadValue<Vector2>();
    }

    private void OnPointerUp()
    {

        arrastrando = false;
    }


}
