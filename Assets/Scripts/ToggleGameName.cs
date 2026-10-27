using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// unico uso Easteregg
/// cambia Adaptica (el nombre normal del juego)
/// a Aduptica (el nombre del juego si se tiene el easteregg)
/// similar a  Minecraft y Minceraft
/// </summary>
public class ToggleGameName : MonoBehaviour
{
    public TMPro.TMP_Text Text;
    public float ChanceToAduptica = 0.01f; //probabilidad de cambiar el nombre a Aduptica cada vez que se inicia el juego
    public float AprilFoolsDayChance = 0.5f; //probabilidad de cambiar el nombre a Aduptica el 1 de abril
											 // Start is called before the first frame update
	void Start()
    {
        if (Text == null)
            return;
        Text.text = "Adaptica";
        if (System.DateTime.Now.Month == 4 && System.DateTime.Now.Day == 1) //si es el 1 de abril, se aumenta la probabilidad de cambiar el nombre a Aduptica
        {
            if (Random.value < AprilFoolsDayChance)
            {
                Text.text = "Aduptica";
                return;
			}
		}
		if (Random.value < ChanceToAduptica)
        {
            Text.text = "Aduptica";
		}
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
