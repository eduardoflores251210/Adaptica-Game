using StandartUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPINNER : MonoBehaviour
{
    public float speed = 100f; // Speed of rotation in degrees per second
    public Eje eje = Eje.Y; // Axis of rotation
    public bool clockwise = true; // Direction of rotation
								  // Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float rotationSpeed = clockwise ? speed : -speed;
        switch (eje)
        {
            case Eje.X:
                transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
				break;
            case Eje.Y:
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
				break;
            case Eje.Z:
                transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
                break;
            default:
                break;
        }
    }
}
