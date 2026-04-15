using SerializableTypes;
using System;

[Serializable]
public class HourTime
{
	public int Hour;
	public int Minute;
	public int Second;
	public HourTime(int value)
	{
		Hour = value;
		Minute = 0;
		Second = 0;
	}
	public HourTime(int H, int Min)
	{
		Hour = H;
		Minute = Min;
		Second = 0;
	}
	public HourTime(int H, int M, int S)
	{
		Hour = H;
		Minute = M;
		Second = S;
	}
	public HourTime(DateTime time)
	{
		Hour = time.Hour;
		Minute = time.Minute;
		Second = time.Second;
	}
	public HourTime(DayTime Copy)
	{

		this.Hour = (int)Copy.Hour;
		this.Minute = (int)Copy.Minute;
		this.Second = (int)Copy.Second;

	}
	public HourTime(HourTime Copy)
	{

		this.Hour = (int)Copy.Hour;
		this.Minute = (int)Copy.Minute;
		this.Second = (int)Copy.Second;

	}
	public string To12Hour()
	{
		if (Hour < 12) return this.ToString() + "AM";
		var v = Hour - 12;
		return $"{new HourTime(v, Minute).ToString()}PM";
	}
	public override string ToString()
	{
		if (Minute < 10)
		{
			return $"{Hour}:0{Minute}";

		}
		return $"{Hour}:{Minute}";
	}
	public override int GetHashCode()
	{
		return Hour ^ Minute;
	}
	public void AddSeconds(int secondsToAdd)
	{
		// Paso 1: sumar los segundos al total actual
		Second += secondsToAdd;

		// Paso 2: normalizar segundos a minutos
		if (Second >= 60 || Second < 0)
		{
			int deltaMinutes = Second / 60;      // cuántos minutos completos hay en los segundos
			Second = Second % 60;                // los segundos "sobrantes"
			if (Second < 0) Second += 60;        // si fue negativo, ajustar a positivo
			Minute += deltaMinutes;              // agregamos los minutos "extra"
		}

		// Paso 3: normalizar minutos a horas
		if (Minute >= 60 || Minute < 0)
		{
			int deltaHours = Minute / 60;        // cuántas horas completas
			Minute = Minute % 60;                // los minutos restantes
			if (Minute < 0) Minute += 60;        // ajustar si es negativo
			Hour += deltaHours;                  // agregamos las horas "extra"
		}

		// Paso 4: normalizar horas a un ciclo de 24h
		if (Hour >= 24 || Hour < 0)
		{
			Hour = Hour % 24;                    // rollover en 24h
			if (Hour < 0) Hour += 24;            // ajustar si es negativo
		}

		// 🎉 Hora final: tu HourTime ya no explota ni viaja en el tiempo
		// Ejemplo divertido: 25:75:80 ya no existe, ahora es 02:16:20
	}
	public void AddSeconds(int secondsToAdd, bool OverFlowHours, bool OverFlowMinutes = true, bool OverFlowSeconds = true)
	{
		// Paso 1: sumar los segundos al total actual
		Second += secondsToAdd;

		if (OverFlowSeconds)
		{
			// Paso 2: normalizar segundos a minutos
			if (Second >= 60 || Second < 0)
			{
				int deltaMinutes = Second / 60;      // cuántos minutos completos hay en los segundos
				Second = Second % 60;                // los segundos "sobrantes"
				if (Second < 0) Second += 60;        // si fue negativo, ajustar a positivo
				Minute += deltaMinutes;              // agregamos los minutos "extra"
			}
		}
		if (OverFlowMinutes)
		{
			// Paso 3: normalizar minutos a horas
			if (Minute >= 60 || Minute < 0)
			{
				int deltaHours = Minute / 60;        // cuántas horas completas
				Minute = Minute % 60;                // los minutos restantes
				if (Minute < 0) Minute += 60;        // ajustar si es negativo
				Hour += deltaHours;                  // agregamos las horas "extra"
			}
		}
		if (OverFlowHours)
		{
			// Paso 4: normalizar horas a un ciclo de 24h
			if (Hour >= 24 || Hour < 0)
			{
				Hour = Hour % 24;                    // rollover en 24h
				if (Hour < 0) Hour += 24;            // ajustar si es negativo
			}
		}

		// 🎉 AddSeconds edición OverFlow:
		// Aquí decides si tu reloj es un ciudadano modelo o un anarquista del tiempo.
		//
		// 🔧 Con OverFlowSeconds/Minutes/Hours en true:
		//   - El tiempo se normaliza como un reloj cuerdo.
		//   - Ejemplo: 25:75:80 → 02:16:20
		//
		// 🚀 Con alguno en false:
		//   - El reloj acumula valores como si fueran puntos de experiencia.
		//   - Ejemplo: (OverFlowHours = false) 25:75:80 → 26:16:20
		//
		// 🤪 Con todos en false:
		//   - Bienvenido al modo “reloj mutante”.
		//   - Ejemplo: 25:75:80 → 25:75:80
		//   - Sí, ahora existen los 80 segundos y nadie puede detenerlos.
		//

	}

	public static bool operator ==(HourTime a, HourTime b)
	{
		if (b is null && a is not null) return false;
		if (b is null && a is null) return true;
		return (a.Hour == b.Hour) && (a.Minute == b.Minute);
	}
	public static bool operator !=(HourTime a, HourTime b)
	{
		return !(a == b);
	}
	public static bool operator ==(HourTime a, DateTime b)
	{
		return (a.Hour == b.Hour) && (a.Minute == b.Minute);
	}
	public static bool operator !=(HourTime a, DateTime b)
	{
		return !(a == b);
	}
	public override bool Equals(object obj)
	{

		if (obj == null) return false;
		if (obj is HourTime)
		{
			return this == (HourTime)obj;
		}
		else if (obj is DateTime dt)
		{
			return (dt.Hour == Hour) && (dt.Minute == Minute);
		}
		else return false;
	}
	public string TimeOfDayString()
	{

		if (Hour >= 5 && Hour < 8) return "Morning";
		else if (Hour < 12 && Hour > 7) return "Day";
		else if (Hour == 12) return "Noon";
		else if (Hour > 12 && Hour < 17) return "Afternoon";
		else if (Hour >= 17 && Hour < 20) return "Evening";
		else if (Hour >= 20 && Hour < 23) return "Night";
		else return "MidNight";
	}
	public TimeOfDay TimeOfDayEnum()
	{
		if (Hour >= 5 && Hour < 8) return TimeOfDay.Morning;
		else if (Hour < 12 && Hour > 7) return TimeOfDay.Day;
		else if (Hour == 12) return TimeOfDay.Noon;
		else if (Hour > 12 && Hour < 17) return TimeOfDay.afternoon;
		else if (Hour >= 17 && Hour < 20) return TimeOfDay.evening;
		else if (Hour >= 20 && Hour < 23) return TimeOfDay.Night;
		else return TimeOfDay.MidNight;
	}


	public long TotalSeconds()
	{
		return Second + (Minute * 60) + (Hour * 60 * 60);
	}
}
[Serializable]
public class DayTime
{
	public long Year;
	public long Month;
	public long Week;
	public long Day;
	public long Hour;
	public long Minute;
	public long Second;
	public long WeekLenght = 7;
	public long MonthLenghtInWeeks = 4;
	public long YearLenghtInMonths = 12;
	public DayTime(long value)
	{
		Year = 0;
		Month = 0;
		Week = 0;
		Day = value;
		Hour = 0;
		Minute = 0;
		Second = 0;
	}


	public DayTime(DateTime time)
	{
		Year = time.Year;
		Month = time.Month;
		Week = time.Day / 4;
		Day = (long)time.DayOfWeek;
		Hour = time.Hour;
		Minute = time.Minute;
		Second = time.Second;
	}

	public DayTime(long year, long month, long week, long day, long hour, long minute, long second) : this(year)
	{
		Month = month;
		Week = week;
		Day = day;
		Hour = hour;
		Minute = minute;
		Second = second;
	}
	public DayTime(long year, long month, long week, long day, long hour, long minute) : this(year)
	{
		Month = month;
		Week = week;
		Day = day;
		Hour = hour;
		Minute = minute;
		Second = 0;
	}
	public DayTime(long year, long month, long week, long day, long hour) : this(year)
	{
		Month = month;
		Week = week;
		Day = day;
		Hour = hour;
		Minute = 0;
		Second = 0;
	}
	public DayTime(long year, long month, long week, long day) : this(year)
	{
		Month = month;
		Week = week;
		Day = day;
		Hour = 0;
		Minute = 0;
		Second = 0;
	}
	public DayTime(DayTime Copy)
	{
		this.Year = Copy.Year;
		this.Month = Copy.Month;
		this.Week = Copy.Week;
		this.Day = Copy.Day;
		this.Hour = Copy.Hour;
		this.Minute = Copy.Minute;
		this.Second = Copy.Second;
		WeekLenght = Copy.WeekLenght;
		MonthLenghtInWeeks = Copy.MonthLenghtInWeeks;
		YearLenghtInMonths = Copy.YearLenghtInMonths;
	}

	public bool IsCompatible(DayTime Other)
	{
		bool MW = this.MonthLenghtInWeeks == Other.MonthLenghtInWeeks;
		bool WD = this.WeekLenght == Other.WeekLenght;
		bool YM = this.YearLenghtInMonths == Other.YearLenghtInMonths;
		return MW && WD && YM;
	}
	public string To12Hour()
	{
		if (Hour < 12) return this.ToString() + "AM";
		var v = Hour - 12;
		return $"{new DayTime(Year, Month, Week, Day, v, Minute).ToString()}PM";
	}
	public override string ToString()
	{
		if (Minute < 10)
		{
			return $"{Year}-{Month}-{Week}-{Day} {Hour}:0{Minute}";

		}
		return $"{Year}-{Month}-{Week}-{Day} {Hour}:{Minute}";
	}
	public override int GetHashCode()
	{
		return Hour.GetHashCode() ^ Minute.GetHashCode();
	}
	public void AddSeconds(long secondsToAdd)
	{
		// Paso 1: sumar los segundos al total actual
		Second += secondsToAdd;

		// Paso 2: normalizar segundos a minutos
		if (Second >= 60 || Second < 0)
		{
			long deltaMinutes = Second / 60;      // cuántos minutos completos hay en los segundos
			Second = Second % 60;                // los segundos "sobrantes"
			if (Second < 0) Second += 60;        // si fue negativo, ajustar a positivo
			Minute += deltaMinutes;              // agregamos los minutos "extra"
		}

		// Paso 3: normalizar minutos a horas
		if (Minute >= 60 || Minute < 0)
		{
			long deltaHours = Minute / 60;        // cuántas horas completas
			Minute = Minute % 60;                // los minutos restantes
			if (Minute < 0) Minute += 60;        // ajustar si es negativo
			Hour += deltaHours;                  // agregamos las horas "extra"
		}

		// Paso 4: normalizar horas a un ciclo de 24h
		if (Hour >= 24 || Hour < 0)
		{
			long DeltaDays = Hour / 24;
			Hour = Hour % 24;
			if (Hour < 0) Hour += 24;
			Day += DeltaDays;
		}
		if (WeekLenght <= 0) WeekLenght = 7;
		// Paso 5 normalizar Dias a una semana
		if (Day >= WeekLenght || Day < 0)
		{
			long DeltaWeeks = Day / WeekLenght;
			Day = Day % WeekLenght;
			if (Day < 0) Day += WeekLenght;
			Week += DeltaWeeks;
		}
		if (MonthLenghtInWeeks <= 0) MonthLenghtInWeeks = 4;
		// Paso 6 Normalizar Semanas a meses
		if (Week >= MonthLenghtInWeeks || Week < 0)
		{
			long DeltaMonths = Week / MonthLenghtInWeeks;
			Week = Week % MonthLenghtInWeeks;
			if (Week < 0) Week += MonthLenghtInWeeks;
			Month += DeltaMonths;
		}
		if (YearLenghtInMonths <= 0) YearLenghtInMonths = 12;
		// Paso 7 Normalizar meses a años
		if (Month >= YearLenghtInMonths || Month < 0)
		{
			long DeltaMonths = Month / YearLenghtInMonths;
			Month = Month % YearLenghtInMonths;
			if (Month < 0) Month += YearLenghtInMonths;
			Year += DeltaMonths;
		}


	}

	public static bool operator ==(DayTime a, DayTime b)
	{
		if (b is null && a is not null) return false;
		if (b is null && a is null) return true;
		return (a.Hour == b.Hour) && (a.Minute == b.Minute)
			&& (a.Day == b.Day) //day equivale a day of the week
			&& (a.Week == b.Week)
			&& (a.Month == b.Month)
			&& (a.Year == b.Year)
			&& (a.WeekLenght == b.WeekLenght)
			&& (a.MonthLenghtInWeeks == b.MonthLenghtInWeeks)
			&& (a.YearLenghtInMonths == b.YearLenghtInMonths);
	}
	public static bool operator !=(DayTime a, DayTime b)
	{
		return !(a == b);
	}
	public static bool operator ==(DayTime a, DateTime b)
	{
		return (a.Hour == b.Hour) && (a.Minute == b.Minute)
			&& (a.Day == (int)b.DayOfWeek) //day equivale a day of the week
			&& (a.Week == b.Day / 7)
			&& (a.Month == b.Month)
			&& (a.Year == b.Year)
			&& (a.WeekLenght == 7)
			&& (a.MonthLenghtInWeeks == 4)
			&& (a.YearLenghtInMonths == 12);
	}
	public static bool operator !=(DayTime a, DateTime b)
	{
		return !(a == b);
	}
	public override bool Equals(object obj)
	{
		return obj switch
		{
			DayTime => this == (DayTime)obj,
			DateTime dt => this == dt,
			_ => false
		};
	}
	public string TimeOfDayString()
	{

		if (Hour >= 5 && Hour < 8) return "Morning";
		else if (Hour < 12 && Hour > 7) return "Day";
		else if (Hour == 12) return "Noon";
		else if (Hour > 12 && Hour < 17) return "Afternoon";
		else if (Hour >= 17 && Hour < 20) return "Evening";
		else if (Hour >= 20 && Hour < 23) return "Night";
		else return "MidNight";
	}
	public TimeOfDay TimeOfDayEnum()
	{
		if (Hour >= 5 && Hour < 8) return TimeOfDay.Morning;
		else if (Hour < 12 && Hour > 7) return TimeOfDay.Day;
		else if (Hour == 12) return TimeOfDay.Noon;
		else if (Hour > 12 && Hour < 17) return TimeOfDay.afternoon;
		else if (Hour >= 17 && Hour < 20) return TimeOfDay.evening;
		else if (Hour >= 20 && Hour < 23) return TimeOfDay.Night;
		else return TimeOfDay.MidNight;
	}

	public double TotalSeconds()
	{
		double secs = 0;
		secs += Year * YearLenghtInMonths * MonthLenghtInWeeks * WeekLenght * 24 * 60 * 60;
		secs += Month * MonthLenghtInWeeks * WeekLenght * 24 * 60 * 60;
		secs += Week * WeekLenght * 24 * 60 * 60;
		secs += Day * 24 * 60 * 60;
		secs += Hour * 3600 + Minute * 60 + Second;
		return secs;
	}
	public long TotalSecondsLong()
	{
		long secs = 0;
		secs += Year * YearLenghtInMonths * MonthLenghtInWeeks * WeekLenght * 24 * 60 * 60;
		secs += Month * MonthLenghtInWeeks * WeekLenght * 24 * 60 * 60;
		secs += Week * WeekLenght * 24 * 60 * 60;
		secs += Day * 24 * 60 * 60;
		secs += Hour * 3600 + Minute * 60 + Second;
		return secs;
	}

}

public enum TimeOfDay
{
	None = -1,
	Morning,
	Day,
	Noon,
	afternoon,
	evening,
	Night,
	MidNight
}
