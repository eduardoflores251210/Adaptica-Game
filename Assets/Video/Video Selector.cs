using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoSelector : MonoBehaviour
{
	public AudioSource Aui;
	public VideoClip V4_3;
	public VideoClip V16_9;
	public VideoPlayer videoPlayer;
	[Header("Opcional")]
	[Tooltip("La textura usada por el renderizador de video a resetear ")]
	public RenderTexture VideoTex;
	bool startTimer = false;
	// Start is called before the first frame update
	void Start()
	{





		if (VideoTex != null)
		{

			RenderTexture active = RenderTexture.active;
			RenderTexture.active = VideoTex;

			GL.Clear(true, true, Color.black);

			RenderTexture.active = active;


		}
		double aspect = (double)Screen.width / (double)Screen.height;

		VideoClip selected;

		double aspect16_9 = (double)16 / (double)9;

		if (aspect >= aspect16_9)
		{
			// Pantallas anchas, ultra anchas y mutantes
			selected = V16_9;
		}
		else
		{
			// Pantallas más cuadradas o clásicas
			selected = V4_3;
		}


		videoPlayer.clip = selected;
		if (Directory.Exists(Paths.CustomIntros))
		{
			List<string> ValidVids = new List<string>();
			foreach (string file in Directory.GetFiles(Paths.CustomIntros))
			{
				string[] Extentions = { ".mp4", ".mov", ".avi", ".webm" }; //unbity no soporta mkv :(
				if (Extentions.Contains(file.ToLower().Trim().Substring(file.ToLower().Trim().LastIndexOf('.'))))
				{
					Debug.Log("Se encontró un video personalizado: " + file);
										ValidVids.Add(file);

				}
			}
			if (ValidVids.Count > 0)
			{
				string file = ValidVids[Random.Range(0, ValidVids.Count)];

				// 1. MUST set source type FIRST so Unity knows it's a URL setup
				videoPlayer.source = VideoSource.Url;
				videoPlayer.clip = null;

				// 2. Setup the Audio Component
				if (Aui == null)
				{
					Aui = gameObject.AddComponent<AudioSource>();
				}

				// Ensure spatial blend is 2D if it's a fullscreen intro, 
				// otherwise 3D audio distance attenuation might mute it.
				Aui.spatialBlend = 0f;

				// 3. Configure the Video Player Audio Modes BEFORE setting the actual URL
				videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
				videoPlayer.controlledAudioTrackCount = 1;
				videoPlayer.EnableAudioTrack(0, true);
				videoPlayer.SetTargetAudioSource(0, Aui);

				// 4. SET THE URL LAST. This forces Unity to bind the track straight to 'Aui'
				videoPlayer.url = file;

				videoPlayer.Play();
				startTimer = true;
				return;
			}
		}
		startTimer = true;

	}
	float Timer = 0f;
	bool waitingForInput = false;
	// Update is called once per frame
	void Update()
	{
		if (f)
		{ 
			var g= FindAnyObjectByType<StartVideoPlayer>();
			if (g != null)
			{
				g.Done = true;
			}
			gameObject.SetActive(false); 
		}

		if (startTimer)
		{
			Timer += Time.deltaTime;
		}

		if (Timer > 60)
		{
			if (waitingForInput)
				return;
			waitingForInput = true;
			ConsoleManager.Instance.consoleUI.SetActive(true);
			Debug.Log("quieres Saltar la INTRO S/N");
			a();
		}
	}
	bool f = false;
	async void a()
	{
		var fa =	await ConsoleManager.Instance.ReadLine();
		if ( fa.Trim().ToUpper() == "S" )
		{
			f = true; //creo que es mas seguro cambiar un bool que usar gameObject.SetActive(false) Xd el multithrafing ES RARISIMO 
		}else Timer = 0f; //reiniciamos el timer para que vuelva a preguntar en 60 segundos

	}
}
