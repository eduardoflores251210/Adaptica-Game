using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[assembly: SuppressMessage("Style", "IDE0090:Usar \"new(...)\"", Justification = "NO ME IMPORTA ", Scope = "module")]
[assembly: SuppressMessage("Style", "IDE0017:Simplificar la inicialización de objetos", Justification = "POR QUE QUIERO HACERLO ASI", Scope = "module")]
[assembly: SuppressMessage("Style", "IDE0063:Use la instrucción \"using\" simple", Justification = "NO ME IMPORTA ESO", Scope = "module")]
[assembly: SuppressMessage("CodeQuality", "IDE0079:Quitar supresión innecesaria", Justification = "TENGO QUE SUPRIMIR", Scope = "module")]
[assembly: SuppressMessage("Style", "IDE0057:Usar el operador de intervalo", Justification = "ES MENOS ENTENDIBLE", Scope = "module")]
[assembly: SuppressMessage("Style", "IDE0074:Usar la asignación compuesta", Justification = "NO SE ENTIENDE MUCHO", Scope = "module")]
[assembly: SuppressMessage("Style", "IDE0180:Utilizar tupla para intercambiar valores", Justification = "NO ES LEGIBLE", Scope = "module")]
[assembly: SuppressMessage("Style", "IDE0054:Usar la asignación compuesta", Justification = "ES DIFICL DE RECORDAR QU HACE", Scope = "module")]
[assembly: SuppressMessage("Style", "IDE0071:Simplificar la interpolación", Justification = "NO SE ENTIENDE BIEN A VECES", Scope = "module")]
[assembly: SuppressMessage("Style", "IDE1006:Estilos de nombres", Justification = "QUE TE IMPORTA ESO", Scope = "module")]
[assembly: SuppressMessage("Style", "IDE0020:Usar coincidencia de patrones", Justification = "NO ES LEGIBLE", Scope = "module")]
[assembly: SuppressMessage("Style", "IDE0251:Convertir el miembro en 'readonly'", Justification = "NO QUIERO", Scope = "module")]
[assembly: SuppressMessage("Style", "IDE0038:Usar coincidencia de patrones", Justification = "NO NO NO", Scope = "module")]
[assembly: SuppressMessage("Style", "IDE0054:Usar la asignación compuesta", Justification = "<pendiente>", Scope = "module")]
[assembly: SuppressMessage("Style", "IDE0044:Agregar modificador de solo lectura", Justification = "<pendiente>", Scope = "module")]

public class ñ
{
	public string a; public string b; public string c;
	public string d; public string e;
	public Soap soap;
}
public class Soap
{
	public string Brand;
	public string Name; public string Description;
	public string Fragance;
	public float Efectiveness;
}