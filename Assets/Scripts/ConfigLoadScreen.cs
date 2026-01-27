using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
