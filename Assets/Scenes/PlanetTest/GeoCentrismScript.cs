using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeoCentrismScript : MonoBehaviour
{
    public Transform SUN;
    public Transform Earth;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SUN.LookAt(Earth);
    }
}
