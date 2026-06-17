using System;
using System.Collections;
using System.Collections.Generic;
#if GUINEAPIGDOS
using GuineaPigDos.MathStuff;
using System.Drawing;
using GuineaPigDos.Extentions;
using GuineaPigDos.Overrides.MiniLinq;

#else

using UnityEngine;
using UnityEngine.Animations;
using System.Linq;
using Random = UnityEngine.Random;
using ColorFloat = UnityEngine.Color;
using ColorExtentions = UnityEngine.ColorUtility;
#endif
//standard utilities  1.1
// añadida compatibilidad con guineapig dos
// fusionado StandardUtilities For Unity 
// Con StandardUtilities For GuineaPigDos
namespace StandartUtilities
{
	public static class StdUtils
	{
#if GUINEAPIGDOS
		public static Random _rng;
		public  static  Random Random { get 
			{
				if (_rng == null)
					_rng = new Random();
				return _rng; } }
#else
		/// <summary>
		/// 📆 TIEMPO Y DEMORAS
		///</summary>
		public static class Timing
		{
			public static IEnumerator DelayAction(float seconds, System.Action action)
			{
				yield return new WaitForSeconds(seconds);
				action?.Invoke();
			}
		}
#endif
		static float RandomValue()
		{
#if GUINEAPIGDOS
			return Random.Value();
#else
			return Random.value;
#endif
		}
		/// <summary>
		/// 🎲 ALEATORIEDAD
		/// </summary>

		public static class Randomness
		{
			public static bool CoinFlip()
			{
				return RandomValue() > 0.5f;
			}

			public static void ExecuteWithProbability(float probability, System.Action action)
			{
				if (RandomValue() < Mathf.Clamp01(probability))
					action?.Invoke();
			}

			public static int RollD20()
			{
				return Random.Range(1, 21);
			}

			public static Color RandomColor()
			{
				return NewColor(1, RandomValue(), RandomValue(), RandomValue());
			}

			// Función genérica para cualquier enum
			public static T GetRandomEnumValue<T>() where T : System.Enum
			{
				var values = System.Enum.GetValues(typeof(T));
				return (T)values.GetValue(Random.Range(0, values.Length));
			}
		}


		/// <summary>
		/// 🧠 MATEMÁTICAS 
		/// </summary>& NÚMEROS

		public static class Math
		{
			public static Vector2 FlipAxis(Vector2 vector, Eje eje)
			{
				return eje switch
				{
					Eje.Z => throw new System.ArgumentException("En 2D NO HAY Z"),
					Eje.Y => new Vector2(vector.x, -vector.y),
					Eje.X => new Vector2(-vector.x, vector.y),
					_ => vector
				};
			}
			public static Vector3 FlipAxis(Vector3 vector, Eje eje)
			{
				return eje switch
				{
					Eje.Z => new Vector3(vector.x, vector.y, -vector.z),
					Eje.Y => new Vector3(vector.x, -vector.y, vector.z),
					Eje.X => new Vector3(-vector.x, vector.y, vector.z),
					_ => vector
				};
			}
			public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
			{
				return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
			}

			public static float LerpUnclamped(float a, float b, float t)
			{
				return a + (b - a) * t;
			}

			public static float Clamp01Fancy(float value)
			{
				return value < 0 ? 0 : (value > 1 ? 1 : value);
			}

			public static float NormalizeAngle(float angle)
			{
				return (angle % 360 + 360) % 360;
			}

			public static int SafeDivide(int a, int b, int fallback)
			{
				try { return a / b; } catch { return fallback; }
			}

			public static float SafeDivide(float a, float b, float fallback)
			{
				try { return a / b; } catch { return fallback; }
			}

			public static bool IsPrime(int number)
			{
				if (number <= 1) return false;
				if (number == 2) return true;
				if (number % 2 == 0) return false;
				for (int i = 3; i * i <= number; i += 2)
					if (number % i == 0) return false;
				return true;
			}

			public static bool IsEven(int value) => (value % 2) == 0;
			public static bool IsOdd(int value) => !IsEven(value);
			public static bool IsEven(float value) => (value % 2) == 0;
			public static bool IsOdd(float value) => !IsEven(value);
		}

		/// <summary>
		/// 🧬 COMPARACIONES 
		/// </summary>& LISTAS
		public static class Comparisons
		{
			// Porque comparar diccionarios debería ser tan fácil como no es.
			public static bool DictionariesAreEqual<K, V>(Dictionary<K, V> a, Dictionary<K, V> b, float tolerance = 0.0001f)
			{
				// Si uno es null, el otro probablemente también está llorando
				if (a == null || b == null) return false;

				// Si no tienen el mismo número de elementos, adivina qué… no son iguales
				if (a.Count != b.Count) return false;

				foreach (var kvp in a)
				{
					// Si la otra lista ni siquiera tiene la clave, felicitaciones, ya fallaste
					if (!b.ContainsKey(kvp.Key)) return false;

					// Si los valores no son iguales, porque la vida es cruel, tampoco son iguales
					if (!AreEqual(kvp.Value, b[kvp.Key], tolerance)) return false;
				}

				// Si llegaste aquí, eres un héroe, porque aparentemente son iguales
				return true;
			}
			public static bool ListsAreEqual<T>(List<T> a, List<T> b, float tolerance = 0.0001f)
			{
				if (a == null || b == null) return false;
				if (a.Count != b.Count) return false;

				for (int i = 0; i < a.Count; i++)
				{
					if (!AreEqual(a[i], b[i], tolerance))
						return false;
				}
				return true;
			}

			private static bool AreEqual<T>(T a, T b, float tolerance)
			{
				if (typeof(T) == typeof(float))
				{
					return Mathf.Abs((float)(object)a - (float)(object)b) <= tolerance;
				}
				else if (typeof(T) == typeof(Vector2))
				{
					return ((Vector2)(object)a - (Vector2)(object)b).sqrMagnitude <= tolerance * tolerance;
				}
				else if (typeof(T) == typeof(Vector3))
				{
					return ((Vector3)(object)a - (Vector3)(object)b).sqrMagnitude <= tolerance * tolerance;
				}
				else if (typeof(T) == typeof(Vector4))
				{
					return ((Vector4)(object)a - (Vector4)(object)b).sqrMagnitude <= tolerance * tolerance;
				}
				else
				{
					return EqualityComparer<T>.Default.Equals(a, b);
				}
			}

		}


		/// <summary>
		/// 🧠 UTILIDADES GENERALES
		/// </summary>

		public static class General
		{
			public static void LogIfNull(object obj, string name = "Object")
			{
				if (obj == null)
					Debug.LogWarning($"{name} is null. This may or may not destroy everything.");
			}

			public static void Swap<T>(ref T a, ref T b)
			{
				T temp = a;
				a = b;
				b = temp;
			}
			public static string ListToString<T>(List<T> list)
			{
				string val = "";
				foreach (T item in list)
				{
					val += item.ToString() + "\n";
				}
				return val;
			}
		}

		static Color NewColor(float A, float R, float G, float B)
		{
#if GUINEAPIGDOS
			return ColorFloat.FromArgb(A, R, G, B).ToColor255();
#else
			return new Color(R, G, B, A);
#endif
		}



		/// <summary>
		/// 🎨 FORMATO
		/// </summary>

		public static class Formatting
		{
			public static string FormatWithColor(string text, Color color)
			{
				string hex = ColorExtentions.ToHtmlStringRGB(color);
				return $"<color=#{hex}>{text}</color>";
			}
		}

		public static class UnitConversion
		{
			public static float KmToM(float value)
			{
				return value * 1000;
			}

			public static float MeterToKm(float value)
			{
				return value / 1000;
			}

			public static float KmToMi(float value)
			{
				return (value / 1.60934f);
			}
			public static float MiToKm(float value)
			{
				return (1.60934f * value);
			}
		}

		/// <summary>
		/// 🧭 ESPACIO Y DIRECCIÓN
		/// </summary>

		public static class Spatial
		{
			public static float AngleBetweenVectorsXZ(Vector3 a, Vector3 b)
			{
				a.y = 0;
				b.y = 0;
				return Vector3.Angle(a, b);
			}
		}
		// nuevo:
		/// <summary>
		/// 💾 Tipos Serializables
		/// </summary>
		public static class Serializable
		{
			[Serializable]
			public struct TRIANGLE
			{
				public int[] p; // 3 vertices

				public TRIANGLE(int a, int b, int c)
				{
					p = new int[3];
					p[0] = a;
					p[1] = b;
					p[2] = c;
				}
			}
			[Serializable]
			public class Mesh
			{
				public List<Vector3> Vertices;
				public List<TRIANGLE> Triangles;

				public Mesh(List<Vector3> vertices, List<TRIANGLE> triangulos)
				{
					Vertices = vertices;
					Triangles = triangulos;
				}

				/*     public Mesh(UnityEngine.Mesh mesh)
					 {
						 Triangles = new List<TRIANGLE>();
						 Vertices = mesh.vertices.ToList();
						 List<int> triingulos = new List<int>();

						 foreach (var t in mesh.triangles)
						 {
							 triingulos.Add(t);
							 if (triingulos.Count == 3)
							 {
								 Triangles.Add(new TRIANGLE(triingulos[0], triingulos[1], triingulos[2]));
								 triingulos.Clear();
							 }
						 }
					 }*/

				/*  // ¡Aquí está la magia para convertir a UnityEngine.Mesh!
				  public UnityEngine.Mesh ToUnityMesh()
				  {
					  var unityMesh = new UnityEngine.Mesh();
					  unityMesh.SetVertices(Vertices);

					  var triangleIndices = new List<int>();
					  foreach (var tri in Triangles)
					  {
						  triangleIndices.Add(tri.p[0]);
						  triangleIndices.Add(tri.p[1]);
						  triangleIndices.Add(tri.p[2]);
					  }

					  unityMesh.SetTriangles(triangleIndices, 0);
					  unityMesh.RecalculateNormals();
					  unityMesh.RecalculateBounds();

					  return unityMesh;
				  }
				  public static explicit operator Mesh(UnityEngine.Mesh unityMesh)
				  {
					  return new Mesh(unityMesh);
				  }
				  public static explicit operator UnityEngine.Mesh(Mesh Mesh)
				  {
					  return Mesh.ToUnityMesh();
				  }*/
				public override string ToString()
				{
					return $"vs {StdUtils.General.ListToString(Vertices)}, tris {StdUtils.General.ListToString(Triangles)}";
				}
				public string ToObjString()
				{
					var sb = new System.Text.StringBuilder();

					// Primero, listamos todos los vértices
					foreach (var v in Vertices)
					{
						sb.AppendLine($"v {v.x} {v.y} {v.z}");
					}

					// Ahora los triángulos (OBJ usa índices empezando en 1)
					foreach (var tri in Triangles)
					{
						sb.AppendLine($"f {tri.p[0] + 1} {tri.p[1] + 1} {tri.p[2] + 1}");
					}

					return sb.ToString();
				}
				public static Mesh FromObjString(string objData)
				{
					var vertices = new List<Vector3>();
					var triangles = new List<TRIANGLE>();

					var lines = objData.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

					foreach (var line in lines)
					{
						var trimmed = line.Trim();
						if (trimmed.StartsWith("v "))
						{
							// Línea de vértice: "v x y z"
							var parts = trimmed.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
							if (parts.Length >= 4)
							{
								float x = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
								float y = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
								float z = float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);
								vertices.Add(new Vector3(x, y, z));
							}
						}
						else if (trimmed.StartsWith("f "))
						{
							// Línea de triángulo: "f a b c"
							var parts = trimmed.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
							if (parts.Length >= 4)
							{
								int a = int.Parse(parts[1]) - 1; // OBJ indexa desde 1
								int b = int.Parse(parts[2]) - 1;
								int c = int.Parse(parts[3]) - 1;
								triangles.Add(new TRIANGLE(a, b, c));
							}
						}
					}

					return new Mesh(vertices, triangles);
				}
				public Vector3 CenterMesh()
				{
					if (Vertices == null || Vertices.Count == 0)
						return Vector3.zero; // Si no hay vértices, no movemos nada

					// Paso 1: calcular el centroide
					Vector3 centroid = Vector3.zero;
					foreach (var v in Vertices)
						centroid += v;
					centroid /= Vertices.Count;

					// Paso 2: mover cada vértice para centrar la malla
					for (int i = 0; i < Vertices.Count; i++)
						Vertices[i] -= centroid;

					// Paso 3: devolver el offset que se aplicó
					return centroid;
				}

				/// <summary>
				/// Rota la malla en torno al origen usando ángulos Euler en grados.
				/// </summary>
				/// <param name="eulerAngles">Vector3 con los ángulos en grados (X, Y, Z)</param>
				public void RotateEuler(Vector3 eulerAngles)
				{
					if (Vertices == null || Vertices.Count == 0)
						return;

					// Convertir grados a radianes
					float radX = eulerAngles.x * Mathf.Deg2Rad;
					float radY = eulerAngles.y * Mathf.Deg2Rad;
					float radZ = eulerAngles.z * Mathf.Deg2Rad;

					// Precalcular senos y cosenos
					float cosX = Mathf.Cos(radX);
					float sinX = Mathf.Sin(radX);
					float cosY = Mathf.Cos(radY);
					float sinY = Mathf.Sin(radY);
					float cosZ = Mathf.Cos(radZ);
					float sinZ = Mathf.Sin(radZ);

					for (int i = 0; i < Vertices.Count; i++)
					{
						Vector3 v = Vertices[i];

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

						Vertices[i] = v;
					}
				}


			}
			[Serializable]
			public struct Transform
			{
				public Vector3 Pos;
				public Vector3 Rot;
				public Vector3 Scale;
				public Transform(Vector3 pos, Vector3 rot, Vector3 scale)
				{
					this.Pos = pos;
					this.Rot = rot;
					this.Scale = scale;
				}
				public override readonly string ToString() => $"t: p{Pos} r{Rot} s{Scale}";
				public override int GetHashCode()
				{
					return Pos.GetHashCode() ^ Rot.GetHashCode() ^ Scale.GetHashCode();
				}
				/* public static explicit operator Transform(UnityEngine.Transform t)
				 {
					 return new Transform(t.position, t.rotation.eulerAngles, t.localScale);
				 }*/
				public static bool operator ==(Transform a, Transform b)
				{
					return (a.Pos == b.Pos && a.Rot == b.Rot && a.Scale == b.Scale);
				}
				/* public void ApplyTransformToUnity(ref UnityEngine.Transform B)
				 {
					 B.SetPositionAndRotation(this.Pos, Quaternion.Euler(this.Rot));
					 B.localScale = this.Scale;
				 }*/
				public static bool operator !=(Transform a, Transform b)
				{
					return !(a.Pos == b.Pos && a.Rot == b.Rot && a.Scale == b.Scale);
				}
				public override readonly bool Equals(object obj)
				{
					if (obj is Transform)
					{
						Transform t = (Transform)obj;
						return t == this;
					}
					/* else if (obj is UnityEngine.Transform)
					 {
						 return ((Transform)(UnityEngine.Transform)obj) == this;
					 }*/
					else return false;
				}
			}
		}

		/// <summary>
		/// 🐱 ¿Esto? Este campo La gata Luna lo escribio.
		/// </summary>
		public static int YBHRFCCVNBNNMMMMNVCCU8IRFD8EOUOEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEUEEEEEEEEEEEEEEUUUUUUUUUUUUUUUUU = 42;
	}

	public enum Eje
	{
		X,
		Y,
		Z
	}
	[System.Serializable]
	public struct Vector27
	{
		public float A;
		public float B;
		public float C;
		public float D;
		public float E;
		public float F;
		public float G;
		public float H;
		public float I;
		public float J;
		public float K;
		public float L;
		public float M;
		public float N;
		public float Ñ;
		public float O;
		public float P;
		public float Q;
		public float R;
		public float S;
		public float T;
		public float U;
		public float V;
		public float W;
		public float X;
		public float Y;
		public float Z;

		public Vector27(float a, float b, float c, float d, float e, float f, float g, float h, float i, float j, float k, float l, float m, float n, float ñ, float o, float p, float q, float r, float s, float t, float u, float v, float w, float x, float y, float z)
		{
			A = a;
			B = b;
			C = c;
			D = d;
			E = e;
			F = f;
			G = g;
			H = h;
			I = i;
			J = j;
			K = k;
			L = l;
			M = m;
			N = n;
			Ñ = ñ;
			O = o;
			P = p;
			Q = q;
			R = r;
			S = s;
			T = t;
			U = u;
			V = v;
			W = w;
			X = x;
			Y = y;
			Z = z;
		}
		public static Vector27 operator +(Vector27 lhs, Vector27 rhs)
		{
			return new Vector27(
				lhs.A + rhs.A,
				lhs.B + rhs.B,
				lhs.C + rhs.C,
				lhs.D + rhs.D,
				lhs.E + rhs.E,
				lhs.F + rhs.F,
				lhs.G + rhs.G,
				lhs.H + rhs.H,
				lhs.I + rhs.I,
				lhs.J + rhs.J,
				lhs.K + rhs.K,
				lhs.L + rhs.L,
				lhs.M + rhs.M,
				lhs.N + rhs.N,
				lhs.Ñ + rhs.Ñ, // El operador suma ahora también es bilingüe
				lhs.O + rhs.O,
				lhs.P + rhs.P,
				lhs.Q + rhs.Q,
				lhs.R + rhs.R,
				lhs.S + rhs.S,
				lhs.T + rhs.T,
				lhs.U + rhs.U,
				lhs.V + rhs.V,
				lhs.W + rhs.W,
				lhs.X + rhs.X,
				lhs.Y + rhs.Y,
				lhs.Z + rhs.Z
			);
		}
		public static Vector27 operator -(Vector27 lhs, Vector27 rhs)
		{
			return new Vector27(
				lhs.A - rhs.A,
				lhs.B - rhs.B,
				lhs.C - rhs.C,
				lhs.D - rhs.D,
				lhs.E - rhs.E,
				lhs.F - rhs.F,
				lhs.G - rhs.G,
				lhs.H - rhs.H,
				lhs.I - rhs.I,
				lhs.J - rhs.J,
				lhs.K - rhs.K,
				lhs.L - rhs.L,
				lhs.M - rhs.M,
				lhs.N - rhs.N,
				lhs.Ñ - rhs.Ñ, // El operador suma ahora también es bilingüe
				lhs.O - rhs.O,
				lhs.P - rhs.P,
				lhs.Q - rhs.Q,
				lhs.R - rhs.R,
				lhs.S - rhs.S,
				lhs.T - rhs.T,
				lhs.U - rhs.U,
				lhs.V - rhs.V,
				lhs.W - rhs.W,
				lhs.X - rhs.X,
				lhs.Y - rhs.Y,
				lhs.Z - rhs.Z
			);
		}
		public static Vector27 operator *(Vector27 lhs, Vector27 rhs)
		{
			return new Vector27(
				lhs.A * rhs.A,
				lhs.B * rhs.B,
				lhs.C * rhs.C,
				lhs.D * rhs.D,
				lhs.E * rhs.E,
				lhs.F * rhs.F,
				lhs.G * rhs.G,
				lhs.H * rhs.H,
				lhs.I * rhs.I,
				lhs.J * rhs.J,
				lhs.K * rhs.K,
				lhs.L * rhs.L,
				lhs.M * rhs.M,
				lhs.N * rhs.N,
				lhs.Ñ * rhs.Ñ, // El operador suma ahora también es bilingüe
				lhs.O * rhs.O,
				lhs.P * rhs.P,
				lhs.Q * rhs.Q,
				lhs.R * rhs.R,
				lhs.S * rhs.S,
				lhs.T * rhs.T,
				lhs.U * rhs.U,
				lhs.V * rhs.V,
				lhs.W * rhs.W,
				lhs.X * rhs.X,
				lhs.Y * rhs.Y,
				lhs.Z * rhs.Z
			);
		}
		public static Vector27 operator /(Vector27 lhs, Vector27 rhs)
		{
			return new Vector27(
				lhs.A / rhs.A,
				lhs.B / rhs.B,
				lhs.C / rhs.C,
				lhs.D / rhs.D,
				lhs.E / rhs.E,
				lhs.F / rhs.F,
				lhs.G / rhs.G,
				lhs.H / rhs.H,
				lhs.I / rhs.I,
				lhs.J / rhs.J,
				lhs.K / rhs.K,
				lhs.L / rhs.L,
				lhs.M / rhs.M,
				lhs.N / rhs.N,
				lhs.Ñ / rhs.Ñ, // El operador suma ahora también es bilingüe
				lhs.O / rhs.O,
				lhs.P / rhs.P,
				lhs.Q / rhs.Q,
				lhs.R / rhs.R,
				lhs.S / rhs.S,
				lhs.T / rhs.T,
				lhs.U / rhs.U,
				lhs.V / rhs.V,
				lhs.W / rhs.W,
				lhs.X / rhs.X,
				lhs.Y / rhs.Y,
				lhs.Z / rhs.Z
			);
		}
		public static bool operator ==(Vector27 lhs, Vector27 rhs)
		{
			return lhs.A == rhs.A &&
				   lhs.B == rhs.B &&
				   lhs.C == rhs.C &&
				   lhs.D == rhs.D &&
				   lhs.E == rhs.E &&
				   lhs.F == rhs.F &&
				   lhs.G == rhs.G &&
				   lhs.H == rhs.H &&
				   lhs.I == rhs.I &&
				   lhs.J == rhs.J &&
				   lhs.K == rhs.K &&
				   lhs.L == rhs.L &&
				   lhs.M == rhs.M &&
				   lhs.N == rhs.N &&
				   lhs.Ñ == rhs.Ñ &&
				   lhs.O == rhs.O &&
				   lhs.P == rhs.P &&
				   lhs.Q == rhs.Q &&
				   lhs.R == rhs.R &&
				   lhs.S == rhs.S &&
				   lhs.T == rhs.T &&
				   lhs.U == rhs.U &&
				   lhs.V == rhs.V &&
				   lhs.W == rhs.W &&
				   lhs.X == rhs.X &&
				   lhs.Y == rhs.Y &&
				   lhs.Z == rhs.Z;
		}

		public static bool operator !=(Vector27 lhs, Vector27 rhs)
		{
			return !(lhs == rhs);
		}
		public override bool Equals(object obj)
		{
			if (obj is Vector27)
			{
				return this == (Vector27)obj;
			}
			else
				return false;
		}
		public override int GetHashCode()
		{
			return this.A.GetHashCode() ^
				   this.B.GetHashCode() ^
				   this.C.GetHashCode() ^
				   this.D.GetHashCode() ^
				   this.E.GetHashCode() ^
				   this.F.GetHashCode() ^
				   this.G.GetHashCode() ^
				   this.H.GetHashCode() ^
				   this.I.GetHashCode() ^
				   this.J.GetHashCode() ^
				   this.K.GetHashCode() ^
				   this.L.GetHashCode() ^
				   this.M.GetHashCode() ^
				   this.N.GetHashCode() ^
				   this.Ñ.GetHashCode() ^
				   this.O.GetHashCode() ^
				   this.P.GetHashCode() ^
				   this.Q.GetHashCode() ^
				   this.R.GetHashCode() ^
				   this.S.GetHashCode() ^
				   this.T.GetHashCode() ^
				   this.U.GetHashCode() ^
				   this.V.GetHashCode() ^
				   this.W.GetHashCode() ^
				   this.X.GetHashCode() ^
				   this.Y.GetHashCode() ^
				   this.Z.GetHashCode();
		}
		public override string ToString()
		{
			return "" + this.A +
					   this.B +
					   this.C +
					   this.D +
					   this.E +
					   this.F +
					   this.G +
					   this.H +
					   this.I +
					   this.J +
					   this.K +
					   this.L +
					   this.M +
					   this.N +
					   this.Ñ +
					   this.O +
					   this.P +
					   this.Q +
					   this.R +
					   this.S +
					   this.T +
					   this.U +
					   this.V +
					   this.W +
					   this.X +
					   this.Y +
					   this.Z.GetHashCode();
		}


	}
	namespace Extentions
	{
		public static class Vector3Extensions
		{
			/// <summary>
			/// Multiplicación componente a componente (tipo ensalada de frutas cósmica).
			/// </summary>
			public static Vector3 Multiply3d(this Vector3 a, Vector3 b)
			{
				return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
			}

			/// <summary>
			/// División componente a componente (¡cuidado con los ceros!).
			/// </summary>
			public static Vector3 Divide3d(this Vector3 a, Vector3 b)
			{
				return new Vector3(
					b.x != 0 ? a.x / b.x : 0,
					b.y != 0 ? a.y / b.y : 0,
					b.z != 0 ? a.z / b.z : 0
				);
			}

			/// <summary>
			/// Valor absoluto de cada componente (quita los negativos mala onda).
			/// </summary>
			public static Vector3 Abs3d(this Vector3 a)
			{
				return new Vector3(Mathf.Abs(a.x), Mathf.Abs(a.y), Mathf.Abs(a.z));
			}

			/// <summary>
			/// Promedio de los tres ejes (por si quieres un "valor único" del vector).
			/// </summary>
			public static float Average(this Vector3 a)
			{
				return (a.x + a.y + a.z) / 3f;
			}

			/// <summary>
			/// Te da el vector con componentes invertidas (como voltear una tortilla).
			/// </summary>
			public static Vector3 Invert(this Vector3 a)
			{
				return new Vector3(-a.x, -a.y, -a.z);
			}

			/// <summary>
			/// Pasa el vector a string bonito, ideal para debug galáctico.
			/// </summary>
			public static string ToStringFancy(this Vector3 a, int decimales = 2)
			{
				return $"(X:{a.x.ToString($"F{decimales}")}, " +
					   $"Y:{a.y.ToString($"F{decimales}")}, " +
					   $"Z:{a.z.ToString($"F{decimales}")})";
			}
		}
	}
}
