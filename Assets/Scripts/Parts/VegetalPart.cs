using UnityEngine;

[CreateAssetMenu(fileName = "NewVegPart", menuName = "Parts/Vegetal Part")]
public class VegetalPart : BasePart
{    
    public float SunLightPower;
    public float WaterAbsortionBoost;
    public VegetalPartFunction function;
}
public enum VegetalPartFunction
{
    none = -1,
    Leaf = 0,
    Branch  = 1,
    healthincreace= 2,
	Flower_Or_Fruit = 3,
    RootBranch = 4,
    Sporangium = 5, //para plantas primitivas
}