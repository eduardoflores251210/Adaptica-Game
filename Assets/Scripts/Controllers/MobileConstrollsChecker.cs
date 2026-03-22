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
	public bool DOITANIWAYS;
	public bool Disable;
	public GameObject Controls;

	// Start is called before the first frame update
	void Start()
	{
		foreach( var de in InputSystem.devices)
        {
            if (de is Touchscreen)
                DOITANIWAYS = true;
			else 
				DOITANIWAYS = false;
        }
	}

	// Update is called once per frame
	void Update() //por que obiamente tenemos que siempre ver el estado de los controles
	{
		Disable = !Application.isMobilePlatform;
		
		Controls.SetActive(!Disable || DOITANIWAYS);
		if (Controls == null)
		{
			Destroy(this); return;
		}
		
	}
}
