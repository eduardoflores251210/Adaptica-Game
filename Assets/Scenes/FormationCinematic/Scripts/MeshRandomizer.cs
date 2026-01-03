using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshRandomizer : MonoBehaviour
{
    public List<Mesh> Meshes;
    // Start is called before the first frame update
    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (Meshes == null)
            return;
        if (Meshes.Count > 0)
            meshFilter.mesh = Meshes[Random.Range(0, Meshes.Count-1)];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
