using SerializableTypes;
using SerializableTypes.Biology;
using StandartUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// creo que carga la criatura en el visualizador de criaturas
/// aunque deveria usar el termino Microbio en vez de criatura
/// </summary>
public class CreatureLoader : MonoBehaviour
{
    public string Path;
    public PartsDatabase Parts;
    public GéneroBiológico ActiveGender = GéneroBiológico.Female;
    public Material FMat;
    public Material MMat;


    // Start is called before the first frame update
    void load()
    {
        if (File.Exists(Path))
        {
            string json = File.ReadAllText(Path);
            MicrobeData microbe = JsonUtility.FromJson<MicrobeData>(json);
            if (microbe != null)
            {
                MeshFilter msh;
                if (TryGetComponent<MeshFilter>(out msh))
                {
                    msh = GetComponent<MeshFilter>();

                } else
                {
                    msh = gameObject.AddComponent<MeshFilter>();
                    gameObject.AddComponent<MeshRenderer>();
                }

                //Debug.Log(microbe.Mesh.ToString());
                msh.mesh = microbe.Mesh.ToUnityMesh();
                foreach (var p in microbe.PartsF)
                {
                    Instantiate(Parts.GetPartByID(p.Id).prefab, p.transform.Pos, Quaternion.Euler(p.transform.Rot));
                }
                var FC = microbe.FemaleColor;
                var MC = microbe.MaleColor;
                FC.a = 1;
                MC.a = 1;
                FMat.color = FC;
                MMat.color = MC;
                var aaa = GetComponent<MeshRenderer>();
                aaa.material = ActiveGender switch
                {
                    GéneroBiológico.Female => FMat,
                    GéneroBiológico.Male => MMat,
                    _ => new Material(MMat)

                };
                
            }
        }
    }
    private void Start()
    {
        load();
    } 

}
