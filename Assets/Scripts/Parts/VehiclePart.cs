using UnityEngine;

[CreateAssetMenu(fileName = "NewVehiclePart", menuName = "Parts/Vehicle Part")]
public class VehiclePart : BasePart
{
	public float powerConsumption;
	public float armor;
	public float speedBoost;


	public VehicleFunction Func;
}
public enum VehicleFunction { 
	Wings, 
	Weapon,
	Wheel,
	ChassisPart,
	Lights,
	WindowsXP //osea ventanas
}