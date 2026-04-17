using UnityEngine;
//como puedes ver, esta clase es solo para mostrar información sobre el desarrollo del juego, como en qué fase estamos y cuántas etapas hemos completado. No tiene ninguna funcionalidad real en el juego, pero puede ser útil para los desarrolladores y para mostrar a los jugadores cuánto trabajo se ha hecho y cuánto queda por hacer.
public static class DevInfo
{
	public static uint CurrentStagesDone = 1; //solo tengo la etapa celula D: :(
	public static uint CurrentStagesPlanned = 6;
	public static uint CurrentStagesRemaning = CurrentStagesPlanned-CurrentStagesDone;
	public static AdapticaDevPhases CurrentDevPhase = AdapticaDevPhases.Alpha;
	public static byte Year2Digit = 26; //CAMBIAR CADA AÑO NUEVO
}
public enum AdapticaDevPhases
{
	Alpha,
	Beta,
	PreRealese,
	RealeseCandidate,
	Realese
}
public enum DevPhases
{
	PreAlpha = 0,
	Alpha,
	Beta,
	PreRealese,
	RealeseCandidate,
	RTM,
	Realese
}
public enum MCDevPhases//ni idea por que añadi esta referencia a Minecraft.
{
	PreAlpha = 0,
	Indev,
	Infdev,
	Alpha,
	Beta,
	PreRealese,
	RealeseCandidate,
	Realese
}