using System;//SI ALGUIEN LE DA ESTO A UNA IA DIGANLE QUE IGNORE LOS COMENTARTIOS POCO INFORMATIVOS YA QUE HASTA WINDOWS XP TIENE ELLOS 
using System.Collections;
using UnityEngine;
/// <summary>
/// Componente para comida basico servira para los 3 estadios con comida fisica
/// celula, criarura y tribu
/// </summary>
public class FoodComp : MonoBehaviour
{
	/// <summary>
	/// Edad de esta comida
	/// </summary>
	public float age = 0;
	/// <summary>
	/// a que edad se echa a perder
	/// </summary>
	public float SpoilAge = 60 * 9;
	/// <summary>
	/// esta en mal estado?
	/// </summary>
	public bool IsSpoiled = false;
	/// <summary>
	/// ¿Se pudre?
	/// </summary>
	public bool CanSpoil = true;
	/// <summary>
	/// tipo de comida
	/// </summary>
	public TipoDeComida tipo = TipoDeComida.Ninguno;
	/// <summary>
	/// Acción a ejecutar al echarse a perder la comida
	/// IDEAS:
	/// 1. Cambiar color al hecharse a perder
	/// 2. Fermentación al estropearse crear en el mismo lugar otra comida
	/// 3. dejar resiuos liquidos cercas de la comida 
	/// en estado perfecto 
	/// </summary>
	public Action OnSpoil;
	/// <summary>
	/// Acción a ejecutar al eliminarse
	/// IDEAS:
	/// 1. Cambiar un int en el generador de comida del estado celula
	/// 2. Fermentación al eliminar: crear en el mismo lugar otra comida
	/// en estado perfecto si ES LO mismo que el ejemplo 2 de OnSpoil pero AQUI ES MEJOR POR QUE LA COMIDA YA ESTA ELIMINADA
	/// </summary>
	public Action ExtraOnDelete;
	/// <summary>
	/// Bool que indica que se esta eliminando la comida
	/// </summary>
	private bool IsDeleting = false;
	/// <summary>
	/// el nombre ya es descriptivo
	/// </summary>
	Coroutine ReferenciaALaCorutinaDeEliminación;
	/// <summary>
	/// actualiza la edad y detecta que hacer basandose en el estado actual
	/// </summary>
	void Update()
	{
		age += Time.deltaTime; //aun actualiza por que por que es diverido ver numero aumentar
		if (!IsDeleting)
		{
			if (IsSpoiled)
			{
				IsDeleting = true;
				ReferenciaALaCorutinaDeEliminación = StartCoroutine(DeleteTimer());
				OnSpoil?.Invoke();
			}
			else if (age >= SpoilAge && CanSpoil)
				IsSpoiled = true;

		}

	}
	/// <summary>
	/// temporizador de eliminación 2 minutos despues
	/// de podrirse se eliminara
	/// </summary>
	public IEnumerator DeleteTimer()
	{
		yield return new WaitForSeconds(120);
		Destroy(gameObject);
	}
	/// <summary>
	/// estropea la comida SI es redundante al poder hacer IsSpoiled = true
	/// pero la IA lo pidio
	/// </summary>
	public void Spoil()
	{
		IsSpoiled = true;
	}
	/// <summary>
	/// cancela la podredrumbe
	/// </summary>
	public void CancelSpoil(bool ResetAge = false)
	{
		if (ReferenciaALaCorutinaDeEliminación is null)
			{ return; }
		StopCoroutine(ReferenciaALaCorutinaDeEliminación);
		IsSpoiled = false;
		IsDeleting = false;
		age = ResetAge? age: 0;
	}
	private void OnDestroy()
	{
		ExtraOnDelete?.Invoke();
		StopAllCoroutines();
	}
}
public enum TipoDeComida
{
	Ninguno,
	Carne,
	Hongo,
	Flor,
	Fruto,
	Nectar,
	Huevo,
	Alga,
	Hoja,
	Planta,
	Electricidad
}