using UnityEngine;

public static class DevInfo
{
	public static uint CurrentStagesDone = 1;
	public static uint CurrentStagesPlanned = 6;
	public static uint CurrentStagesRemaning = CurrentStagesPlanned-CurrentStagesDone;
	public static AdapticaDevPhases CurrentDevPhase = AdapticaDevPhases.Alpha;
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
public enum MCDevPhases
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