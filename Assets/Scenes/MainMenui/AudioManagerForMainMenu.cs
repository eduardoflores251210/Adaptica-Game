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
    public StartVideoPlayer StartVideo; //preparandome para cinematica de carga :D
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
                            if (StartVideo.gameObject.activeInHierarchy ==  false)
                            {
                                StartVideo.gameObject.SetActive( true);
                            }else
                            {
                                if (StartVideo.Done)
                                {
                                    StartVideo.gameObject.SetActive( false);
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
