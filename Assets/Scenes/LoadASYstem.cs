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
    public string SolarSystemID = "";
    public bool LOADNOW = false;
    public SysData systemData;
							  // Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
      if (LOADNOW)
        {
            LOADNOW = false;
            bu.OpaceMat = OpaceMat;
            bu.BaseMat = BasMat;
            bu.BholMat = BholMat;
            bu.BaseGasMaterial = GasMaterial;
			bu.InstantiateSystem(SolarSystemID);

        }
    }
}
