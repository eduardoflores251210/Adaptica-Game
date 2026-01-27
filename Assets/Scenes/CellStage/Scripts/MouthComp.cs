using SerializableTypes;
using SerializableTypes.Biology;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MouthComp : MonoBehaviour
{
    public Diets ComidasQuePuedeComer;
    public Rigidbody Rigidbody;
    public CellController cellController;
    // Start is called before the first frame update
    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        if (Rigidbody == null)
        {
            Rigidbody=gameObject.AddComponent<Rigidbody>();
        }
        Rigidbody.isKinematic = true;
        Rigidbody.useGravity = false;
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
	private void OnCollisionEnter(Collision collision)
	{
        Debug.Log("2A");
        if (collision.gameObject.TryGetComponent<FoodComp>(out var Food))
        {
            if (IsFoodCompatibleWithDiet(Food.tipo, cellController.CreatureDiet))
            {
                if (Food.IsSpoiled)
                {
                    cellController.Health += 0.25f;
                    cellController.Health = Mathf.Clamp(cellController.Health, 0, cellController.MaxHealth);
                }
                else
                {
                    cellController.Health += 1f;
                    cellController.Health = Mathf.Clamp(cellController.Health, 0, cellController.MaxHealth);
                }


				cellController.CurrentEvoPoints++; //Definittivamente no son los puntos de ADN de spore pero con otro nombre definitivamente (Sarcasmo)
                cellController.MaxEvoPointsGotStat++;
                cellController.StageProgress += Mathf.Abs( Random.insideUnitCircle.x);
                
                Destroy(collision.gameObject);
            }
        }
	}
    public static bool IsFoodCompatibleWithDiet(TipoDeComida a, Diets b)
    {
        if (b == Diets.none) return false;
        if (a == TipoDeComida.Ninguno) return false;
        TipoDeComida[] herbivoro = new TipoDeComida[7]
        {
            TipoDeComida.Nectar,
            TipoDeComida.Hongo,
            TipoDeComida.Flor,
            TipoDeComida.Fruto,
            TipoDeComida.Hoja,
            TipoDeComida.Alga,
            TipoDeComida.Planta,
        };
        TipoDeComida[] Carnivoro = new TipoDeComida[2]
		{
            TipoDeComida.Carne,
            TipoDeComida.Huevo
        };
        if (b == Diets.Herbivore)
        {
            return herbivoro.Contains(a);
        }
        if (b == Diets.Carnivore)
        {
            return Carnivoro.Contains(a);
        }
        if (b == Diets.Omnivore)
        {
            List<TipoDeComida> Omni = new();
            foreach (var aa in herbivoro)
            {
                Omni.Add(aa);
            }
            foreach (var aa in Carnivoro)
            {
                Omni.Add(aa);
                Omni.Add(aa);
            }
            return Omni.Contains(a);
        }
        return false;
    }
}
