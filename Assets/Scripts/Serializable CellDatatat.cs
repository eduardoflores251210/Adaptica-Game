using SerializableTypes.Biology;
using System;

namespace SerializableTypes.Game.Microbe
{
	[Serializable]
	public struct CellGameData : IEquatable<CellGameData>
	{
		public float DNA_Amount;
		public float MaxDNA_Got;
		public float Progress;
		public float PlayerHealth;
		public GéneroBiológico Gender;
		public bool Finished;
		public CellGameData(float dNA_Amount, float maxDNA_Got, float progress, float playerHealth, GéneroBiológico gender)
		{
			DNA_Amount = dNA_Amount;
			MaxDNA_Got = maxDNA_Got;
			Progress = progress;
			PlayerHealth = playerHealth;
			Gender = gender;
			Finished = false;
		}

		public bool Equals(CellGameData other)
		{
			return DNA_Amount == other.DNA_Amount && Progress == other.Progress && PlayerHealth == other.PlayerHealth && Gender == other.Gender && MaxDNA_Got == other.MaxDNA_Got;
		}
		public override bool Equals(object o)
		{
			if (ReferenceEquals(this, o)) return true;
			if (o is null) return false;

			if (o is CellGameData Cell)
			{
				return Equals((CellGameData)o);
			}
			else return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(DNA_Amount, Progress, PlayerHealth, Gender);
		}
		public override string ToString()
		{
			return $"DNA {DNA_Amount}, progresss {Progress}, HP {PlayerHealth}, Gender {Gender}, Done {Finished}";
		}
	}

}