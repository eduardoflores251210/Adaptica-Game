using SerializableTypes;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlanetOrbit : MonoBehaviour
{
    public Transform Planet;
    public Transform Parent;          // Centro de la órbita
    public float Distance = 10f;      // Radio de la órbita
    public float AxialTilt;

    public int WeekLenght;            // Días por semana
    public int MonthLenght;           // Semanas por mes
    public int DaysInYear;

    [Serialize] public HourTime DayLength;      // Duración del día (escalado)
    [Serialize] public HourTime YearLength;     // Duración del año (escalado)
    [Serialize] public DayTime CurrentDay;      // Día actual

    private float leftoverSeconds = 0f;

    private void Start()
    {
        if (Planet == null) Planet = transform;

        // Inicializar YearLength a partir de DayLength y número de días en año
        long totalSeconds = (long)DayLength.TotalSeconds() * DaysInYear;
        YearLength = new HourTime(0, 0, 0);
        YearLength.AddSeconds((int)totalSeconds, false, true, true);

        // Posición inicial del planeta según CurrentDay
        RefreshPositionFromCurrentDayTime();
    }

    void FixedUpdate()
    {
        if (Planet == null) Planet = transform;
        if (Parent == null) return;

        float delta = Time.fixedDeltaTime;

        // -------------------------------
        // 1️⃣ Escalar tiempo real a tiempo planetario
        // -------------------------------
        float targetSecondsRealForADay = 24 * 60f; // 24 minutos reales = 1440 s
        float scaleFactor = (float)DayLength.TotalSeconds() / targetSecondsRealForADay;

        float scaledSeconds = delta * scaleFactor;
        leftoverSeconds += scaledSeconds;

        int secondsToAdd = Mathf.FloorToInt(leftoverSeconds);
        if (secondsToAdd > 0)
        {
            // Sumamos los segundos al CurrentDay
            CurrentDay.AddSeconds(secondsToAdd);
            leftoverSeconds -= secondsToAdd;
        }

        // -------------------------------
        // 2️⃣ Rotación diaria
        // -------------------------------
        long daySeconds = CurrentDay.Hour * 3600 + CurrentDay.Minute * 60 + CurrentDay.Second;
        float dayFraction = daySeconds / (float)DayLength.TotalSeconds();
        Planet.localRotation = Quaternion.Euler(AxialTilt, dayFraction * 360f, 0f);

        // -------------------------------
        // 3️⃣ Órbita anual
        // -------------------------------
        long daysPassed = (long)CurrentDay.Month * MonthLenght * WeekLenght
                        + (long)CurrentDay.Week * WeekLenght
                        + (long)CurrentDay.Day;

        long secondsFromYearStart = daysPassed * (long)DayLength.TotalSeconds() + (long)daySeconds;
        double fractionOfYear = (double)secondsFromYearStart / (double)YearLength.TotalSeconds();
        fractionOfYear -= Math.Floor(fractionOfYear);

        float angleDeg = (float)(fractionOfYear * 360.0);
        Vector3 offset = new Vector3(-Distance, 0f, 0f);
        Planet.position = Parent.position + Quaternion.Euler(0f, angleDeg, 0f) * offset;
    }


    /// <summary>
    /// Posiciona el planeta inmediatamente según CurrentDay y hora actual
    /// </summary>
    public void RefreshPositionFromCurrentDayTime()
    {
        if (Planet == null) Planet = transform;
        if (Parent == null) return;

        // Rotación diaria
        float daySeconds = CurrentDay.Hour * 3600 + CurrentDay.Minute * 60 + CurrentDay.Second;
        float dayFraction = daySeconds / (float)DayLength.TotalSeconds();
        Planet.localRotation = Quaternion.Euler(AxialTilt, dayFraction * 360f, 0f);

        // Órbita anual
        long daysPassed = (long)CurrentDay.Month * MonthLenght * WeekLenght
                        + (long)CurrentDay.Week * WeekLenght
                        + (long)CurrentDay.Day;

        long secondsFromYearStart = daysPassed * (long)DayLength.TotalSeconds() + (long)daySeconds;
        double fractionOfYear = (double)secondsFromYearStart / (double)YearLength.TotalSeconds();
        fractionOfYear -= Math.Floor(fractionOfYear);

        float angleDeg = (float)(fractionOfYear * 360.0);
        Vector3 offset = new Vector3(-Distance, 0f, 0f);
        Planet.position = Parent.position + Quaternion.Euler(0f, angleDeg, 0f) * offset;
    }

    /// <summary>
    /// Fija fecha y hora simultáneamente y actualiza posición
    /// </summary>
    public void SetCurrentDayAndTime(DayTime dayTime, bool useCopy = true)
    {
        if (dayTime == null) return;
        CurrentDay = useCopy ? new DayTime(dayTime) : dayTime;
        RefreshPositionFromCurrentDayTime();
    }
}
