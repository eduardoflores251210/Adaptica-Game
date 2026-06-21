using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileConsoleManager : MonoBehaviour
{
    public GameObject ConsoleRef;
    // Start is called before the first frame update
    void Start()
    {
        if (Application.platform == RuntimePlatform.Switch) //aunque dudo portear a switch pues mi objetivo es PC/movil
		{
            Debug.Log("Oh F** oh no esto no sirve en switch");
                ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; // puntos y coma innecesarios pero es que me da risa ponerlos
			Destroy (gameObject);
        }

        if (Application.isMobilePlatform || Application.isEditor)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (ConsoleRef == null)
        {
			Destroy(gameObject);
		}
    }
}
