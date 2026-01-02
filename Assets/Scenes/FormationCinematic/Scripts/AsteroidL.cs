using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidL : MonoBehaviour
{
    public GameObject Target;
    public float gravityStrength = 9.8f;  // ¿Qué tan fuerte es la gravedad del objetivo?
    public float maxSpeed = 90f;          // Velocidad máxima para no salir disparado al infinito
    
    public GameObject ExplosionPrefab;

    [SerializeField]
    public static List<GameObject> Asteroids = new();


    private Rigidbody rb;
    private void OnEnable()
    {
        Asteroids.Add(this.gameObject);
    }
    private void OnDisable()
    {
        if (Asteroids.Contains(this.gameObject)) 
        Asteroids.Remove(this.gameObject);
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("Este asteroide no tiene Rigidbody. ¡No podrá sentir la gravedad!");
        }
    }

    void FixedUpdate()
    {
        if (Target == null || rb == null)
            return;

        // Calculamos la dirección hacia el target
        Vector3 directionToTarget = (Target.transform.position - transform.position).normalized;

        // Aplicamos una fuerza gravitatoria hacia el target
        Vector3 gravityForce = directionToTarget * gravityStrength;

        rb.AddForce(gravityForce);

        // Limitamos la velocidad para que no se vuelva un cohete descontrolado
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        foreach (GameObject obj in Asteroids)
        {
            if (obj != gameObject)
            {
                Vector3 direction = (obj.transform.position - transform.position).normalized;
                rb.AddForce(direction * (gravityStrength*0.1f));
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<AsteroidL>(out _))
        {
            return;
        }
        else
        {
            var go = Instantiate(ExplosionPrefab);
            go.transform.position = transform.position;
            go.transform.localScale = transform.localScale;
            Destroy(gameObject);
        }
    }
}
