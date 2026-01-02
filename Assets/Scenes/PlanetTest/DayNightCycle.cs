using SerializableTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
	public Transform Planet;
    public float AxialTilt;
	// esta clase Hour Time tiene Hour Minute y Second
	[Serialize]
    public HourTime LenghtInRealTime;
    [Serialize]
    public HourTime CurrentTime;

    // Start is called before the first frame update
    void Start()
	{
		if (Planet == null)
		{
			Planet = transform;
		}
	}

    void FixedUpdate()
    {
        // avance por FixedDeltaTime
        float delta = Time.fixedDeltaTime; // segundos reales que pasaron
        float dayInSeconds = LenghtInRealTime.TotalSeconds(); // duración del día en segundos
        float fractionOfDay = delta / dayInSeconds; // qué fracción del día ha pasado

        // convertir a segundos de CurrentTime
        float secondsToAdd = fractionOfDay * 86400f; // 86400s = 24h en segundos
        CurrentTime.AddSeconds((int)(secondsToAdd)); // asumiendo que HourTime tiene AddSeconds
                                                     // calcular fracción del día
        float dayFraction = CurrentTime.TotalSeconds() / 86400f; // 0 a 1

        // rotar 360° sobre Y
        Planet.localRotation = Quaternion.Euler(0f+ AxialTilt, dayFraction * 360f, 0f);
    }

}
