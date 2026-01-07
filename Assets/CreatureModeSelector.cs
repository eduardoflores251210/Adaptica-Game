using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// seleciona el modo del estadio creatura: Tierra o Panqueque
/// Tierra: este sera el final, con un planeta fisicas de planeta y mas similitud a la realidad/Spore
/// Panqueque: modo terraplanista, con un planeta plano y camara orbitando
/// simplificado para pruebas y desarrollo
/// </summary>
public class CreatureModeSelector : MonoBehaviour
{
    public bool EarthOrPankake = false;
    public List<GameObject> EarthAssets;
    public List<GameObject> PankakeAssets;
    public GameObject SpherePlayer;
    
    // Start is called before the first frame update
    void Start()
    {
        if (EarthOrPankake)
        {
            if (EarthAssets != null)
            {
                if (EarthAssets.Count > 0)
                {
                    foreach (GameObject go in EarthAssets)
                    {
                        go.SetActive(true);
                    }
                    
                }
            }
			if (PankakeAssets != null)
			{
				if (PankakeAssets.Count > 0)
				{
					foreach (GameObject go in PankakeAssets)
					{ go.SetActive(false); }
				}
			}
            Camera.main.GetComponent<CameraOrbitController>().enabled = false;
		}
		else
        {
            if (PankakeAssets != null)
            {
                if (PankakeAssets.Count > 0)
                {
                    foreach(GameObject go in PankakeAssets)
                    {
                        go.SetActive(true) ;
                    }
                }
            }
            if (EarthAssets != null)
            {
                if (EarthAssets.Count > 0)
                {
                    foreach( GameObject go in EarthAssets)
                        { go.SetActive(false) ; }
                }
            }
			Camera.main.GetComponent<CameraOrbitController>().enabled = true; //activamos el Casmera orbit

		}
	}

	// Update is vacio por que dudo usarlo pd yes SPANGLISH
	void Update()
    {
        
    }
}
