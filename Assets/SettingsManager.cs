using StandartUtilities; //remanente de depuracion, lo usaba para convertir lista a string
using System; //no lo uso pero se ve raro sin esto
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Ent32 = System.Int32;
using Console = ConsoleManager;

public class SettingsManager : MonoBehaviour
{

	[Header("Audio Settings")]
	public Slider musicSlider;
	public Slider MasterSlider;
	public Slider SFXSlider;
	public AudioMixer AudioMixer;

	[Header("Display Settings")]
	public Toggle fullscreenToggle;
	public TMP_Dropdown resolutionDropdown;
	public TMP_Text resolutionTextTag;
	public TMP_Text LanguageTextTag;
	public TMP_Text MasterVolumeTextTag;
	public TMP_Text MusicVolumeTextTag;
	public TMP_Text SFXSliderVolumeTextTag;
	public TMP_Text FullscreenTextTag;
	[Header("Other Settings")]
	public TMP_Dropdown LangDrop;
	public string table;
	private string CurrentLocaleCode = "nh"; // Nahuatl predeterminado
	private Resolution[] resolutions;
	public event Action OnLanguageChange;

	private void Start()
	{



		// --------------------
		// Configuración de Audio
		// --------------------
		musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
		ApplyVolume(musicSlider.value, AudioGroups.Music);
		musicSlider.onValueChanged.AddListener(ApplyMusicVolume);
		musicSlider.onValueChanged.AddListener(value => PlayerPrefs.SetFloat("MusicVolume", value));

		SFXSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
		ApplyVolume(SFXSlider.value, AudioGroups.SFX);
		SFXSlider.onValueChanged.AddListener(ApplySFXVolume);
		SFXSlider.onValueChanged.AddListener(value => PlayerPrefs.SetFloat("SFXVolume", value));

		MasterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
		ApplyVolume(MasterSlider.value, AudioGroups.Master);
		MasterSlider.onValueChanged.AddListener(ApplyMasterVolume);
		MasterSlider.onValueChanged.AddListener(value => PlayerPrefs.SetFloat("MasterVolume", value));

		// --------------------
		// Configuración de Fullscreen
		// --------------------
		fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
		ApplyFullscreen(fullscreenToggle.isOn);
		fullscreenToggle.onValueChanged.AddListener(value => ApplyFullscreen(value));
		fullscreenToggle.onValueChanged.AddListener(value => PlayerPrefs.SetInt("Fullscreen", value ? 1 : 0));

		// --------------------
		// Configuración de Resolución
		// --------------------
		resolutions = Screen.resolutions;
		List<string> options = new List<string>();
		Ent32 currentResolutionIndex = 0;

		for (Ent32 i = 0; i < resolutions.Length; i++)
		{
			string option = resolutions[i].width + " x " + resolutions[i].height;
			options.Add(option);

			if (resolutions[i].width == Screen.currentResolution.width &&
				resolutions[i].height == Screen.currentResolution.height)
			{
				currentResolutionIndex = i;
			}
		}

		resolutionDropdown.ClearOptions();
		resolutionDropdown.AddOptions(options);

		currentResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
		resolutionDropdown.value = currentResolutionIndex;
		resolutionDropdown.RefreshShownValue();

		resolutionDropdown.onValueChanged.AddListener(SetResolution);

		// --------------------
		// Configuración de Idioma
		// --------------------
		InitializeLanguageDropdown();

		UpdateText();
		gameObject.SetActive(false); // Iniciar con el menú de configuración oculto pero configuramos todo para que funcione correctamente al abrirlo por primera vez

	}

	private void UpdateText()
	{
		MusicVolumeTextTag.text = LocalizationSettings.StringDatabase.GetLocalizedString(
table,
MusicVolumeTextTag.gameObject.name
); //les di el nombre del codigo de localizacion para hacer corssreference mejor
		SFXSliderVolumeTextTag.text = LocalizationSettings.StringDatabase.GetLocalizedString(
	table,
	SFXSliderVolumeTextTag.gameObject.name
); //les di el nombre del codigo de localizacion para hacer corssreference mejor
		MasterVolumeTextTag.text = LocalizationSettings.StringDatabase.GetLocalizedString(table, MasterVolumeTextTag.gameObject.name);
		resolutionTextTag.text = LocalizationSettings.StringDatabase.GetLocalizedString(table, resolutionTextTag.gameObject.name);
		LanguageTextTag.text = LocalizationSettings.StringDatabase.GetLocalizedString(table, LanguageTextTag.gameObject.name);
		FullscreenTextTag.text = LocalizationSettings.StringDatabase.GetLocalizedString(table, FullscreenTextTag.gameObject.name);
		OnLanguageChange?.Invoke();
	}

	private void InitializeLanguageDropdown()
	{
		// Obtener todos los idiomas disponibles
		var availableLocales = LocalizationSettings.AvailableLocales.Locales;
		List<string> languageOptions = new List<string>();

		int currentLanguageIndex = 0;
		string savedLanguageCode = PlayerPrefs.GetString("LanguageCode", CurrentLocaleCode);

		for (int i = 0; i < availableLocales.Count; i++)
		{

			Locale locale = availableLocales[i];
			string languageName = locale.LocaleName.Replace($" ({locale.Identifier.Code})", "");// náhuatl no tiene culture al ser custom asi que para que todo idioma sirva tengo que hacer esto
			languageOptions.Add(languageName);

			// Encontrar el índice del idioma guardado
			if (locale.Identifier.Code == savedLanguageCode)
			{
				currentLanguageIndex = i;
			}
		}

		LangDrop.ClearOptions();
		LangDrop.AddOptions(languageOptions);
		LangDrop.value = currentLanguageIndex;
		LangDrop.RefreshShownValue();

		// Suscribirse al cambio de idioma
		LangDrop.onValueChanged.AddListener(ChangeLanguage);

		// Aplicar el idioma guardado al iniciar
		ChangeLanguage(currentLanguageIndex);
	}

	public void ChangeLanguage(int index)
	{
		var availableLocales = LocalizationSettings.AvailableLocales.Locales;

		if (index >= 0 && index < availableLocales.Count)
		{
			var selectedLocale = availableLocales[index];
			LocalizationSettings.SelectedLocale = selectedLocale;

			// Guardar el código del idioma
			string languageCode = selectedLocale.Identifier.Code;
			PlayerPrefs.SetString("LanguageCode", languageCode);
			PlayerPrefs.Save();

			CurrentLocaleCode = languageCode;
			UpdateText();
			//Debug.Log($"Idioma cambiado a: {selectedLocale.Identifier.CultureInfo.DisplayName}"); no espameamos la consola por que el jugador siempre puede CTRL + SHIFT + C y ver los logs infinitos
		}
	}

	private void ApplyMasterVolume(float arg0)
	{
		ApplyVolume(arg0, AudioGroups.Master);
	}

	private void ApplySFXVolume(float arg0)
	{
		ApplyVolume(arg0, AudioGroups.SFX);
	}

	private void ApplyMusicVolume(float arg0)
	{
		ApplyVolume(arg0, AudioGroups.Music);
	}

	private void ApplyVolume(float value, AudioGroups group)
	{
		switch (group)
		{
			case AudioGroups.Music:
				{
					var g = musicSlider.value;
					float db = 20 * Mathf.Log10((value>=0)? value:g / 100);
					AudioMixer.SetFloat("MUS_VOL", db);
					break;
				}

			case AudioGroups.Master:
				{
					var g = MasterSlider.value;
					float db = 20 * Mathf.Log10( (value >= 0)? value:g / 100);
					AudioMixer.SetFloat("MAS_VOL", db);
					break;
				}

			case AudioGroups.SFX:
				{
					var g = SFXSlider.value;
					float db = 20 * Mathf.Log10( (value >= 0)?value:g / 100);
					AudioMixer.SetFloat("SFX_VOL", db);
					break;
				}

			case AudioGroups.None:
				break;
			default:
				break;
		}
	}

	enum AudioGroups
	{
		None = -1,
		Master,
		Music,
		SFX
	}

	private void ApplyFullscreen(bool isFullscreen)
	{
		Screen.fullScreen = isFullscreen;
		//Debug.Log($"Fullscreen set to {isFullscreen}");
	}

	public void SetResolution(int index)
	{
		Resolution res = resolutions[index];
		Screen.SetResolution(res.width, res.height, Screen.fullScreen);
		
		PlayerPrefs.SetInt("ResolutionIndex", index);
		PlayerPrefs.Save();

		//Debug.Log($"Resolución cambiada a {res.width}x{res.height}");
	}
	int CAKER = 0;
	public void CAKE()
	{
		CAKER++;
		//los numeros son temporales pueden aumentar
		if (CAKER >= 5 && CAKER < 10) 
		{
			Console.Instance.ForceOn();
			string MSG = LocalizationSettings.StringDatabase.GetLocalizedString(
	table,
	"5CAKE"
);
			Debug.Log(MSG);//esp: PARA

		}
		if (CAKER >= 10 && CAKER < 20)
		{
			string MSG = LocalizationSettings.StringDatabase.GetLocalizedString(
	table,
	"10CAKE" //que dejes de pulsar 
);
			Debug.Log(MSG);

		}
		if (CAKER >= 20 && CAKER < 30)
		{

			//the cake is a lie (no localizado)

			Debug.Log("the cake is a lie");

		}
		if (CAKER >= 30 && CAKER <= 33)
		{
			string MSG = LocalizationSettings.StringDatabase.GetLocalizedString(
	table,
	"CAKERECYPE"
);			Console.Instance.Execute("CLS");
			Debug.Log(MSG);// eso es una receta que dice 
			/*
			 oh y la receta ahora como se ve // [INTERNAL_RECIPE]
			// "The cake IS STILL NOT A LIE"
			// "The settings cake is still red" (Strawberry Flavor Edition v2.0 - Stable Build)


 			 * Patch Notes:
 			 * - Fixed yeast confusion bug (now using Baking Powder correctly)
 			 * - Added butter for improved texture (no more dry DLC)
 			 * - Rebalanced strawberry jam physics
 			 * - Improved moistness rendering pipeline
 

			*300g Flour(High-polygon count, now properly hydrated)
			*150g Sugar(For the sweet taste of victory)
			*3 Eggs(Still not for hatching.Please stop trying.)
			*100g Butter OR 80ml Oil(Finally, some smooth performance)
			*100–150g Strawberry Jam(THE RED INGREDIENT - now balanced, not overpowered)
			* 100ml Milk(Powered by Calcium Engine v1.0)
			*15g Baking Powder(NOT yeast.This is not a fermentation simulator. but if you try to use it You measure not us)

			// Logic:
			// We added fat to prevent the "desert biome cake" bug.
			// Jam quantity reduced to avoid "raw center any% speedrun".
			// Stability improved. No early access nonsense.

			// Instructions:
			// 1. Cream butter + sugar until fluffy (increase volume settings).
			// 2. Add eggs one by one (do not rage-quit if it looks weird).
			// 3. Mix in milk and strawberry jam (activate red shader).
			// 4. Add flour + baking powder (sifted, no texture glitches).
			// 5. Mix until batter reaches stable framerate (no overclocking).
			//
			// Optional DLC:
			// - Swirl extra jam on top for aesthetic rendering.
			// - Or inject jam post-bake for a hidden loot layer.
			//
			// Bake at 180°C for ~30–40 minutes until golden-brown.
			// Toothpick test: if it comes out clean, you win.
			//
			// WARNING:
			// Do NOT use your GPU as an oven.
			// Thermal throttling is not a cooking technique.
			// We tried. The cake tasted like driver updates and sadness.
			*/
			//la cual si esta localizada a distintos idiomas

		}
	}
}

//PD:
//MALDITO UNITY POR NO SOPORTAR LENGUAS INDIGENAS AMERICANS COMO
//MAYA
//NÁHUATL
//quechua
//aimara
//guaraní.
//etc
//>:(



//pd PASTEL DE FRESA ES VIDA, EL MEJOR DLC QUE PODRÍAMOS HABER PEDIDO, GRACIAS MAXIS, GRACIAS EA, GRACIAS SIMS, GRACIAS POR HACER ESTE SUEÑO REALIDAD, GRACIAS POR HACER UN JUEGO CON OPCIONES DE ACCESIBILIDAD Y PERSONALIZACIÓN TAN INCREÍBLES, GRACIAS POR HACER UN JUEGO QUE INCLUYE IDIOMAS INDÍGENAS AMERICANOS COMO EL NÁHUATL, GRACIAS POR HACER UN JUEGO QUE CELEBRA LA DIVERSIDAD CULTURAL Y LINGÜÍSTICA DE NUESTRA HERMOSA AMÉRICA, GRACIAS POR HACER UN JUEGO QUE NOS PERMITE EXPRESAR NUESTRA IDENTIDAD Y NUESTRO ORGULLO A TRAVÉS DE LOS AJUSTES DE CONFIGURACIÓN, GRACIAS POR HACER UN JUEGO QUE NOS HACE SENTIR EN CASA, GRACIAS POR HACER UN JUEGO QUE NOS HACE SENTIR VIVOS, GRACIAS POR HACER UN JUEGO QUE NOS HACE SENTIR FELICES, GRACIAS POR HACER UN JUEGO QUE NOS HACE SENTIR AMADOS, GRACIAS POR HACER UN JUEGO QUE NOS HACE SENTIR LIBRES, GRACIAS POR HACER UN JUEGO QUE NOS HACE SENTIR INFINITOS
// EL PASTEL ES EL mejor

public class MensajesDePastelQUeNoSOnDelEasterEggSInoparaQueEldesarolladorSeRia
{
	public int CAKER = 0;
	public string SSSS = "pastel de fresa pastel de chocolatr pastel de vianilaaaaaaaaaaaaaaaaaa fegbyhjkusw [bsod por sobrecarga de pastel]";
}






// TItulo de ventana fin del prototipo:
// Fin del Prototipo -es/es-mx
// End of Prototype -en
// Fin du Prototype -fr
// Prototypen Ende -de
// nh: "Tlami nopa prototipo" 
// espero que eso signifique "fin del prototipo" en náhuatl, no tengo ni la menor idea de como se dice eso en náhuatl pero suena bien, es un idioma muy bonito, me gusta mucho, es una lástima que no tenga soporte oficial en Unity, pero al menos lo incluimos como idioma personalizado, así que aunque no sea perfecto, es un gran paso para la representación de las lenguas indígenas americanas en los videojuegos, y eso es algo que debemos celebrar y agradecer.
// Maya: "Xul le prototipo" (Prototipo en maya)
// pr-br: "Fim do Protótipo" (Prototipo em português brasileiro)
// pt-pt: "Fim do Protótipo" (Prototipo em português de Portugal)//es lo mismo OH
//mensaje de fin del prototipo:
// es: "Fin del Juego!, LLegaste al fin de lo jugable de la etapa Microbio de Adaptica, en el futuro llegaran mas etapas como Criatura, Tribal, etc, cada una con su propia jugabilidad, ¡¡Gracias por jugar y apoyar el desarrollo de Adaptica!!"
// en: "End of the Game!, You have reached the end of the playable content of the Microbe stage of Adaptica, in the future more stages will come like Creature, Tribal, etc, each with its own gameplay, Thank you for playing and supporting the development of Adaptica!!"
// fr: "Fin du Jeu!, Vous avez atteint la fin du contenu jouable de l'étape Microbe d'Adaptica, à l'avenir d'autres étapes viendront comme Creature, Tribal, etc, chacune avec son propre gameplay, Merci de jouer et de soutenir le développement d'Adaptica!!"
// de: "Ende des Spiels!, Sie haben das Ende des spielbaren Inhalts der Mikrobenphase von Adaptica erreicht, in Zukunft werden weitere Phasen wie Kreatur, Tribal, etc. kommen, jede mit ihrem eigenen Gameplay, Vielen Dank fürs Spielen und Unterstützen der Entwicklung von Adaptica!!"
// nh: "Tlami nopa tlauisokilistli! Ya tiajsito kampa tlami nopa tlauisokilistli tlen nopa etapa Microbio tlen Adaptica. Ipan nopa tonali tlen ualas onkas sekinok etapas kej Creature, Tribal, etc., sejse ika i tlauisokilistli. ¡Tlaskamati pampa tlauisoki uan tijpaleuijtok ma moskalti Adaptica!!"
// yua: "Ts'o'okok le báaxala'! Ts'o'ok k'uchul u xuul le ba'ax ku jugable ti' le p'isibij microbio Adaptica, Ti' le futuro yaan u k'uchul asab jejelas súutukil bey Creature, Tribal, etcetera, Amal juntúulal yéetel u kajnáalo'ob báaxal. Yuumbo'otik báaxal yéetel yáanta'al le ma'alo'ob Adaptica!"
// pt: "Fim do Jogo!, Você chegou ao fim do conteúdo jogável da fase Microbe de Adaptica, no futuro mais fases virão como Creature, Tribal, etc, cada uma com sua própria jogabilidade, Obrigado por jogar e apoiar o desenvolvimento de Adaptica!!"
//espero que los traductores en linea no me hayan fallado.





//definitvamente lo voy a reciclar para las pantallas de fin de etapa de las demas cuando lleges al final del prototipo de esa etapa
//excepto cuando este el juego completo y ya no haya mas etapas.

//creditos:

//Programador principal: Jesús Eduardo -yo
//compositor de musica: Jesús Eduardo -yo
//diseñador de UI: Jesús Eduardo -yo
//apoyo con las dudas: diversos LLMs y foros de desarrollo
// tutoriales usados:
// How to do Marching Cubes (Unity 2020) (omar snatiago) https://youtu.be/JdeyNbDACV0?si=hPZqKH690j02oBUQ
// Official unity input system tutorials : https://www.youtube.com/playlist?list=PLX2vGYjWbI0RpLvO3B7aH-ObfcOifMD20
// Coding Adventure: Ray Marching (Sebastian Lague) https://www.youtube.com/watch?v=Cp5WWtMoeKg
// progrmas usados:
// Unity 2023.2.20/22f1 (empeze en .20 y migre a .22 por Exploit GRAVE)
// blender 4.5-5.1  (para modelado de partes)
// Musescore 4 (para composicion de musica)
// Paint (windows 11) para fondos y texturas.
// Spore, Inspiracion para todo el juego
// 
// MUCHAS gracias a Maxis/EA por hacer spore lo que me 
// inspiro a hacer este juego
// A D I O S

// no, no lo traducire por flojera :)