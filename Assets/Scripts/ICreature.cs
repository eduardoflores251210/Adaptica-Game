using UnityEngine;

namespace SerializableTypes.Biology
{
	/// <summary>
	/// Interfaz mínima para tipos de criatura (microbios, animales, plantas, ...)
	/// Provee operaciones comunes que permiten tratar distintos tipos de criatura de forma uniforme.
	/// </summary>
	public interface ICreature
	{
		/// <summary>Centra la criatura (malla, segmentos y demás subcomponentes relacionados).</summary>
		void CenterCreature();

		/// <summary>Rota la criatura usando ángulos Euler en grados.</summary>
		/// <param name="eulerAngles">Vector3 con los ángulos en grados (X, Y, Z)</param>
		void RotateEuler(Vector3 eulerAngles);
	}
}
