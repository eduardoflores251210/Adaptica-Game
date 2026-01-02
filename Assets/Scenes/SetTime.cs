using FixedMath;
using SerializableTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTime : MonoBehaviour
{
    public PlanetOrbit PlanetOrbit;
    public DayTime DayTime;
    public bool Set;
    public bool IsIncompatible;
    // Start is called before the first frame update
    void Start()
    {
        TestComplexFixed128 test = new TestComplexFixed128();
        test.TestTrigonometry();
        test.BasicTest();
        test.TestRoots();
        test.FULLTEST();
    }
	public Material mat; // asigna el material en el inspector

	void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		if (mat == null)
		{
			Graphics.Blit(src, dest);
			return;
		}

		Camera cam = Camera.current;

		// Pasa matrices necesarias al shader
		// Inversa de la proyección (clip -> view)
		Matrix4x4 invProj = cam.projectionMatrix.inverse;
		// Matriz camera->world (view -> world)
		Matrix4x4 camToWorld = cam.cameraToWorldMatrix;

		mat.SetMatrix("_CameraInverseProjection", invProj);
		mat.SetMatrix("_CameraToWorld", camToWorld);
		mat.SetVector("_CameraPos", cam.transform.position);

		Graphics.Blit(src, dest, mat);
	}
	// Update is called once per frame
	void Update()
    {
        IsIncompatible = !DayTime.IsCompatible(PlanetOrbit.CurrentDay);
        if (IsIncompatible)
        {
            
            Set = false;
        }
        if (Set)
        {
            PlanetOrbit.SetCurrentDayAndTime(DayTime);
            Set = !Set;
        }
    }
}

