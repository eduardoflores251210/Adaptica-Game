using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
//using static UnityEngine.Rendering.DebugUI;//ugh DEJA DE AÑADIR USINGS TONTOS INTELISENSE

public class LocalizeMainMenu : MonoBehaviour
{
    public string TableName = "MainMenu";
    public List<TMPro.TMP_Text> LocalizedTexts = new List<TMPro.TMP_Text>();
    public SettingsManager manager;
    bool added = false;
    bool FirstUpdate = true;
	// Start is called before the first frame update
	void Start()
    {
        Localize();
    }
    void Localize()
    {
		foreach (var aa in LocalizedTexts)
		{
			aa.text = LocalizationSettings.StringDatabase.GetLocalizedString(
	TableName,
	aa.gameObject.name); //el texto que se muestra en el editor es el mismo que se busca en la tabla de localización 
		}
	}
    // Update is called once per frame
    void Update()
    {
        if(FirstUpdate)
        {
            Localize(); //localiza al iniciar el juego por si Start() hizo algo raro;
			FirstUpdate = false;
            
		}

		if (manager.enabled)
        {
            if (!added)
            {
                added = true;
                manager.OnLanguageChange += Localize; //cuando se cambie el idioma, se llama a la función Localize para actualizar los textos
			}
        }
    }
}
//pd SI soporto Nahuatl (nh) aunque es traduccion provisional y es mas para probar como localizare Locales con idiomas No soportados por unity.
