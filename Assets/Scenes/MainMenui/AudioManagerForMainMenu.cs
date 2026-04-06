using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Video;
/// <summary>
/// Si tecnicamente Maneja el audio y fundido pero 
/// eh aqui es normal que el mismo script haga varios trabajos
/// </summary>
public class AudioManagerForMainMenu : MonoBehaviour
{
	[Header("OBLIGATORIO")]
	public AudioSource MusicPlayer;
	public GalaxyGenerator GalaxyGenerator;
	public FadeToBlck fade;
	[Header("Opcional")]
	public StartVideoPlayer StartVideo; //preparandome para cinematica de carga :D
	public LayerMask DefMask;
	public bool LoadScreenEXIST = false;
	public GameObject LoadScreen;
	/// <summary>
	/// aka Fadeed aka
	/// Faded controla SI has hecho fundido o no 
	/// </summary>
	public bool FaeDeed { get; private set; } //y si fae deed significa algo asi como accion del Hada. jejejejeje
	void Start()
	{
		if (CheckStuff())
			Debug.Log("⚠️⚠️ERROR CATASTROFICOOOOOO, ALGO IMPRESCINDIBLE NO ESTA ASIGNADO⚠️⚠️"); //el script es dramatico, asi que el mensaje de error es dramatico tambien
		fade.OnFadeInEnd += DisabeObj;

		LoadLoadScreen	(); //si cargamos LA PANTALLA de carga. ¿redundante? si, pero asi me aseguro de que la pantalla de carga se cargue antes de que se necesite, ademas de que asi puedo mostrar un spinner o algo asi mientras se carga la galaxia o lo que sea que se este cargando

	}
	ConfigLoadScreen configLoadScreen;
	Sprite Spinner	= null;
	public async void LoadLoadScreen()
	{
		var handle = Addressables.LoadAssetAsync<ConfigLoadScreen>("Assets/GLSS"); // "GLSS" es el Address que yo le puse
		await handle.Task;
		configLoadScreen = handle.Result;
		Spinner = configLoadScreen.Spinner;

	}
	public void RealeseHandeeere()
	{
		if (configLoadScreen != null)
		{
			Addressables.Release(configLoadScreen);
		}
	}
		void DisabeObj() { fade.gameObject.SetActive(false); Debug.Log("DISBING"); }
	bool A = false;
	GameObject P = null;
	GameObject E = null;
	// Update is called once per frame
	void Update()
	{
		//ADVETENCIA MONTAÑA DE IF-ELSE ABAJO
		if (!FaeDeed)
		{
			if (CheckStuff())
				return;

			if (GalaxyGenerator.DoesTheGalaxyExist && !GalaxyGenerator.IsGenerating)
			{
				if (obj != null)
				{
					Destroy(obj);
				}
				if (GalaxyGenerator.visualizer != null)
				{
					if (GalaxyGenerator.visualizer.Done)
					{
						if (StartVideo != null)
						{
							if (StartVideo.gameObject.activeInHierarchy ==  false)
							{
								StartVideo.gameObject.SetActive( true);
							}else
							{
								if (StartVideo.Done)
								{
									Init();
									StartVideo.gameObject.SetActive(false);
								}
							}
						}
						else
						{
							Init();
						}
					}else
					{
						if (A)
						{

						}
						else
						{
							Transform C = fade.transform;
							GameObject TextGO = new("GEN_TXT");
							var TEXT = TextGO.AddComponent<TMPro.TextMeshProUGUI>();
							TEXT.text = "Cargando Galaxia\n por favor espere....";
							TEXT.fontSize = 120;
							TEXT.color = Color.white;
							TEXT.alignment = TMPro.TextAlignmentOptions.Left;

							TEXT.rectTransform.SetParent(C, false);
							TEXT.rectTransform.anchorMin = Vector2.zero;
							TEXT.rectTransform.anchorMax = Vector2.one;
							TEXT.rectTransform.offsetMin = Vector2.zero;
							TEXT.rectTransform.offsetMax = Vector2.zero;
							GameObject eee = new("SPINNER"); //esta porqué la gente piensa que crasheo si no ven algo moviendose en la pantalla, aunque la verdad es que el texto ya se mueve por si solo, pero bueno, asi se sienten mas seguros de que el juego no se crasheo
							eee.transform.SetParent(C, false);
							var IMG = eee.AddComponent<UnityEngine.UI.Image>();
							IMG.sprite = Spinner;
							var SPIN = eee.AddComponent<SPINNER>();// NO esto no es un fidget Spinner de 2017, es un script que hice para rotar cosas, como por ejemplo este sprite de carga
							SPIN.speed = 200f;
							SPIN.eje = StandartUtilities.Eje.Z; // el unico eje posible para 2D	.
							SPIN.clockwise = !true;//!true por que la claridad a proposito es mala. ejejejeje
							E = eee;
							A = true; //false
							P = TextGO;
							//el comewntario de arriba es confuso a proposito, porque el texto ya se mueve por si solo, pero bueno, asi se sienten mas seguros de que el juego no se crasheo
						}
					}
				}
				else
				{
					Init();
				}
			}
			else
			{
				if (A)
				{

				}else
				{
					Transform C = fade.transform;
					GameObject TextGO = new("GEN_TXT");
					obj = TextGO;
					var TEXT = TextGO.AddComponent<TMPro.TextMeshProUGUI>();
					TEXT.text = "Generando Galaxia por favor espere....";
					TEXT.fontSize = 120;
					TEXT.color = Color.white;
					TEXT.alignment = TMPro.TextAlignmentOptions.Center;

					TEXT.rectTransform.SetParent(C, false);
					TEXT.rectTransform.anchorMin = Vector2.zero;
					TEXT.rectTransform.anchorMax = Vector2.one;
					TEXT.rectTransform.offsetMin = Vector2.zero;
					TEXT.rectTransform.offsetMax = Vector2.zero;
					A = true;
					P = TextGO;
				}
			}
		}
		//FIN DE LA MONTAÑA DE IF-ELSE

		// (en otro universo): SI-SINO mountain

		// en otro otro universo: [Metodo mas facil que no hacer una montaña de if-else pero que no se me ocurrio]

		//OH NO CASDI SON LAS 12 AM son 11:56 creo es que reloj Analogico.
	}
	GameObject obj = null;
	private void Init()
	{
		fade.StartFadeIn();
		FaeDeed = true;
		MusicPlayer.enabled = true;
		if (P != null)
		{
			P.SetActive(false);
		}
		if (E != null)
		{
			E.SetActive(false);
		}
		if (Camera.main != null)
		{
			Camera.main.cullingMask = DefMask;
		}
		if (LoadScreenEXIST && LoadScreen != null)
		{
			Destroy(LoadScreen);
		}
		RealeseHandeeere();
	}

	bool CheckStuff()
	{
		LoadScreen = GameObject.Find("LoadingScreen");
		if (LoadScreen != null)

		{
			LoadScreenEXIST = true;
			StartVideo = null;

		}
		return MusicPlayer == null || GalaxyGenerator == null || fade == null;

	}
	Texture2D CopyTexture(Texture source)
	{
		RenderTexture rt = RenderTexture.GetTemporary(
			source.width,
			source.height,
			0,
			RenderTextureFormat.ARGB32
		);

		Graphics.Blit(source, rt);

		RenderTexture prev = RenderTexture.active;
		RenderTexture.active = rt;

		Texture2D readable = new Texture2D(
			source.width,
			source.height,
			TextureFormat.ARGB32,
			false
		);

		readable.ReadPixels(
			new Rect(0, 0, rt.width, rt.height),
			0,
			0
		);
		readable.Apply();

		RenderTexture.active = prev;
		RenderTexture.ReleaseTemporary(rt);

		return readable;
	}
	Texture2D MultiplicarTextura(Texture2D original, float Ñ)
	{
		// Creamos una nueva textura readable
		Texture2D nueva = new Texture2D(
			original.width,
			original.height,
			original.format,
			false
		);

		Color[] pixeles = original.GetPixels();

		for (int i = 0; i < pixeles.Length; i++)
		{
			Color c = pixeles[i];
			c.r *= Ñ;
			c.g *= Ñ;
			c.b *= Ñ;
			c.a *= Ñ;
			pixeles[i] = c;
		}

		nueva.SetPixels(pixeles);
		nueva.Apply();

		return nueva;
	}
	Texture2D AclararTextura(Texture2D original, float Ñ)
	{
		// Creamos una nueva textura readable
		Texture2D nueva = new Texture2D(
			original.width,
			original.height,
			original.format,
			false
		);

		Color[] pixeles = original.GetPixels();

		for (int i = 0; i < pixeles.Length; i++)
		{
			Color c = pixeles[i];
			c.r *= Ñ;
			c.g *= Ñ;
			c.b *= Ñ;
			// alpha se deja en paz, que no hizo nada malo
			pixeles[i] = c;
		}

		nueva.SetPixels(pixeles);
		nueva.Apply();

		return nueva;
	}

}
