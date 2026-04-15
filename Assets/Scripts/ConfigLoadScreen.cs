using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// classe para configurar las imágenes y spinner de la pantalla de carga
/// </summary>
[CreateAssetMenu(fileName = "Configuracion de pantalla de carga", menuName = "Config/ConfLoad")]
public class ConfigLoadScreen : ScriptableObject
{
	public Sprite Spinner;
	public Sprite CellLoadImg;
	public Sprite CreatureLoadImg;
	public Sprite TribeLoadImg;
	public Sprite FeudalLoadImg;
	public Sprite NationLoadImg;
	public Sprite SpaceLoadImg;
	public Sprite MainMenuLoadImg;
	public string[] Tips = new[]
	{
		"El Estadio Microbio No tiene depredadores aun",
		"ESTO ES UN PROTOTIPO",
	};
}
