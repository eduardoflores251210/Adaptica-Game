//using FixedMath;
using StandartUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Mesh = StandartUtilities.StdUtils.Serializable.Mesh;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Transform = StandartUtilities.StdUtils.Serializable.Transform; //basicamente son 3 vector 3 Pos Rot y Scale
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;


namespace SerializableTypes.Biology
{
	//la montaña de enums que controlan la biología del juego
	#region Enums

	//antes decia Sexo pero por Cosas de padres sobreprotectores aunque no he lanzzado el juego
	//decidi usar "genero" aunque lo cientifico es decir "Sexo"
	public enum GéneroBiológico
	{
		None = -1,
		Male,
		Female
	}
	public enum Diets
	{
		none = -1,
		Omnivore, //omnonnom Carne y Plantas
		Carnivore,// ESA GACELA SE VEÍA TAN DELICIOSA QUE ME LA COMÍ
		Herbivore// SOLO PLANTAS, POBRES PLANTAS
	}
	[Obsolete("Usa el otro ActionTypes instead")]
	public enum ActionTypes
	{
		none = -1,
		eat,
		evolve,
		die,

		ChangeStage,
		layegg,
		AtackOtherAnimal,
		KillotherAnimal,
		befriendAnimal,
		AllySpecies,
		getPregnant,
		Miscarry,
		// ... habra mas cuando desarolle el estadio tribal civilisacion y mas pero ahora solo los de criatura y Microbio
	}
	[Serializable]
	public enum editorphase
	{
		BodyEdit,
		PartEditAndBodyColouring,
		NamingAndSaving
	}
	public enum reproductionTypes
	{
		SingleCell, //mejor dicho Asexual
		MultiCell, //mejor dicho Sexual
	}
	public enum ReproductionMethod
	{
		#region SingleCell
		SplitIn2 = 0, //antes era mitosis pero esto NO ES una celula es un muicrobio multicelular con organos internos y todo, asi que no es mitosis, es mas como una division celular pero a lo bestia, por eso SplitIn2
		Fragmentation,
		Budding,          // Gemation → Budding
		EggBasedParthenogenesis,    // partenogenesisEgg → EggBasedParthenogenesis
		PregnancyBasedParthenogenesis, // partenogenesisPreggnancy → PregnancyBasedParthenogenesis
		#endregion

		#region MultiCell
		EggBased,     // Egg
		PregnancyBased // Pregnancy
		#endregion
	}
	public enum PlantReproductionMethod
	{
		#region SingleCell

		Fragmentation,
		Budding,
		SeedPartenogenesis,
		Spore, // esporas, como los helechos y hongos, y referencia a Mi Juego inspirado a este proyecto SPORE(tm) (EA)
		#endregion

		#region MultiCell
		Seed,

		#endregion
	}



	#endregion
	#region bio
	/// <summary>
	/// Basicamente Metabola Serializada
	/// </summary>
	[Serializable]
	public class SegmentData
	{
		public float radius;
		public Transform transform;
	}


	[Serializable]
	public class SerializedPartData
	{
		public string Id;
		public Transform transform;
		public SerializedPartData(string id, Transform transform)
		{
			Id = id;
			this.transform = transform;
		}
		public override string ToString()
		{
			return $"Part: {Id}, {transform}";
		}
		public override int GetHashCode()
		{
			return Id.GetHashCode() ^ transform.GetHashCode();
		}
		public static bool IsValidMethodTypePair(reproductionTypes type, ReproductionMethod method)
		{
			if (type == reproductionTypes.MultiCell)
			{
				if ((method != ReproductionMethod.EggBased) && (method != ReproductionMethod.PregnancyBased))
					return false;
				else return true;
			}
			else if (type == reproductionTypes.SingleCell)
			{
				if ((method == ReproductionMethod.EggBased) || (method == ReproductionMethod.PregnancyBased))
					return false;
				else return true;
			}
			else return false;
		}
	}

	[Serializable]
	public abstract class CreatureBase
	{
		public string Name;
		public string Description;
		public bool HasMale;
		public Color FemaleColor;
		public Color MaleColor;
		public List<SerializedPartData> PartsF;
		public List<SerializedPartData> PartsM;
		public List<SegmentData> Segments;
		public Mesh Mesh;

		protected CreatureBase() { }

		protected CreatureBase(string name, string description, bool hasMale, Color femaleColor, Color maleColor, List<SerializedPartData> partsF, List<SerializedPartData> partsM, List<SegmentData> segments, Mesh mesh)
		{
			Name = name;
			Description = description;
			HasMale = hasMale;
			FemaleColor = femaleColor;
			MaleColor = maleColor;
			PartsF = partsF;
			PartsM = hasMale ? partsM : partsF;
			Segments = segments;
			Mesh = mesh;
		}

		public virtual void CenterCreature()
		{
			if (Mesh == null) return;
			Vector3 offset = Mesh.CenterMesh();

			if (PartsF != null)
			{
				foreach (var part in PartsF)
					part.transform.Pos -= offset;
			}

			if (PartsM != null)
			{
				foreach (var part in PartsM)
					part.transform.Pos -= offset;
			}

			if (Segments != null)
			{
				foreach (var seg in Segments)
					seg.transform.Pos -= offset;
			}
		}

		public virtual void RotateEuler(Vector3 eulerAngles)
		{
			Mesh?.RotateEuler(eulerAngles);
			Vector3 pivot = Vector3.zero;
			if (Mesh != null && Mesh.Vertices != null && Mesh.Vertices.Count > 0)
			{
				foreach (var v in Mesh.Vertices) pivot += v;
				pivot /= Mesh.Vertices.Count;
			}

			float radX = eulerAngles.x * Mathf.Deg2Rad;
			float radY = eulerAngles.y * Mathf.Deg2Rad;
			float radZ = eulerAngles.z * Mathf.Deg2Rad;

			float cosX = Mathf.Cos(radX);
			float sinX = Mathf.Sin(radX);
			float cosY = Mathf.Cos(radY);
			float sinY = Mathf.Sin(radY);
			float cosZ = Mathf.Cos(radZ);
			float sinZ = Mathf.Sin(radZ);

			Vector3 RotateVectorLocal(Vector3 v)
			{
				float y1 = v.y * cosX - v.z * sinX;
				float z1 = v.y * sinX + v.z * cosX;
				v.y = y1; v.z = z1;

				float x2 = v.x * cosY + v.z * sinY;
				float z2 = -v.x * sinY + v.z * cosY;
				v.x = x2; v.z = z2;

				float x3 = v.x * cosZ - v.y * sinZ;
				float y3 = v.x * sinZ + v.y * cosZ;
				v.x = x3; v.y = y3;

				return v;
			}

			if (PartsF != null)
			{
				foreach (var part in PartsF)
				{
					Vector3 worldPos = part.transform.Pos;
					Vector3 rel = worldPos - pivot;
					Vector3 relRot = RotateVectorLocal(rel);
					part.transform.Pos = pivot + relRot;

					if (part.transform.Rot != null)
					{
						part.transform.Rot += eulerAngles;
						part.transform.Rot = new Vector3(
							Mathf.Repeat(part.transform.Rot.x + 180f, 360f) - 180f,
							Mathf.Repeat(part.transform.Rot.y + 180f, 360f) - 180f,
							Mathf.Repeat(part.transform.Rot.z + 180f, 360f) - 180f);
					}
				}
			}

			if (PartsM != null	&& HasMale) //si no tiene macho no rota partesM por  que sera la misma referencia que partesF (F es Femenino y M Masculino)
			{
				foreach (var part in PartsM)
				{
					Vector3 worldPos = part.transform.Pos;
					Vector3 rel = worldPos - pivot;
					Vector3 relRot = RotateVectorLocal(rel);
					part.transform.Pos = pivot + relRot;

					if (part.transform.Rot != null)
					{
						part.transform.Rot += eulerAngles;
						part.transform.Rot = new Vector3(
							Mathf.Repeat(part.transform.Rot.x + 180f, 360f) - 180f,
							Mathf.Repeat(part.transform.Rot.y + 180f, 360f) - 180f,
							Mathf.Repeat(part.transform.Rot.z + 180f, 360f) - 180f);
					}
				}
			}
		}
	}



	[Serializable]
	public class MicrobeData : CreatureBase
	{
		public reproductionTypes RepType;
		public ReproductionMethod RepMeth; //DICE metodo no metanfetamina
		public MicrobeData(string name, string description, bool hasMale, reproductionTypes repType, ReproductionMethod repMeth, List<SerializedPartData> partsF, List<SerializedPartData> partsM, Color femaleColor, Color maleColor, List<SegmentData> segments, Mesh mesh)
			: base(name, description, hasMale, femaleColor, maleColor,  partsF, partsM, segments, mesh)
		{
			RepMeth = repMeth;
			RepType = repType;
		}

		public static string GenerateMicrobeID(MicrobeData microbe)
		{
			// Serializar todo el microbio a JSON
			string json = JsonUtility.ToJson(microbe);

			// Concatenar con la hora actual para que siempre sea único
			string input = json + DateTime.Now.ToString("o");

			// Generar SHA-512
			using SHA512 sha = SHA512.Create();
			byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));

			// Convertir a hexadecimal
			StringBuilder sb = new StringBuilder();
			foreach (byte b in hashBytes)
				sb.Append(b.ToString("x2"));

			return sb.ToString();
		}
		/// <summary>
		/// Centra un MicrobeData moviendo su Mesh y sus PartsF/PartsM
		/// </summary>
		public void CenterMicrobe()
		{
			MicrobeData microbe = this;
			if (microbe.Mesh != null)
			{
				// Centrar la malla principal y obtener el offset
				Vector3 offset = microbe.Mesh.CenterMesh();

				// Aplicar offset a PartsF
				foreach (var part in microbe.PartsF)
				{
					part.transform.Pos -= offset;
				}
				if (HasMale)
					// Aplicar offset a PartsM
					foreach (var part in microbe.PartsM)
					{
					part.transform.Pos -= offset;
					}
				
				//aplicar offset a Segments
				foreach (var Seg in microbe.Segments)
				{
					Seg.transform.Pos -= offset;
				}
			}
		}

		/// <summary>
		/// Rota todo el microbio (Mesh y partes) alrededor del origen usando ángulos Euler en grados.
		/// </summary>
		/// <param name="eulerAngles">Vector3 con los ángulos en grados (X, Y, Z)</param>
		/// <summary>
		/// Rota todo el microbio (Mesh y partes) alrededor del centro de la malla (pivot)
		/// usando ángulos Euler en grados (orden X -> Y -> Z).
		/// </summary>
		public void RotateMicrobeEuler(Vector3 eulerAngles)
		{
			// 1) Rotar la malla primero (operación in-place en vértices)
			Mesh?.RotateEuler(eulerAngles);

			// 2) Calcular pivot (centroide) en las coordenadas actuales de la malla
			Vector3 pivot = Vector3.zero;
			if (Mesh != null && Mesh.Vertices != null && Mesh.Vertices.Count > 0)
			{
				foreach (var v in Mesh.Vertices) pivot += v;
				pivot /= Mesh.Vertices.Count;
			}
			else
			{
				// Fallback al origen si no hay malla
				pivot = Vector3.zero;
			}

			// 3) Convertir grados a radianes y precalcular cos/sin (mismo RotateVector)
			float radX = eulerAngles.x * Mathf.Deg2Rad;
			float radY = eulerAngles.y * Mathf.Deg2Rad;
			float radZ = eulerAngles.z * Mathf.Deg2Rad;

			float cosX = Mathf.Cos(radX);
			float sinX = Mathf.Sin(radX);
			float cosY = Mathf.Cos(radY);
			float sinY = Mathf.Sin(radY);
			float cosZ = Mathf.Cos(radZ);
			float sinZ = Mathf.Sin(radZ);

			Vector3 RotateVectorLocal(Vector3 v)
			{
				// Rotación X
				float y1 = v.y * cosX - v.z * sinX;
				float z1 = v.y * sinX + v.z * cosX;
				v.y = y1;
				v.z = z1;

				// Rotación Y
				float x2 = v.x * cosY + v.z * sinY;
				float z2 = -v.x * sinY + v.z * cosY;
				v.x = x2;
				v.z = z2;

				// Rotación Z
				float x3 = v.x * cosZ - v.y * sinZ;
				float y3 = v.x * sinZ + v.y * cosZ;
				v.x = x3;
				v.y = y3;

				return v;
			}

			// 4) Rotar partes femeninas y masculinas alrededor del pivot
			if (PartsF != null)
			{
				foreach (var part in PartsF)
				{
					// Usamos posiciones relativas al pivot
					Vector3 worldPos = part.transform.Pos; // asumo que esto está en coordenadas locales del microbio
					Vector3 rel = worldPos - pivot;
					Vector3 relRot = RotateVectorLocal(rel);
					part.transform.Pos = pivot + relRot;

					// Rotar la orientación Euler aproximando por suma (mejor que no rotar)
					if (part.transform.Rot != null) // si es Vector3, ajusta directamente
					{
						part.transform.Rot += eulerAngles;
						// opcional: normalizar ángulos a [-180,180]
						part.transform.Rot = new Vector3(
							Mathf.Repeat(part.transform.Rot.x + 180f, 360f) - 180f,
							Mathf.Repeat(part.transform.Rot.y + 180f, 360f) - 180f,
							Mathf.Repeat(part.transform.Rot.z + 180f, 360f) - 180f
						);
					}
				}
			}
			//evitamso rotar mas de 1 vez
			if (StdUtils.Comparisons.ListsAreEqual(PartsF, PartsM))
			{
				if (PartsM != null)
				{
					foreach (var part in PartsM)
					{
						Vector3 worldPos = part.transform.Pos;
						Vector3 rel = worldPos - pivot;
						Vector3 relRot = RotateVectorLocal(rel);
						part.transform.Pos = pivot + relRot;

						if (part.transform.Rot != null)
						{
							part.transform.Rot += eulerAngles;
							part.transform.Rot = new Vector3(
								Mathf.Repeat(part.transform.Rot.x + 180f, 360f) - 180f,
								Mathf.Repeat(part.transform.Rot.y + 180f, 360f) - 180f,
								Mathf.Repeat(part.transform.Rot.z + 180f, 360f) - 180f
							);
						}
					}
				}
			}
		}

		// Overrides to use base virtual contract
		public override void CenterCreature()
		{
			CenterMicrobe();
		}

		public override void RotateEuler(Vector3 eulerAngles)
		{
			RotateMicrobeEuler(eulerAngles);
		}

		public static MicrobeData GetDefaultMicrobe()
		{
			string BAseOBJ = "# 8 vertices del cubo\r\nv -1 -1 -1\r\nv  1 -1 -1\r\nv  1  1 -1\r\nv -1  1 -1\r\nv -1 -1  1\r\nv  1 -1  1\r\nv  1  1  1\r\nv -1  1  1\r\r\n# 12 triángulos (2 por cara) - normales invertidas\r\n# Cara frontal\r\nf 3 2 1\r\nf 4 3 1\r\n\r\n# Cara trasera\r\nf 7 8 5\r\nf 6 7 5\r\n\r\n# Cara izquierda\r\nf 8 4 1\r\nf 5 8 1\r\n\r\n# Cara derecha\r\nf 7 6 2\r\nf 3 7 2\r\n\r\n# Cara superior\r\nf 7 3 4\r\nf 8 7 4\r\n\r\n# Cara inferior\r\nf 6 5 1\r\nf 2 6 1\r\n";
			MicrobeData Tempdata;
			var EmergencyMEsh = StandartUtilities.StdUtils.Serializable.Mesh.FromObjString(BAseOBJ);
			Tempdata = new MicrobeData("AAAAA",
			                          "AAAA",
			                          false,
			                          reproductionTypes.SingleCell,
			                          ReproductionMethod.SplitIn2,
			                          new List<SerializedPartData>() {
			                          new("0", new StdUtils.Serializable.Transform(Vector3.forward, Quaternion.identity.eulerAngles, Vector3.one)),
			                          new("-1", new StdUtils.Serializable.Transform(Vector3.up, Quaternion.identity.eulerAngles, Vector3.one))
			                          },
			                          null,
			                          Color.magenta,
			                          Color.blue,
			                          new() { new() { radius = 3 } }, mesh: EmergencyMEsh);
			return Tempdata;
		}

		public AnimalData ToAnimal()
		{
			return new AnimalData(Name, Description, HasMale, RepType, RepMeth, PartsF, PartsM, FemaleColor, MaleColor, Segments, Mesh);
		}
		public void TransFromMicrobe(Transform transformation)
		{
			// Aplicar transformación a la malla
			int i = 0;
			foreach (var item in Mesh.Vertices)
			{
				Mesh.Vertices[i] = item+ transformation.Pos;
				i++;
			}
			Mesh.RotateEuler(transformation.Rot);
			i = 0; //reaprovechar variable reseteandola a 0	
			foreach (var item in Mesh.Vertices)
			{
				Mesh.Vertices[i] = Vector3.Scale(item, transformation.Scale);
				i++;
			}//creo que un for seria mas eficiente pero bueno
			 // Aplicar transformación a las partes femeninas
			if (PartsF != null)
			{
				foreach (var part in PartsF)
				{
					Transform temp = part.transform;
					temp.Rot = new Vector3(
						temp.Rot.x * transformation.Scale.x,
						temp.Rot.y * transformation.Scale.y,
						temp.Rot.z * transformation.Scale.z);
					temp.Pos = temp.Pos + transformation.Pos;
					temp.Scale = Vector3.Scale(temp.Scale, transformation.Scale);
					part.transform = temp;
				}
			}
			// Aplicar transformación a las partes masculinas
			if (PartsM != null && HasMale)
			{
				foreach (var part in PartsM)
				{
					Transform temp = part.transform;
					temp.Rot = temp.Rot + transformation.Rot;
					//ahora a quitar lo sobrante de la suma de rotación si pasa 360
					if (temp.Rot.x > 360f) temp.Rot.x -= 360f;
					if (temp.Rot.y > 360f) temp.Rot.y -= 360f;
					if (temp.Rot.z > 360f) temp.Rot.z -= 360f;

					temp.Pos = temp.Pos + transformation.Pos;
					temp.Scale = Vector3.Scale(temp.Scale, transformation.Scale);
					part.transform = temp;
				}
			}
		}
	}

	[Serializable]
	public class AnimalData : CreatureBase
	{
		public reproductionTypes RepType;
		public ReproductionMethod RepMeth; //DICE metodo no metanfetamina por que hay gente malpensada.
		public List<MicrobeData> Limbs; // SI internamente las Extremidades son Microbios por que  asi es mas facil de hacer que sean Dinamicas 
		public AnimalData(string name, string description, bool hasMale, reproductionTypes repType, ReproductionMethod repMeth, List<SerializedPartData> partsF, List<SerializedPartData> partsM, Color femaleColor, Color maleColor, List<SegmentData> segments, Mesh mesh)
	: base(name, description, hasMale, femaleColor, maleColor, partsF, partsM, segments, mesh)
		{
			RepMeth = repMeth;
			RepType = repType;
			Limbs = new(); //si listas vacia por defecto
		}
		public AnimalData(string name, string description, bool hasMale, reproductionTypes repType, ReproductionMethod repMeth, List<SerializedPartData> partsF, List<SerializedPartData> partsM, Color femaleColor, Color maleColor, List<SegmentData> segments, Mesh mesh, List<MicrobeData> limbs)
	: base(name, description, hasMale, femaleColor, maleColor, partsF, partsM, segments, mesh)
		{
			RepMeth = repMeth;
			RepType = repType;
			Limbs = limbs;
		}

		public static string GenerateAnimalID(AnimalData Animal)
		{
			// Serializar todo el microbio a JSON
			string json = JsonUtility.ToJson(Animal);

			// Concatenar con la hora actual para que siempre sea único
			string input = json + DateTime.Now.ToString("o");

			// Generar SHA-512
			using SHA512 sha = SHA512.Create();
			byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));

			// Convertir a hexadecimal
			StringBuilder sb = new StringBuilder();
			foreach (byte b in hashBytes)
				sb.Append(b.ToString("x2"));

			return sb.ToString();
		}
		/// <summary>
		/// Centra un MicrobeData digo AnimalData moviendo su Mesh y sus PartsF/PartsM
		/// </summary>
		public void CenterAnimal()
		{
			if (Mesh != null)
			{
				// Centrar la malla principal y obtener el offset
				Vector3 offset = Mesh.CenterMesh();

				// Aplicar offset a PartsF
				foreach (var part in PartsF)
				{
					part.transform.Pos -= offset;
				}

				// Aplicar offset a PartsM
				if (HasMale)
					foreach (var part in PartsM)
					{
						part.transform.Pos -= offset;
					}
				if (Limbs != null)
				{
					foreach (var limb in Limbs)
					{
						limb.TransFromMicrobe(new(offset,Vector3.zero, Vector3.one)); //solo aplico la traslacion por que eso solo centra posiciones No rotaciones ni escalados
					}
				}

				//aplicar offset a Segments
				foreach (var Seg in Segments)
				{
					Seg.transform.Pos -= offset;
				}
			}
		}

		/// <summary>
		/// Rota todo el microbio (Mesh y partes) alrededor del origen usando ángulos Euler en grados.
		/// </summary>
		/// <param name="eulerAngles">Vector3 con los ángulos en grados (X, Y, Z)</param>
		/// <summary>
		/// Rota todo el microbio (Mesh y partes) alrededor del centro de la malla (pivot)
		/// usando ángulos Euler en grados (orden X -> Y -> Z).
		/// </summary>
		public void RotateAnimalEuler(Vector3 eulerAngles)
		{
			// 1) Rotar la malla primero (operación in-place en vértices)
			Mesh?.RotateEuler(eulerAngles);

			// 2) Calcular pivot (centroide) en las coordenadas actuales de la malla
			Vector3 pivot = Vector3.zero;
			if (Mesh != null && Mesh.Vertices != null && Mesh.Vertices.Count > 0)
			{
				foreach (var v in Mesh.Vertices) pivot += v;
				pivot /= Mesh.Vertices.Count;
			}
			else
			{
				// Fallback al origen si no hay malla
				pivot = Vector3.zero;
			}

			// 3) Convertir grados a radianes y precalcular cos/sin (mismo RotateVector)
			float radX = eulerAngles.x * Mathf.Deg2Rad;
			float radY = eulerAngles.y * Mathf.Deg2Rad;
			float radZ = eulerAngles.z * Mathf.Deg2Rad;

			float cosX = Mathf.Cos(radX);
			float sinX = Mathf.Sin(radX);
			float cosY = Mathf.Cos(radY);
			float sinY = Mathf.Sin(radY);
			float cosZ = Mathf.Cos(radZ);
			float sinZ = Mathf.Sin(radZ);

			Vector3 RotateVectorLocal(Vector3 v)
			{
				// Rotación X
				float y1 = v.y * cosX - v.z * sinX;
				float z1 = v.y * sinX + v.z * cosX;
				v.y = y1;
				v.z = z1;

				// Rotación Y
				float x2 = v.x * cosY + v.z * sinY;
				float z2 = -v.x * sinY + v.z * cosY;
				v.x = x2;
				v.z = z2;

				// Rotación Z
				float x3 = v.x * cosZ - v.y * sinZ;
				float y3 = v.x * sinZ + v.y * cosZ;
				v.x = x3;
				v.y = y3;

				return v;
			}

			// 4) Rotar partes femeninas y masculinas alrededor del pivot
			if (PartsF != null)
			{
				foreach (var part in PartsF)
				{
					// Usamos posiciones relativas al pivot
					Vector3 worldPos = part.transform.Pos; // asumo que esto está en coordenadas locales del microbio
					Vector3 rel = worldPos - pivot;
					Vector3 relRot = RotateVectorLocal(rel);
					part.transform.Pos = pivot + relRot;

					// Rotar la orientación Euler aproximando por suma (mejor que no rotar)
					if (part.transform.Rot != null) // si es Vector3, ajusta directamente
					{
						part.transform.Rot += eulerAngles;
						// opcional: normalizar ángulos a [-180,180]
						part.transform.Rot = new Vector3(
							Mathf.Repeat(part.transform.Rot.x + 180f, 360f) - 180f,
							Mathf.Repeat(part.transform.Rot.y + 180f, 360f) - 180f,
							Mathf.Repeat(part.transform.Rot.z + 180f, 360f) - 180f
						);
					}
				}
			}

			if (PartsM != null)
			{
				foreach (var part in PartsM)
				{
					Vector3 worldPos = part.transform.Pos;
					Vector3 rel = worldPos - pivot;
					Vector3 relRot = RotateVectorLocal(rel);
					part.transform.Pos = pivot + relRot;

					if (part.transform.Rot != null)
					{
						part.transform.Rot += eulerAngles;
						part.transform.Rot = new Vector3(
							Mathf.Repeat(part.transform.Rot.x + 180f, 360f) - 180f,
							Mathf.Repeat(part.transform.Rot.y + 180f, 360f) - 180f,
							Mathf.Repeat(part.transform.Rot.z + 180f, 360f) - 180f
						);
					}
				}
			}
		}

		// Overrides to use base virtual contract
		public override void CenterCreature()
		{
			CenterAnimal();
		}

		public override void RotateEuler(Vector3 eulerAngles)
		{
			RotateAnimalEuler(eulerAngles);
		}

		public static AnimalData GetDefaultAnimal()
		{
			string BAseOBJ = "# 8 vertices del cubo\r\nv -1 -1 -1\r\nv  1 -1 -1\r\nv  1  1 -1\r\nv -1  1 -1\r\nv -1 -1  1\r\nv  1 -1  1\r\nv  1  1  1\r\nv -1  1  1\r\r\n# 12 triángulos (2 por cara) - normales invertidas\r\n# Cara frontal\r\nf 3 2 1\r\nf 4 3 1\r\n\r\n# Cara trasera\r\nf 7 8 5\r\nf 6 7 5\r\n\r\n# Cara izquierda\r\nf 8 4 1\r\nf 5 8 1\r\n\r\n# Cara derecha\r\nf 7 6 2\r\nf 3 7 2\r\n\r\n# Cara superior\r\nf 7 3 4\r\nf 8 7 4\r\n\r\n# Cara inferior\r\nf 6 5 1\r\nf 2 6 1\r\n";
			AnimalData Tempdata;
			var EmergencyMEsh = StandartUtilities.StdUtils.Serializable.Mesh.FromObjString(BAseOBJ);
			Tempdata = new ("AAAAA",
									  "AAAA",
									  false,
									  reproductionTypes.SingleCell,
									  ReproductionMethod.SplitIn2,//Mitosis, //AKAJAJAJA anima mitosenado... ah claro por que el gato se va a dividir en 2.... (si sarcasmo) pero ya lo corregi por SplitIn2 que es mas generico y no tan biologicamente incorrecto para un animal, ademas de que el metodo de reproduccion no es algo que afecte a la forma del animal en si, asi que da igual que se llame Mitosis o SplitIn2
									  new List<SerializedPartData>() {
									  new("0", new StdUtils.Serializable.Transform(Vector3.forward, Quaternion.identity.eulerAngles, Vector3.one)),
									  new("-1", new StdUtils.Serializable.Transform(Vector3.up, Quaternion.identity.eulerAngles, Vector3.one))
									  },
									  null,
									  Color.magenta, //las niñas son rosas y los niños azules, lo siento pero es asi, no es mi culpa que la sociedad sea asi, yo solo lo reflejo en el juego, no es mi culpa que el rosa sea un color tan bonito y femenino y el azul un color tan feo y masculino, lo siento de verdad pero es asi, no es mi culpa, no me odien por esto por favor, no me maten, no me hagan bullying
									  Color.blue,//eso lo escribio el Github copilot. no yo, quise decir que es rosa y azul por motivos sociales que ayudan a identiifcar.
									  new() { new() { radius = 3 } }, mesh: EmergencyMEsh);
			return Tempdata;
		}
	}


	/// <summary>
	/// Guarda toda la información serializable de una planta
	/// 
	/// </summary>
	[Serializable]
	public class PlantData
	{
		public string Name;
		public string Description;
		public bool HasFruit;
		public Color LeavesColor;
		public Color StemColor;
		public reproductionTypes RepType;
		public PlantReproductionMethod RepMeth; //DICE metodo no metanfetamina
		public List<SerializedPartData> Parts;
		public List<SegmentData> Segments;
		public Mesh Mesh;

		public PlantData(string name, string description, bool hasFruit, Color leavesColor, Color stemColor, reproductionTypes repType, PlantReproductionMethod repMeth, List<SerializedPartData> parts, List<SegmentData> segments, Mesh mesh)
		{
			Name = name;
			Description = description;
			HasFruit = hasFruit;
			LeavesColor = leavesColor;
			StemColor = stemColor;
			RepType = repType;
			RepMeth = repMeth;
			Parts = parts;
			Segments = segments;
			Mesh = mesh;
		}
	}

	#endregion
}