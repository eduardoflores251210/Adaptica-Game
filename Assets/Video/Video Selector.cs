using FixedMath;
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

	// Start is called before the first frame update
	void Start()
	{
		if (videoPlayer == null)
			return;
		if (V4_3 == null)
			return;
		if (V16_9  == null)
			return;
		Fixed128 aspec = (Fixed128)Screen.width / (Fixed128)Screen.height;
		VideoClip Seected = null;
		if (aspec == (Fixed128)4 / (Fixed128)3)
		{
			Seected = V4_3;
		}    
		if (aspec >= (Fixed128)16 /(Fixed128)9)
		{
			Seected= V16_9;
		}
		if (Seected == null)
			Seected = V4_3;
		videoPlayer.clip = Seected;
		
	}

	// Update is called once per frame
	void Update()
	{
		
	}
}
