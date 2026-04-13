using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bu = SpaceUtils.SystemObjectsBuilder;
using SysData = SpaceUtils.SystemObjectsBuilder.InstantiatedSystemData;

public class LoadASYstem : MonoBehaviour
{
	public Material BasMat; // SI O SI DEBE USAR EL SHADER StarShader
	public Material BholMat; //este material brilla por que es el del disco de acrecion
	public Material OpaceMat; //este material es opaco por que planetas y enanas negras
    public Material GasMaterial;
    public Mesh SphereMesh;
    public string SolarSystemID = "";
    public bool LOADNOW = false;
	public bool Done = false;
    public SysData systemData;
	bool Called = false;
							  // Start is called before the first frame update
	void Start()
    {
        
    }

	// Update is called once per frame
	public Transform tr = null;
    void Update()
    {
		if (LOADNOW)
		{
			StartCoroutine(aaa() );
		}
	}
	public IEnumerator aaa ()
	{
		LOADNOW = false;
		if (!Called)
		{
			bu.OpaceMat = OpaceMat;
			bu.BaseMat = BasMat;
			bu.BholMat = BholMat;
			bu.BaseGasMaterial = GasMaterial;
			bu.CacheSphere = SphereMesh;
			
		}
		yield return bu.InstantiateSystemCo(SolarSystemID, tr != null, tr);
		while (!bu.done)
		{
			yield return null;
		}
		Done = true;
	}
    public void a ()
    {
		LOADNOW = true;
		bu.OpaceMat = OpaceMat;
		bu.BaseMat = BasMat;
		bu.BholMat = BholMat;
		bu.BaseGasMaterial = GasMaterial;
		bu.CacheSphere = SphereMesh;
		Called = false;

	}
	public void a (Transform transform)
    {
		LOADNOW = true;
		bu.OpaceMat = OpaceMat;
		bu.BaseMat = BasMat;
		bu.BholMat = BholMat;
		bu.BaseGasMaterial = GasMaterial;
		bu.CacheSphere = SphereMesh;
		tr = transform;
		Called = true;
	}
    public void a (Transform transform, Material Opace,Material Base, Material Gas, Material Bhol)
    {
		LOADNOW = true;
		bu.OpaceMat =Opace;
		bu.BaseMat = Base;
		bu.BholMat = Bhol;
		bu.BaseGasMaterial = Gas;
		bu.CacheSphere = SphereMesh;
		tr =transform;
		Called = true;

	}
}
