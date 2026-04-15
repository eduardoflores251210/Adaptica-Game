using SerializableTypes.Biology;
using System;
using System.Collections.Generic;
using Mesh = StandartUtilities.StdUtils.Serializable.Mesh;
using Transform = StandartUtilities.StdUtils.Serializable.Transform; //basicamente son 3 vector 3 Pos Rot y Scale

namespace SerializableTypes.Game.City
{
	[Serializable]
	public struct CityData
	{
		public Mesh RoadPathMesh;
		public Mesh TrackMesh;
		public List<BuildingData> Buildings;
	}
	[Serializable]
	public struct BuildingData
	{
		public Buiding_Type Type;
		public int Level;
		public Transform transform;
	}
	/// <summary>
	/// otro enum largo pero necesario
	/// a lo mejor no se usan todos los tipos de edificios pero estan aqui por si acaso
	/// </summary>
	public enum Buiding_Type
	{
		None = 0,
		House = 1,
		Factory = 2,
		Water_Pump = 3,
		ElectricalGenerator = 4,
		Farm = 5,
		Schools_Kinder = 6,
		Schools_Elementary = 7,
		Schools_Middle = 8,
		Schools_FarmHigh = 9,
		Schools_TechnicalHigh = 10,
		Schools_IndustrialHigh = 11,
		Schools_GeneralHigh = 12,
		Schools_University_Medical = 13,
		Schools_University_Farm = 14,
		Schools_University_Technical = 15,
		Schools_University_Industrial = 16,
		Schools_University_General = 17,
		Schools_University_Space = 18,
		Schools_DayCare = 19,
		Schools_AllInOne = 20,
		Schools_Unknown = 21,
		Hospital = 22,
		FireFighter_Station = 23,
		FireFighter_Command = 24,
		Police_Comisary = 25,
		Police_Prison = 26,
		Police_SherifOfice = 27,
		Police_Command = 28,
		Park_Small = 29,
		Park_Medium = 30,
		Park_Large = 31,
		Park_Amusesment = 32,
		Holy_Church = 33,
		Holy_Temple = 34,
		Holy_Generic = 35,
		Transport_Bus_Stop = 36,
		Transport_Bus_BigStation = 37,
		Transport_Bus_Station = 38,
		Transport_Taxi_Stop = 39,
		Transport_Taxi_Base = 40,
		Transport_Metro_Station = 41,
		Transport_Metro_BigStation = 42,
		Transport_Train_Station = 43,
		Transport_Train_BigStation = 44,
		Transport_Train_StationFarm = 45,
		Transport_Teleferic_Station = 46,
		Transport_Teleferic_Depo = 47,
		Transport_Generic_CETRAM_Small = 48,
		Transport_Generic_CETRAM_Big = 49,
		Transport_Generic_DepartamentOfMovility = 50,

	}


}