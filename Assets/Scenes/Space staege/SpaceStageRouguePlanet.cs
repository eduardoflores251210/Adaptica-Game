using SerializableTypes;
using SerializableTypes.Space;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceStageRouguePlanet : MonoBehaviour
{
	public string ID;
	public StandartUtilities.StdUtils.Serializable.Transform BinTransform;
	public PlanetTypes Type;
	//public List<string> Children;
	public static float MaxDist = 200;
	private SphereCollider Renderer;
	// Start is called before the first frame update
	void Start()
	{
		Renderer = GetComponent<SphereCollider>();
	}

	// Update is called once per frame
	void Update()
	{

		Renderer.enabled = (Vector3.Distance(Camera.main.transform.position, transform.position) < MaxDist);


	}
}
