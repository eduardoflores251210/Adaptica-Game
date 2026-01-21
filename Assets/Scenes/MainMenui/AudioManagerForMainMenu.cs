using System.Collections;
using System.Collections.Generic;
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
    public VideoPlayer StartVideo; //preparandome para cinematica de carga :D
    /// <summary>
    /// aka Fadeed aka
    /// Faded controla SI has hecho fundido o no 
    /// </summary>
    public bool FaeDeed { get; private set; }
    void Start()
    {
        if (CheckStuff())
            Debug.Log("⚠️⚠️ERROR CATASTROFICOOOOOO, ALGO IMPRESCINDIBLE NO ESTA ASIGNADO⚠️⚠️");
    }


    // Update is called once per frame
    void Update()
    {
        if (!FaeDeed)
        {
            if (CheckStuff())
                return;

            if (GalaxyGenerator.DoesTheGalaxyExist)
            {
                if (GalaxyGenerator.visualizer != null)
                {
                    if (GalaxyGenerator.visualizer.Done)
                    {
                        if (StartVideo != null)
                        {
                            if (StartVideo.enabled ==  false)
                            {
                                StartVideo.enabled = true;
                                StartVideo.Play();
                            }else
                            {
                                if (!StartVideo.isPlaying)
                                {
									fade.StartFadeIn();
									FaeDeed = true;
									MusicPlayer.enabled = true;
								}
                            }
                        }
                        else
                        {
                            fade.StartFadeIn();
                            FaeDeed = true;
                            MusicPlayer.enabled = true;
                        }
                    }
                }
                else
                {
                    fade.StartFadeIn();
                    FaeDeed = true;
					MusicPlayer.enabled = true;
				}
			}
        }
    }
    bool CheckStuff()
    {
		return MusicPlayer == null || GalaxyGenerator == null || fade == null;

	}
}
