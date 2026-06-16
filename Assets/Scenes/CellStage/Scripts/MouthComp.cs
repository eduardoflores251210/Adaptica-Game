using aaa;
using ActualUtils;
using SerializableTypes;
using SerializableTypes.Biology;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MouthComp : MonoBehaviour
{
	public Diets ComidasQuePuedeComer;
	public float DamageAmount;
	public bool Is2D = false;
	public bool Trigger = false;
	public Rigidbody Rigidbody;
	public Rigidbody2D Rigidbody2D;
	public CellController cellController;
	// Start is called before the first frame update
	void Start()
	{
		if (!Is2D)
		{
			if (TryGetComponent<MeshFilter>(out MeshFilter filter))
			{
				if (filter.mesh != null)
				{
					MeshCollider collider;
					if (!TryGetComponent<MeshCollider>(out collider))
					{
						collider = gameObject.AddComponent<MeshCollider>(); // Capturamos la referencia devuelta
					}

					// Asignar la malla de colisión creada (altura 14)
					collider.sharedMesh = filter.mesh.CreateMicrobePartCollider(14f).ScaleMesh(new Vector3(1.7f, 1, 1.7f));
				}
			}
			if (!Trigger)
			{
				Rigidbody = GetComponent<Rigidbody>();
				if (Rigidbody == null)
				{
					Rigidbody = gameObject.AddComponent<Rigidbody>();
				}
				Rigidbody.isKinematic = true; // no queremos que haga tonterias
				Rigidbody.useGravity = false;
			}
		}
		else
		{
			if (!Trigger)
			{
				Rigidbody2D = GetComponent<Rigidbody2D>();
				if (Rigidbody2D == null)
				{
					Rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
				}
				Rigidbody2D.isKinematic = true;
				Rigidbody2D.gravityScale = 0f;
			}
		}

	}
	
	// Update is called once per frame
	void Update()
	{
		
	}
	private void OnCollisionEnter(Collision collision)
	{
		Eat(collision.gameObject);
	}
	private void OnCollisionEnter2D(Collision2D collision)
	{
		Eat(collision.gameObject);
	}
	private void OnTriggerEnter(Collider other)
	{
		Eat(other.gameObject);
	}
	private void OnTriggerStay(Collider other)
	{
		Eat(other.gameObject);
	}
	void Eat(GameObject collision)
	{
		if (collision.TryGetComponent<FoodComp>(out var Food))
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
				cellController.StageProgress += Mathf.Abs(Random.insideUnitCircle.x);
				if (cellController.isAI)
				{ }
				else
				{
					HistoryActions item = new HistoryActions();
					item.Tipo = ActionType.Eat;
					item.Path = GetCompatibleDiet(Food.tipo) switch
					{
						Diets.none => HistoryPaths.Neutral,
						Diets.Omnivore => HistoryPaths.Neutral,
						Diets.Carnivore => HistoryPaths.Agressive,
						Diets.Herbivore => HistoryPaths.Friendly,
						_ => 0
					};
					Saver.CurrentGame.Actions.Add(item);
				}
				Destroy(collision);
			}
		}
		else if (collision.TryGetComponent<CellController>(out var cellController))
		{
			if ( cellController == this.cellController)
			{
				return
					;
			}
			//Debug.Log("ÑAM");
			cellController.Damage(DamageAmount, DamageType.Predator);
		}
	}
	private void OnCollisionStay(Collision collision)
	{
		Eat(collision.gameObject);

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
	public static Diets GetCompatibleDiet(TipoDeComida a)
	{
		
		if (a == TipoDeComida.Ninguno) return Diets.none;
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
		if (herbivoro.Contains(a))
		{
			return Diets.Herbivore;
		}
		if (Carnivoro.Contains(a))
		{
			return Diets.Carnivore;
		}

		return Diets.none;
	}
}


 

public static class MeshUtils
{
	/// <summary>
	/// Crea una malla de colisión centrada en el centro de la malla original,
	/// con altura 'height' (por defecto 14) y anchura/profundidad según bounds de la malla.
	/// Seguro para mallas no-readable (usa bounds como fallback).
	/// </summary>
	public static Mesh CreateMicrobePartCollider(this Mesh originalMesh, float height = 24f)
	{
		if (originalMesh == null)
		{
			Debug.LogError("CreateMicrobePartCollider: originalMesh es null");
			return null;
		}

		Vector3 min, max;

		// 1️⃣ Calcular bounds: si readable usamos vértices, si no usamos bounds
		if (originalMesh.isReadable && originalMesh.vertexCount > 0)
		{
			Vector3[] verts = originalMesh.vertices;
			min = verts[0];
			max = verts[0];
			for (int i = 1; i < verts.Length; i++)
			{
				min = Vector3.Min(min, verts[i]);
				max = Vector3.Max(max, verts[i]);
			}
		}
		else
		{
			var b = originalMesh.bounds;
			min = b.min;
			max = b.max;
		}

		// 2️⃣ Tamaño del cubo/paralelepípedo
		Vector3 size = max - min;
		float width = Mathf.Max(0.0001f, size.x);
		float depth = Mathf.Max(0.0001f, size.z);
		float finalHeight = Mathf.Max(0.0001f, height);

		// 3️⃣ Centro de la malla
		Vector3 center = (min + max) * 0.5f;

		// 4️⃣ Vértices del cubo centrados en center
		Vector3[] cubeVertices = new Vector3[8];

		float halfHeight = finalHeight / 2f;

		// Inferior (y = center.y - halfHeight)
		cubeVertices[0] = center + new Vector3(-width / 2f, -halfHeight, -depth / 2f);
		cubeVertices[1] = center + new Vector3(width / 2f, -halfHeight, -depth / 2f);
		cubeVertices[2] = center + new Vector3(width / 2f, -halfHeight, depth / 2f);
		cubeVertices[3] = center + new Vector3(-width / 2f, -halfHeight, depth / 2f);

		// Superior (y = center.y + halfHeight)
		cubeVertices[4] = center + new Vector3(-width / 2f, halfHeight, -depth / 2f);
		cubeVertices[5] = center + new Vector3(width / 2f, halfHeight, -depth / 2f);
		cubeVertices[6] = center + new Vector3(width / 2f, halfHeight, depth / 2f);
		cubeVertices[7] = center + new Vector3(-width / 2f, halfHeight, depth / 2f);

		// 5️⃣ Triángulos
		int[] triangles = new int[]
		{
            // Inferior
            0, 2, 1,
			0, 3, 2,

            // Superior
            4, 5, 6,
			4, 6, 7,

            // Frente
            3, 2, 6,
			3, 6, 7,

            // Derecha
            2, 1, 5,
			2, 5, 6,

            // Atrás
            1, 0, 4,
			1, 4, 5,

            // Izquierda
            0, 3, 7,
			0, 7, 4
		};

		Mesh cubeMesh = new Mesh();
		cubeMesh.name = originalMesh.name + "_MicrobeCollider";
		cubeMesh.vertices = cubeVertices;
		cubeMesh.triangles = triangles;
		cubeMesh.RecalculateNormals();
		cubeMesh.RecalculateBounds();

		return cubeMesh;
	}


	public static Mesh CreateMicrobePartCollider(this BiologicalPart original, float height = 24)
	{
		Mesh mesh = null;
		if (original.prefab == null)
			throw new System.ArgumentException("NO HAY PREFAB");

		var filt = original.prefab.GetComponent<MeshFilter>();

		if (filt != null)
		{
			mesh = filt.sharedMesh;
			return mesh.CreateMicrobePartCollider(height);
		}
		throw new System.ArgumentException("algo paso a lo mejor no hay meshfilter");
	}

	public static Mesh CreateMicrobeBodyCollider(this MicrobeData data , float height = 24)
	{
		Mesh mesh = (UnityEngine.Mesh) data.Mesh; //que bueno que tengo esta conversion implementada
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return  mesh.CreateMicrobePartCollider(height);
	}
}


