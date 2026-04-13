using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explossdasdsd : MonoBehaviour //es solo para que la explosion se destruya al finalizar
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Wait());
    }

    IEnumerator Wait ()
    {
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }
}
