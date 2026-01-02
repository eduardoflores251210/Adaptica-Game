using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// bloquea la rotación de un objeto
/// es el segundo componente más simple que he hecho en mi vida por debajo de SnapOffset.cs
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Transform))]
public class CeroRot : MonoBehaviour
{	void Start()
	{
		transform.rotation = Quaternion.identity;
	}
	void Update()
	{
		transform.rotation = Quaternion.identity;
	}
}
