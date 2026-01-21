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

    bool Fadeed;// F*** correct english spellig      
    //COPILOT POR QUE SUGERISTE ESE COMENTARIO con el que estoy de acuerdo pero NO DE FORMA TAN GROSERA
    // Start is called before the first frame update
    void Start()
    {
        if (CheckStuff())
            Debug.Log("⚠️⚠️ERROR CATASTROFICOOOOOO, ALGO IMPRESCINDIBLE NO ESTA ASIGNADO⚠️⚠️");
    }


    // Update is called once per frame
    void Update()
    {
        if (!Fadeed)
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
									Fadeed = true;
									MusicPlayer.enabled = true;
								}
                            }
                        }
                        else
                        {
                            fade.StartFadeIn();
                            Fadeed = true;
                            MusicPlayer.enabled = true;
                        }
                    }
                }
                else
                {
                    fade.StartFadeIn();
                    Fadeed = true;
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
