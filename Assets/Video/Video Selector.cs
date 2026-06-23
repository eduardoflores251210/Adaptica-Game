using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoSelector : MonoBehaviour
{
	public VideoClip V4_3;
	public VideoClip V16_9;
	public VideoPlayer videoPlayer;
	[Header("Opcional")]
	[Tooltip("La textura usada por el renderizador de video a resetear ")]
	public RenderTexture VideoTex;

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


	}

	// Update is called once per frame
	void Update()
	{
		
	}
}
