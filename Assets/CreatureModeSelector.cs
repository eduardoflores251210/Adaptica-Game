using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Update is called once per frame
    void Update()
    {
        
    }
}
