using System.Collections;
using System.Collections.Generic;//usings inecesarios 
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Ni me acordaba de la existencia de este script
/// pero supongo que pense que ERA BUENA IDEA PORTEAR UN JUEGO INEFICIENTE A MOVILES
/// </summary>
public class MobileConstrollsChecker : MonoBehaviour
{
    public InputActionAsset inputActions;

    public InputAction Check;

    public bool Disable;
    public GameObject Controls;

    // Start is called before the first frame update
    void Start()
    {
        var map = inputActions.FindActionMap("GC", true); //No no es Gamecube
        map.Enable();

        Check = map.FindAction("PPos", true); //no no es tartamudeo
        Check.started += ctx =>
        {
            if (ctx.control.device is Pointer)
                Disable = true;
        };
    }

    // Update is called once per frame
    void Update() //por que obiamente tenemos que siempre ver el estado de los controles
    {
        if (Disable)
        {
            Controls.SetActive(!Disable);
        }
    }
}
