using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidLauncher : MonoBehaviour
{
    public GameObject AsteroidPrefab; // El pequeñín espacial que vamos a lanzar
    public bool IsWaiting = true;     // ¿Estamos en pausa o desatando el caos?
    public GameObject Target;         // El blanco al que queremos sorprender

    public float launchInterval = 1f;  // Tiempo entre lanzamientos (segundos)
    public float launchSpeed = 20f;    // Velocidad del asteroide
    public float orbitRadius;
    private float launchTimer = 0f;



    void Update()
    {
        if (IsWaiting)
        {
            // Hacemos una pausa, el lanzador está tomando té de flores mágicas
            return;
        }

        if (AsteroidPrefab == null)
        {
            Debug.LogWarning("¡No hay asteroide! ¿Dónde está la roca mágica?");
            return;
        }

        if (Target == null)
        {
            Debug.LogWarning("¿Quién es el objetivo? ¡Los asteroides necesitan una dirección!");
            return;
        }

        // Controlamos el tiempo entre lanzamientos para no bombardear a lo loco
        launchTimer += Time.deltaTime;
        if (launchTimer >= launchInterval)
        {
            LaunchAsteroidAtRandomDirection();
            launchTimer = 0f;
        }
        OrbitAroundTarget(transform, Target.transform, ref curretagle, orbitRadius, 100f);
    }
    float curretagle=0;

    void OrbitAroundTarget(Transform orbiter, Transform target, ref float currentAngle, float baseOrbitRadius, float orbitSpeed)
    {
        if (target == null) return;

        currentAngle += orbitSpeed * Time.deltaTime;
        float angleRad = currentAngle * Mathf.Deg2Rad;

        // Variación suave en la altura usando PerlinNoise (vale para dar un movimiento orgánico)
        float yVariation = Mathf.PerlinNoise(Time.time * 0.5f, 0f) * 2f - 1f; // rango aprox [-1,1]
        yVariation *= 1.5f; // escala la variación en Y

        // Variación suave en el radio de la órbita
        float radiusVariation = Mathf.PerlinNoise(0f, Time.time * 0.7f) * 2f - 1f; // rango aprox [-1,1]
        float currentRadius = baseOrbitRadius + radiusVariation;

        float x = Mathf.Cos(angleRad) * currentRadius;
        float z = Mathf.Sin(angleRad) * currentRadius;

        orbiter.position = new Vector3(x, yVariation, z) + target.position;
        orbiter.LookAt(target);
    }



    void LaunchAsteroidAtRandomDirection()
    {
        // Creamos el asteroide en la posición del lanzador
        GameObject asteroid = Instantiate(AsteroidPrefab, transform.position, Quaternion.identity);

        if (asteroid.TryGetComponent<Rigidbody>(out var rb))
        {
            // Generar una dirección aleatoria en 3D (vector unitario)
            Vector3 randomDirection = Random.onUnitSphere;

            // Lanzar el asteroide con la velocidad definida
            rb.velocity = randomDirection * launchSpeed;
        }
        else
        {
            Debug.LogWarning("Este asteroide no tiene Rigidbody, ¡no podrá salir disparado!");
        }

        if (asteroid.TryGetComponent<AsteroidL>(out var asteroidL))
        {
            asteroidL.Target = Target;
        }
        else
        {
            Debug.LogWarning("Este asteroide no tiene AsteroidL, ¡no podrá volar ni con magia!");
        }
    }

}
