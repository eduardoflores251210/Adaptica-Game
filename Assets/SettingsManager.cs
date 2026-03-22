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