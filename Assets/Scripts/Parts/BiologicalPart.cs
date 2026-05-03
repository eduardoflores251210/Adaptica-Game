using UnityEngine;

[CreateAssetMenu(fileName = "NewBioPart", menuName = "Parts/Biological Part")]
public class BiologicalPart : BasePart
{    
    public float attackPower;
    public float movementBoost;
    public BiologicalPartFunction function;
    // Comportamiento de reproducción, etc.
}
public enum BiologicalPartFunction
{
    none = -1,
    Mouth = 0,
    eye= 1,
    healthincreace= 2,
    VelIncreace = 3,
    Weapon = 4,
}