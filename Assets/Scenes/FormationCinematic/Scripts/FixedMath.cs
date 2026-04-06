using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
//esto NO lo uso que recuerde en el juego es solo una prueba que hize
//en unity por que es mas faxcil debugear esto en unity que en
// Cosmos OS por que todos sabemos Lo poco que se puede debugear en Cosmos OS
//y su debugger de por si explota con static 

namespace FixedMath
{
    [Serializable]
    /// <summary>
    /// Antes LongDecimal 
    /// es una clase con mayor precisión que double  y float
    /// </summary>
    public struct Fixed128
    {
        // Parte entera y decimal (hasta 18 dígitos decimales)
        public long Ent { get; set; }
        public long Dec { get; set; }

        // Constructor para inicializar con un número de tipo double
        public Fixed128(double num)
        {
            Ent = (long)num;
            Dec = (long)((Math.Abs(num - Ent)) * 1_000_000_000_000_000_000);
        }
        public override int GetHashCode()
        {
            return this.Ent.GetHashCode() ^ this.Dec.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            // 1. Verificar si el objeto es null
            if (obj == null)
                return false;

            // 2. Verificar si el objeto es del mismo tipo
            if (obj.GetType() != this.GetType())
                return false;

            // 3. Convertir el objeto a Fixed128 y comparar las partes
            Fixed128 other = (Fixed128)obj;
            return this.Ent == other.Ent && this.Dec == other.Dec;
        }

        // Constructor con long para la parte entera y decimal
        public Fixed128(long parteEntera, long parteDecimal)
        {
            Ent = parteEntera;
            Dec = parteDecimal;
        }

        // Método ToString() para obtener la representación del número
        public override string ToString()
        {
            return $"{Ent}.{Dec:D18}";
        }
        public static bool operator >(Fixed128 a, Fixed128 b)
        {
            bool greater = a.Ent > b.Ent;
            if (a.Ent == b.Ent)
            {
                greater = a.Dec > b.Dec;
            }
            return greater;
        }
        public static bool operator <(Fixed128 a, Fixed128 b)
        {
            bool Lower = a.Ent < b.Ent;
            if (a.Ent == b.Ent)
            {
                Lower = a.Dec < b.Dec;
            }
            return Lower;
        }
        public static Fixed128 operator -(Fixed128 a)
        {
            return new Fixed128(-a.Ent, a.Dec);
        }
        public static bool operator >=(Fixed128 a, Fixed128 b)
        {
            return a > b || a == b;
        }
        public static bool operator <=(Fixed128 a, Fixed128 b)
        {
            return a < b || a == b;
        }
        public static bool operator ==(Fixed128 a, Fixed128 b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Fixed128 a, Fixed128 b)
        {
            return !a.Equals(b);
        }

        // Operador suma (+)
        public static Fixed128 operator +(Fixed128 a, Fixed128 b)
        {
            long nuevaParteEntera = a.Ent + b.Ent;
            long nuevaParteDecimal = a.Dec + b.Dec;

            if (nuevaParteDecimal >= 1_000_000_000_000_000_000)
            {
                nuevaParteEntera++;
                nuevaParteDecimal -= 1_000_000_000_000_000_000;
            }

            return new Fixed128(nuevaParteEntera, nuevaParteDecimal);
        }
        public static Fixed128 operator +(Fixed128 a, int bu)
        {
            Fixed128 b = new Fixed128(bu);
            long nuevaParteEntera = a.Ent + b.Ent;
            long nuevaParteDecimal = a.Dec + b.Dec;

            if (nuevaParteDecimal >= 1_000_000_000_000_000_000)
            {
                nuevaParteEntera++;
                nuevaParteDecimal -= 1_000_000_000_000_000_000;
            }

            return new Fixed128(nuevaParteEntera, nuevaParteDecimal);
        }
        public Fixed128 Abs()
        {
            long nuevaParteEntera = Ent < 0 ? -Ent : Ent;
            long nuevaParteDecimal = Dec < 0 ? -Dec : Dec;
            return new Fixed128(nuevaParteEntera, nuevaParteDecimal);
        }


        // Operador resta (-)
        public static Fixed128 operator -(Fixed128 a, Fixed128 b)
        {
            long nuevaParteEntera = a.Ent - b.Ent;
            long nuevaParteDecimal = a.Dec - b.Dec;

            if (nuevaParteDecimal < 0)
            {
                nuevaParteEntera--;
                nuevaParteDecimal += 1_000_000_000_000_000_000;
            }

            return new Fixed128(nuevaParteEntera, nuevaParteDecimal);
        }
        public static Fixed128 operator -(Fixed128 a, int bu)
        {
            Fixed128 b = new Fixed128(bu);
            long nuevaParteEntera = a.Ent - b.Ent;
            long nuevaParteDecimal = a.Dec - b.Dec;

            if (nuevaParteDecimal < 0)
            {
                nuevaParteEntera--;
                nuevaParteDecimal += 1_000_000_000_000_000_000;
            }

            return new Fixed128(nuevaParteEntera, nuevaParteDecimal);
        }
        private const long ScaleFactor = 1000000000000000000; // Factor de escala (10^18)
                                                              // Operador multiplicación (*)
        public static Fixed128 operator *(Fixed128 a, Fixed128 b)
        {
            long nuevaParteEntera = a.Ent * b.Ent;
            long nuevaParteDecimal = (a.Dec * b.Dec) / 1_000_000_000_000_000_000;

            nuevaParteEntera += (a.Ent * b.Dec + a.Dec * b.Ent) / 1_000_000_000_000_000_000;

            return new Fixed128(nuevaParteEntera, nuevaParteDecimal);
        }
        public static Fixed128 operator *(Fixed128 a, int bu)
        {
            Fixed128 b = new Fixed128(bu);
            long nuevaParteEntera = a.Ent * b.Ent;
            long nuevaParteDecimal = (a.Dec * b.Dec) / 1_000_000_000_000_000_000;

            nuevaParteEntera += (a.Ent * b.Dec + a.Dec * b.Ent) / 1_000_000_000_000_000_000;

            return new Fixed128(nuevaParteEntera, nuevaParteDecimal);
        }

        // Operador división (/)
        public static Fixed128 operator /(Fixed128 a, Fixed128 b)
        {
            // Asegúrate de que el divisor no sea cero
            if (b.Ent == 0 && b.Dec == 0)
            {
                throw new DivideByZeroException("No se puede dividir entre cero.");
            }

            // Escala ambos números
            long aScaled = a.Ent * ScaleFactor + a.Dec;
            long bScaled = b.Ent * ScaleFactor + b.Dec;

            // Realiza la división a nivel de enteros largos
            long resultScaled = aScaled * ScaleFactor / bScaled;  // Escala el resultado

            // Calcula la parte entera y la parte decimal
            long resultIntegerPart = resultScaled / ScaleFactor;
            long resultDecimalPart = resultScaled % ScaleFactor;

            // Asegurarse de que el resultado sea positivo si ambos operandos son positivos o negativos
            if (aScaled < 0 && bScaled > 0 || aScaled > 0 && bScaled < 0)
            {
                resultDecimalPart = -resultDecimalPart; // Ajustar el signo si es necesario
            }

            // Redondear la parte decimal
            if (resultDecimalPart < 0)
            {
                resultDecimalPart += ScaleFactor; // Ajuste de redondeo
                resultIntegerPart -= 1; // Ajuste de la parte entera si es necesario
            }

            return new Fixed128(resultIntegerPart, resultDecimalPart);
        }

        // Operador módulo (%)
        public static Fixed128 operator %(Fixed128 a, int bb)
        {
            Fixed128 b = new Fixed128(bb, 0);
            // Asegúrate de que el divisor no sea cero
            if (b.Ent == 0 && b.Dec == 0)
            {
                throw new DivideByZeroException("No se puede calcular el módulo entre cero.");
            }

            // Escala ambos números
            long aScaled = a.Ent * ScaleFactor + a.Dec;
            long bScaled = b.Ent * ScaleFactor + b.Dec;

            // Calcula el residuo usando aritmética de enteros largos
            long remainderScaled = aScaled % bScaled;

            // Calcula la parte entera y la parte decimal
            long resultIntegerPart = remainderScaled / ScaleFactor;
            long resultDecimalPart = remainderScaled % ScaleFactor;

            // Ajuste de signo
            if (remainderScaled < 0)
            {
                resultDecimalPart += ScaleFactor;
                resultIntegerPart -= 1;
            }

            return new Fixed128(resultIntegerPart, resultDecimalPart);
        }
        // Operador módulo (%)
        public static Fixed128 operator %(Fixed128 a, Fixed128 b)
        {
            // Asegúrate de que el divisor no sea cero
            if (b.Ent == 0 && b.Dec == 0)
            {
                throw new DivideByZeroException("No se puede calcular el módulo entre cero.");
            }

            // Escala ambos números
            long aScaled = a.Ent * ScaleFactor + a.Dec;
            long bScaled = b.Ent * ScaleFactor + b.Dec;

            // Calcula el residuo usando aritmética de enteros largos
            long remainderScaled = aScaled % bScaled;

            // Calcula la parte entera y la parte decimal
            long resultIntegerPart = remainderScaled / ScaleFactor;
            long resultDecimalPart = remainderScaled % ScaleFactor;

            // Ajuste de signo
            if (remainderScaled < 0)
            {
                resultDecimalPart += ScaleFactor;
                resultIntegerPart -= 1;
            }

            return new Fixed128(resultIntegerPart, resultDecimalPart);
        }
        public static implicit operator Fixed128(int a)
        {
            return new Fixed128(a, 0);
        }
        public static implicit operator Fixed128(long a)
        {
            return new Fixed128(a, 0);
        }
        public static explicit operator Fixed128(double a)
        {
            return new Fixed128(a);
        }
        public static explicit operator int(Fixed128 a)
        {
            return (int)a.ToDouble();
        }
        public static explicit operator long(Fixed128 a)
        {
            return (long)a.ToDouble();
        }
        // Método para convertir el Fixed128 a un double
        public double ToDouble()
        {
            return Ent + (double)Dec / 1_000_000_000_000_000_000;
        }
        public static Fixed128 Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new FormatException("No puedes convertir aire en Fixed128… todavía.");

            // Separa la parte entera y decimal manualmente
            string[] parts = s.Split('.', ','); // soporta ambos separadores
            long integerPart = long.Parse(parts[0].Trim());

            long decimalPart = 0;
            if (parts.Length > 1)
            {
                // Tomamos solo los primeros 8 dígitos decimales, ajustado a tu ScaleFactor
                string decStr = parts[1].PadRight(8, '0').Substring(0, 8);
                decimalPart = long.Parse(decStr);
            }

            return new Fixed128(integerPart, decimalPart);
        }

        public static bool TryParse(string s, out Fixed128 result)
        {
            try
            {
                result = Parse(s);
                return true;
            }
            catch
            {
                result = default(Fixed128);
                return false;
            }
        }

    }
	/// <summary>
	/// por que no era suficiente con los Naturales y Racionales las matematicas inventaron los numeros complejos para complicarnos la vida aun mas, asi que aqui esta el ComplexFixed128
	/// </summary>
	[Serializable]
    public struct ComplexFixed128
    {
        public Fixed128 Real;//a
		public Fixed128 Imaginary;//i

        public ComplexFixed128(Fixed128 real, Fixed128 imaginary)
        {
            Real = real;
            Imaginary = imaginary;
        }
        public ComplexFixed128(Fixed128 real)
        {
            Real = real;
            Imaginary = 0;
        }

        public override string ToString()
        {
            return $"r: {Real}, i: {Imaginary}";
        }

        // Suma de números complejos
        public static ComplexFixed128 operator +(ComplexFixed128 a, ComplexFixed128 b)
        {
            return new ComplexFixed128(a.Real + b.Real, a.Imaginary + b.Imaginary);
        }
        public static explicit operator ComplexFixed128(Fixed128 a)
        {
            return new(a, 0);
        }

        // Resta de números complejos
        public static ComplexFixed128 operator -(ComplexFixed128 a, ComplexFixed128 b)
        {
            return new ComplexFixed128(a.Real - b.Real, a.Imaginary - b.Imaginary);
        }

        // Multiplicación de números complejos
        public static ComplexFixed128 operator *(ComplexFixed128 a, ComplexFixed128 b)
        {
            Fixed128 realPart = a.Real * b.Real - a.Imaginary * b.Imaginary;
            Fixed128 imaginaryPart = a.Real * b.Imaginary + a.Imaginary * b.Real;
            return new ComplexFixed128(realPart, imaginaryPart);
        }
        public static ComplexFixed128 operator /(ComplexFixed128 a, ComplexFixed128 b)
        {
            Fixed128 denominator = b.Real * b.Real + b.Imaginary * b.Imaginary;
            if (denominator == 0)
                throw new DivideByZeroException("División entre cero en complejos");

            Fixed128 real = (a.Real * b.Real + a.Imaginary * b.Imaginary) / denominator;
            Fixed128 imag = (a.Imaginary * b.Real - a.Real * b.Imaginary) / denominator;
            return new ComplexFixed128(real, imag);
        }
        // Conjugado de un número complejo
        public ComplexFixed128 Conjugate()
        {
            return new ComplexFixed128(Real, -Imaginary);
        }

        // Magnitud aproximada (sqrt(real^2 + imag^2))
        public Fixed128 Magnitude()
        {
            Fixed128 realSquared = Real * Real;// -1*-1
            Fixed128 imagSquared = Imaginary * Imaginary;
            Debug.Log("r " +realSquared.ToString());
            Debug.Log("i " +imagSquared.ToString());
            Fixed128 Sum = realSquared + imagSquared;
            Debug.Log("Sum " + Sum.ToString());
            Fixed128 Root = 0;
            try { Root = Fixed128MathPlus.Root(Sum, 2); }
            catch (Exception e)
            {
                Debug.Log("eeerrr: " +e.ToString());
            }
            return Root;
        }
    }


	//e^(i * pi) = -1
	//i^4 = 1


	/// <summary>
	/// Guarda Un numero racional como fracción
	/// </summary>
	[Serializable]
    public struct Fraccion
    {
        public long Numerador { get; private set; }
        public long Denominador { get; private set; }

        // Constructor: convierte un decimal a una fracción (¡porque a nadie le gustan los decimales!)
        public Fraccion(Fixed128 decimalValue)
        {
            // Vamos a tomar ese pobre decimal flotante y convertirlo a una fracción decente.
            // ¡Porque claramente, los decimales nunca son lo suficientemente buenos para una fracción!
            long decimalPart = decimalValue.Dec;
            long integerPart = decimalValue.Ent;

            // Convertimos a un número entero absoluto para evitar peleas con negativos por ahora
            long commonDenominator = 1_000_000_000_000_000_000; // ¡Ah sí, un número bonito y redondo!

            Numerador = integerPart * commonDenominator + decimalPart;
            Denominador = commonDenominator;

            // Simplificamos la fracción (¡aunque claro, algunos dirían que las fracciones no necesitan ser simples!)
            Simplificar();
        }
        public Fraccion(double fraccion)
            : this(new Fixed128(fraccion))
        { }
        public Fraccion(float fraccion) : this((double)fraccion) { }



        // Constructor: construye una fracción de la forma numerador/denominador
        public Fraccion(long numerador, long denominador)
        {
            if (denominador == 0)
            {
                throw new DivideByZeroException("El denominador no puede ser cero. ¿Te gustaría intentar de nuevo?");
            }

            Numerador = numerador;
            Denominador = denominador;

            Simplificar();
        }

        // Método para simplificar la fracción (¡porque todos queremos un estilo limpio y sin complicaciones!)
        private void Simplificar()
        {
            long gcd = ObtenerMCD(Numerador, Denominador);
            Numerador /= gcd;
            Denominador /= gcd;
        }

        // Método para obtener el Máximo Común Divisor (MCD) de dos números (básicamente para hacerlo todo más pequeño y más elegante)
        private long ObtenerMCD(long a, long b)
        {
            while (b != 0)
            {
                long temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        // Operador suma (+) para fracciones (¡porque sumar fracciones nunca es tan simple!)
        public static Fraccion operator +(Fraccion a, Fraccion b)
        {
            // Sumamos como buenos amigos fraccionados
            long numeradorResultado = a.Numerador * b.Denominador + b.Numerador * a.Denominador;
            long denominadorResultado = a.Denominador * b.Denominador;

            // Y como no nos gusta la complejidad, simplificamos la fracción
            return new Fraccion(numeradorResultado, denominadorResultado);
        }

        // Operador resta (-) para fracciones (sí, a veces necesitamos quitar algo)
        public static Fraccion operator -(Fraccion a, Fraccion b)
        {
            long numeradorResultado = a.Numerador * b.Denominador - b.Numerador * a.Denominador;
            long denominadorResultado = a.Denominador * b.Denominador;

            return new Fraccion(numeradorResultado, denominadorResultado);
        }

        // Operador multiplicación (*) para fracciones (¿por qué no?)
        public static Fraccion operator *(Fraccion a, Fraccion b)
        {
            long numeradorResultado = a.Numerador * b.Numerador;
            long denominadorResultado = a.Denominador * b.Denominador;

            return new Fraccion(numeradorResultado, denominadorResultado);
        }

        // Operador división (/) para fracciones (¡aquí no hay nada que temer, solo multiplicar por el inverso!)
        public static Fraccion operator /(Fraccion a, Fraccion b)
        {
            // No tememos a la división, solo multiplicamos por el inverso
            if (b.Numerador == 0)
            {
                throw new DivideByZeroException("Dividir por cero no te llevará a ningún lado. Lo siento.");
            }

            long numeradorResultado = a.Numerador * b.Denominador;
            long denominadorResultado = a.Denominador * b.Numerador;

            return new Fraccion(numeradorResultado, denominadorResultado);
        }

        // Método ToString() para mostrar la fracción de una manera "elegante"
        public override string ToString()
        {
            return $"{Numerador}/{Denominador}";
        }

        // Método Equals para verificar si dos fracciones son iguales (porque sabemos que las fracciones también quieren ser amadas)
        public override bool Equals(object obj)
        {
            if (obj is Fraccion)
            {
                var otraFraccion = (Fraccion)obj;
                return Numerador == otraFraccion.Numerador && Denominador == otraFraccion.Denominador;
            }
            return false;
        }

        // Método GetHashCode para que las fracciones puedan ser felices en un diccionario
        public override int GetHashCode()
        {
            return Numerador.GetHashCode() ^ Denominador.GetHashCode();
        }

        // Operador == para comparar fracciones
        public static bool operator ==(Fraccion a, Fraccion b)
        {
            return a.Equals(b);
        }

        // Operador != para comparar fracciones
        public static bool operator !=(Fraccion a, Fraccion b)
        {
            return !a.Equals(b);
        }
    }
    /// <summary>
    /// Matemáticas para números complejos con Fixed128
    /// </summary>
    public static class ComplexFixed128MathPlus
    {
        public static ComplexFixed128 Root(ComplexFixed128 z, int n)
        {
            if (n <= 0)
                throw new ArgumentException("El orden de la raíz debe ser un entero positivo.");

            // Ahora usamos el método Magnitude del propio complejo
            Fixed128 r = z.Magnitude();
            Fixed128 theta = Angle(z); // atan2 para el ángulo

            Fixed128 rootMagnitude = Fixed128MathPlus.Root(r, n);
            Fixed128 rootAngle = theta / n;

            Fixed128 realPart = rootMagnitude * Fixed128MathPlus.Cos(rootAngle);
            Fixed128 imagPart = rootMagnitude * Fixed128MathPlus.Sin(rootAngle);

            return new ComplexFixed128(realPart, imagPart);
        }



        // Exponencial de un complejo: exp(a + bi) = exp(a) * (cos(b) + i*sin(b))
        public static ComplexFixed128 Exp(ComplexFixed128 z)
        {
            Fixed128 expReal = Fixed128MathPlus.Exp(z.Real);
            Fixed128 cosImag = Fixed128MathPlus.Cos(z.Imaginary);
            Fixed128 sinImag = Fixed128MathPlus.Sin(z.Imaginary);
            return new ComplexFixed128(expReal * cosImag, expReal * sinImag);
        }

        // Seno de un complejo: sin(a + bi) = sin(a)*cosh(b) + i*cos(a)*sinh(b)
        public static ComplexFixed128 Sin(ComplexFixed128 z)
        {
            Fixed128 sinReal = Fixed128MathPlus.Sin(z.Real);
            Fixed128 cosReal = Fixed128MathPlus.Cos(z.Real);
            Fixed128 sinhImag = Sinh(z.Imaginary);
            Fixed128 coshImag = Cosh(z.Imaginary);

            return new ComplexFixed128(sinReal * coshImag, cosReal * sinhImag);
        }

        // Coseno de un complejo: cos(a + bi) = cos(a)*cosh(b) - i*sin(a)*sinh(b)
        public static ComplexFixed128 Cos(ComplexFixed128 z)
        {
            Fixed128 sinReal = Fixed128MathPlus.Sin(z.Real);
            Fixed128 cosReal = Fixed128MathPlus.Cos(z.Real);
            Fixed128 sinhImag = Sinh(z.Imaginary);
            Fixed128 coshImag = Cosh(z.Imaginary);

            return new ComplexFixed128(cosReal * coshImag, -sinReal * sinhImag);
        }

        // Tangente de un complejo: tan(z) = sin(z) / cos(z)
        public static ComplexFixed128 Tan(ComplexFixed128 z)
        {
            return Sin(z) / Cos(z);
        }

        // Funciones hiperbólicas básicas usando Fixed128
        public static Fixed128 Sinh(Fixed128 x)
        {
            return (Fixed128MathPlus.Exp(x) - Fixed128MathPlus.Exp(-x)) / new Fixed128(2);
        }

        public static Fixed128 Cosh(Fixed128 x)
        {
            return (Fixed128MathPlus.Exp(x) + Fixed128MathPlus.Exp(-x)) / new Fixed128(2);
        }



        // Argumento de un complejo: atan2(b, a)
        // Argumento de z: atan2(b, a)
        public static Fixed128 Angle(ComplexFixed128 z)
        {
            // Usa atan2 para obtener el ángulo correcto en todos los cuadrantes
            double angle = Math.Atan2(z.Imaginary.ToDouble(), z.Real.ToDouble());
            return new Fixed128(angle);
        }

    }
    /// <summary>
    /// Matematicas
    /// </summary>
    public class Fixed128MathPlus //Fixed128Math+
    {
        // Función para calcular la raíz n-ésima usando el método de Newton
        public static Fixed128 Root(Fixed128 x, int n)
        {
            if (x < 0 && n % 2 == 0)
                throw new ArgumentException("No se puede calcular la raíz par de un número negativo.");

            if (n <= 0)
                throw new ArgumentException("El orden de la raíz debe ser un número entero positivo.");

            Fixed128 guess = 1;                  // Aproximación inicial
            Fixed128 tolerance = (Fixed128)0.0000000000001; // Tolerancia
            Fixed128 difference;

            do
            {
                Fixed128 denom = Pow(guess, n - 1);
                if (denom == 0) // evitamos la división por cero
                    denom = tolerance;

                Fixed128 nextGuess = (guess * (n - 1) + x / denom) / n;
                difference = nextGuess - guess;
                guess = nextGuess;
            }
            while (difference.Abs() > tolerance);

            return guess;
        }


        public static Fixed128 Pow(Fixed128 x, int exp)
        {
            Fixed128 result = new(1);
            for (int i = 0; i < exp; i++)
            {
                result *= x;
            }
            return result;
        }

        public static Fixed128 Pow(Fixed128 x, Fixed128 exp)
        {
            int expa = (int)exp.ToDouble();
            Fixed128 result = new(1);
            for (int i = 0; i < expa; i++)
            {
                result *= x;
            }
            return result;
        }

        public static Fixed128 Sin(Fixed128 x)
        {
            Fixed128 result = new(0);
            Fixed128 term;
            int n = 0;
            double threshold = 0.000001; // Umbral más grande

            do
            {
                term = Pow(x, new Fixed128(2 * n + 1)) / Factorial(2 * n + 1);
                if (n % 2 != 0)
                    term = -term;
                result += term;
                n++;

            } while (term.Abs() > new Fixed128(threshold)); // Ajustado el umbral

            return result;
        }

        public static Fixed128 Cos(Fixed128 x)
        {
            Fixed128 result = new(0);
            Fixed128 term;
            int n = 0;
            double threshold = 0.000001; // Umbral más grande

            do
            {
                term = Pow(x, new Fixed128(2 * n)) / Factorial(2 * n);
                if (n % 2 != 0)
                    term = -term;
                result += term;
                n++;

            } while (term.Abs() > new Fixed128(threshold)); // Ajustado el umbral

            return result;
        }



        // Función para calcular la tangente (sin series de Taylor)
        public static Fixed128 Tan(Fixed128 x)
        {
            Fixed128 cosValue = Cos(x);
            if (cosValue == new Fixed128(0)) throw new ArgumentException("No se puede calcular tangente cuando el coseno es 0.");

            return Sin(x) / cosValue;
        }

        // Función para calcular exponencial
        public static Fixed128 Exp(Fixed128 x)
        {
            Fixed128 result = new(1); // x^0 / 0! = 1
            Fixed128 term = new(1);
            int n = 1;
            DateTime dt = DateTime.Now;

            while (term.Abs() > new Fixed128(0.00000000001)) // Hasta que el término sea suficientemente pequeño
            {
                term = term * x / new Fixed128(n); // Calcular el siguiente término
                result += term;
                n++;
                DateTime Dt2 = DateTime.Now;
                if (Dt2 - dt > new TimeSpan(0, 0, 1))
                {
                    Debug.Log("El cálculo de exponencial está tardando más de 1 segundo.");
                    return result;
                }
            }

            return result;
        }

        // Función para calcular el logaritmo natural
        public static Fixed128 Log(Fixed128 x)
        {
            if (x <= new Fixed128(0)) throw new ArgumentException("El logaritmo no está definido para valores menores o iguales a cero.");
            double result = Math.Log(x.ToDouble());

            return new Fixed128(result);
        }

        // Función auxiliar para calcular el factorial (utilizado en las series de Taylor)
        public static Fixed128 Factorial(int n)
        {
            Fixed128 result = new(1);
            for (int i = 1; i <= n; i++)
            {
                result *= new Fixed128(i);
            }
            return result;
        }
    }
    public class TestFixed128
    {
        public void BasicTest()
        {
            Fixed128 a = new Fixed128(1.5);
            Fixed128 b = new Fixed128(-1.5);
            Fixed128 cinco = new Fixed128(5);
            Fixed128 Veinti_cinco = new Fixed128(25);
            Fixed128 c = a + b;
            Fixed128 d = a + a;
            Fixed128 e = b + b;
            Fixed128 f = a * a;
            Fixed128 g = b * b;
            Fixed128 h = a / a;
            Fixed128 i = b / b;
            Fixed128 cv = cinco / Veinti_cinco;
            Fixed128 vc = Veinti_cinco / cinco;
            Debug.Log(c.ToString());
            Debug.Log(d.ToString());
            Debug.Log(e.ToString());
            Debug.Log(f.ToString() + "mult1");
            Debug.Log(g.ToString() + "mult2");
            Debug.Log(h.ToString());
            Debug.Log(i.ToString());
            Debug.Log(cv.ToString());
            Debug.Log(vc.ToString());
        }
        public void FULLTEST()
        {
            Fixed128 a = new Fixed128(8);
            Fixed128 b = new Fixed128(-8);
            Fixed128 c = new Fixed128(2, 4);
            Fixed128 d = new Fixed128(-2, 4);
            Fixed128 e = new Fixed128(2, 999900000000);
            Fixed128 f = new Fixed128(0, 100000000000);
            Fixed128 g = new Fixed128(11, 11);
            Fixed128 h = new Fixed128(-11, 11);
            Fixed128 aMb = a + b;
            Fixed128 bMa = b + a;
            Fixed128 cMMd = c * d;
            Fixed128 dMMc = d * c;
            Fixed128 eMMe = e * e;
            Fixed128 fENg = f / g;
            Fixed128 fMog = f / g;
            Fixed128 eMf = e + f;
            Fixed128 gENh = g / h;
            Fixed128 gMoh = g / h;
            Debug.Log("TEST INITIALICED");
            Debug.Log((a.ToString()) + " +" + b.ToString() + "=" + aMb.ToString());
            Debug.Log((b.ToString()) + "+" + bMa.ToString() + "=" + aMb.ToString());
            Debug.Log(c.ToString() + " * " + d.ToString() + "=" + cMMd.ToString());
            Debug.Log(d.ToString() + "*" + c.ToString() + "=" + dMMc.ToString());
            Debug.Log(e.ToString() + "*" + e.ToString() + "=" + eMMe.ToString());
            Debug.Log(f.ToString() + "/" + g.ToString() + "=" + fENg.ToString());
            Debug.Log(g.ToString() + "/" + h.ToString() + "=" + gENh.ToString());
            Debug.Log(f.ToString() + "%" + g.ToString() + "=" + fMog.ToString());
            Debug.Log(g.ToString() + "%" + h.ToString() + "=" + gMoh.ToString());
            Debug.Log(e.ToString() + "+" + f.ToString() + "=" + eMf.ToString());
            Debug.Log("intentando Hacer Seno Y coseno");
            Fixed128 sdsdsdf = new Fixed128(56, 1);
            Fixed128 hfdggf = Fixed128MathPlus.Sin(sdsdsdf);
            Debug.Log(hfdggf.ToString());
        }
        public void TestTrigonometry()
        {
            Fixed128 Fixed128 = new Fixed128(1, 1);
            Fixed128 Cos = Fixed128MathPlus.Cos(Fixed128);
            Debug.Log("Cos: " + Cos);
            Fixed128 Sin = Fixed128MathPlus.Sin(Fixed128);
            Debug.Log("Sin: " + Sin);
            Fixed128 Tan = Fixed128MathPlus.Tan(Fixed128);
            Debug.Log("Tan: " + Tan);
        }
    }
    public class TestComplexFixed128
    {
        public void BasicTest()
        {
            ComplexFixed128 a = new(new Fixed128(1.5), 1);
            ComplexFixed128 b = new(new Fixed128(-1.5), 1);
            ComplexFixed128 cinco = new(new Fixed128(5), 5);
            ComplexFixed128 Veinti_cinco = new(25, 25);
            ComplexFixed128 c = a + b;
            ComplexFixed128 d = a + a;
            ComplexFixed128 e = b + b;
            ComplexFixed128 f = a * a;
            ComplexFixed128 g = b * b;
            ComplexFixed128 h = a / a;
            ComplexFixed128 i = b / b;
            ComplexFixed128 cv = cinco / Veinti_cinco;
            ComplexFixed128 vc = Veinti_cinco / cinco;
            Debug.Log(c.ToString());
            Debug.Log(d.ToString());
            Debug.Log(e.ToString());
            Debug.Log(f.ToString() + "mult1");
            Debug.Log(g.ToString() + "mult2");
            Debug.Log(h.ToString());
            Debug.Log(i.ToString());
            Debug.Log(cv.ToString());
            Debug.Log(vc.ToString());
        }
        public void FULLTEST()
        {
            ComplexFixed128 a = new(new Fixed128(8), 2);
            ComplexFixed128 b = new(new Fixed128(-8), 2);
            ComplexFixed128 c = new(new Fixed128(2, 4), 0);
            ComplexFixed128 d = (ComplexFixed128)new Fixed128(-2, 4);
            ComplexFixed128 e = (ComplexFixed128)new Fixed128(2, 999900000000);
            ComplexFixed128 f = (ComplexFixed128)new Fixed128(0, 100000000000);
            ComplexFixed128 g = (ComplexFixed128)new Fixed128(11, 11);
            ComplexFixed128 h = (ComplexFixed128)new Fixed128(-11, 11);
            ComplexFixed128 aMb = a + b;
            ComplexFixed128 bMa = b + a;
            ComplexFixed128 cMMd = c * d;
            ComplexFixed128 dMMc = d * c;
            ComplexFixed128 eMMe = e * e;
            ComplexFixed128 fENg = f / g;
            ComplexFixed128 fMog = f / g;
            ComplexFixed128 eMf = e + f;
            ComplexFixed128 gENh = g / h;
            ComplexFixed128 gMoh = g / h;
            Debug.Log("TEST INITIALICED");
            Debug.Log((a.ToString()) + " +" + b.ToString() + "=" + aMb.ToString());
            Debug.Log((b.ToString()) + "+" + bMa.ToString() + "=" + aMb.ToString());
            Debug.Log(c.ToString() + " * " + d.ToString() + "=" + cMMd.ToString());
            Debug.Log(d.ToString() + "*" + c.ToString() + "=" + dMMc.ToString());
            Debug.Log(e.ToString() + "*" + e.ToString() + "=" + eMMe.ToString());
            Debug.Log(f.ToString() + "/" + g.ToString() + "=" + fENg.ToString());
            Debug.Log(g.ToString() + "/" + h.ToString() + "=" + gENh.ToString());
            Debug.Log(f.ToString() + "%" + g.ToString() + "=" + fMog.ToString());
            Debug.Log(g.ToString() + "%" + h.ToString() + "=" + gMoh.ToString());
            Debug.Log(e.ToString() + "+" + f.ToString() + "=" + eMf.ToString());
            Debug.Log("intentando Hacer Seno Y coseno");
            ComplexFixed128 sdsdsdf = (ComplexFixed128)new Fixed128(56, 1);
            ComplexFixed128 hfdggf = ComplexFixed128MathPlus.Sin(sdsdsdf);
            Debug.Log(hfdggf.ToString());
        }
        public void TestTrigonometry()
        {
            ComplexFixed128 Fixed128 = (ComplexFixed128)new Fixed128(100, 15);
            ComplexFixed128 Cos = ComplexFixed128MathPlus.Cos(Fixed128);
            Debug.Log("Cos: " + Cos);
            ComplexFixed128 Sin = ComplexFixed128MathPlus.Sin(Fixed128);
            Debug.Log("Sin: " + Sin);
            try
            {

                ComplexFixed128 Tan = ComplexFixed128MathPlus.Tan(Fixed128);
                Debug.Log("Tan: " + Tan);
            }
            catch
            {

            }

        }
        public void TestRoots()
        {
            ComplexFixed128 i = ComplexFixed128MathPlus.Root(new ComplexFixed128(new Fixed128(-1)), 2);
            Debug.Log("Calculo i como" + i.ToString());
        }
    }

}