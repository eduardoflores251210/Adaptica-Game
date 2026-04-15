using SerializableTypes.Game.City;
using System;
using System.Collections.Generic;

namespace SerializableTypes.Game.SpaceStage
{
	[Serializable]
	public struct PopulatedPlanetData
	{
		public List<CityData> Cities;
	}
	[Serializable]
	public struct SpaceStageData
	{
		public long MONEY_Amount;// capitalismo
		public List<PopulatedPlanetData> PlanetData;
	}
}