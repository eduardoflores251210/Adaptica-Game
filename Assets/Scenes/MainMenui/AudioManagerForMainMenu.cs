using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
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
    public bool FaeDeed { get; private set; }
    void Start()
    {
        if (CheckStuff())
            Debug.Log("⚠️⚠️ERROR CATASTROFICOOOOOO, ALGO IMPRESCINDIBLE NO ESTA ASIGNADO⚠️⚠️");
		fade.OnFadeInEnd += DisabeObj;
    }

	void DisabeObj() { fade.gameObject.SetActive(false); Debug.Log("DISBING"); }
	bool A = false;
    GameObject P = null;
    // Update is called once per frame
    void Update()
    {
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
							TEXT.text = "Cargando Galaxia por favor espere....";
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
		if (Camera.main != null)
		{
			Camera.main.cullingMask = DefMask;
		}
		if (LoadScreenEXIST && LoadScreen != null)
		{
			Destroy(LoadScreen);
		}
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
