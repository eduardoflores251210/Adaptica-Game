using FixedMath;
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
using Random = UnityEngine.Random;
using Transform = StandartUtilities.StdUtils.Serializable.Transform; //basicamente son 3 vector 3 Pos Rot y Scale
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;
//ni me molestare en explicar estos usings si quieres ver la descripcion revisa SerializanleBiology y SerializablePlanets

//esto es un Namespace de pruebas por que queria añadir idiomas al juego estaba en utils.cs antes A y no esto es obsoleto por que ya no añadire idiomas... Asi que ahora lo uso como generador de frases graciosas como Montaña come Río
namespace ModelosDeIdioma
{
	[Serializable]
	public enum PartOfSpeech { Sustantivo, Verbo, Adjetivo, Determinante, Adverbio, Pronombre, Preposicion, Conjuncion, Interjeccion, Otro }

	// ----------------------
	// Clase principal: Idioma
	// ----------------------
	[Serializable]
	public class Idioma
	{
		public string Nombre { get; set; }
		public Orthography Ortografia { get; set; } = new();
		public Dictionary<int, Lexema> LexicoById { get; set; } = new();
		public Trie LexicoTrie { get; set; } = new();
		public List<ReglaMorfologica> ReglasMorfologicas { get; set; } = new();
		public List<ReglaSintactica> ReglasSintacticas { get; set; } = new();
		public Fonologia Fonologia { get; set; } = new();
		public Corpus Corpus { get; set; } = new();
		public NGramModel NGramModel { get; set; } = new();

		private int nextId = 1; // para asignar IDs a lexemas (evita usar strings por todas partes)

		public Idioma(string nombre)
		{
			Nombre = nombre;
		}

		// Agrega una palabra al léxico y al trie para búsqueda por prefijo
		public Lexema AgregarLexema(string lemma, PartOfSpeech pos, float frecuencia = 1.0f)
		{
			var id = nextId++;
			var lex = new Lexema
			{
				Id = id,
				Lemma = lemma,
				POS = pos,
				Frequency = frecuencia
			};

			LexicoById[id] = lex;
			LexicoTrie.Insert(lemma, id);
			return lex;
		}

		/// <summary>
		/// Busca lexemas cuyo lemma empiece con prefijo
		/// </summary>
		/// <param name="prefijo">prefijo a buscar</param>
		/// <returns></returns>

		public List<Lexema> BuscarPorPrefijo(string prefijo)
		{
			var ids = LexicoTrie.GetWordsByPrefix(prefijo);
			return ids.Select(id => LexicoById[id]).ToList();
		}

		/// <summary>
		/// Escoge una palabra al azar de cierta categoría (ponderada por frecuencia)
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="rng"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public Lexema ElegirPorPOS(PartOfSpeech pos, System.Random rng)
		{
			var candidatos = LexicoById.Values.Where(l => l.POS == pos).ToArray();
			if (candidatos.Length == 0) throw new InvalidOperationException("No hay candidatos para esa POS    " + pos);

			double total = candidatos.Sum(c => c.Frequency);
			double r = rng.NextDouble() * total;
			double acc = 0;
			foreach (var c in candidatos)
			{
				acc += c.Frequency;
				if (r <= acc) return c;
			}
			return candidatos[0];
		}

		// Genera una frase muy simple siguiendo el orden dado
		public string GenerarFraseSimple(System.Random rng)
		{
			WordsOrder order = Ortografia.Order;
			try
			{
				// Elegir palabras por categoría
				string det = "";
				var conPron = rng.NextDouble() < 0.5;
				var DetoSust = rng.NextDouble() < 0.5;
				if (DetoSust)
				{
					det = ElegirPorPOS(PartOfSpeech.Determinante, rng).Lemma;
				}
				else
				{
					det = ElegirPorPOS(PartOfSpeech.Sustantivo, rng).Lemma; //para permitir cosas caoticas como fresa come caballo
				}
				var sust = ElegirPorPOS(PartOfSpeech.Sustantivo, rng).Lemma;
				var verbo = ElegirPorPOS(PartOfSpeech.Verbo, rng).Lemma;
				var conAdj = rng.NextDouble() < 0.5;
				var adj = conAdj ? ElegirPorPOS(PartOfSpeech.Adjetivo, rng).Lemma : "";
				var pron = conPron ? ElegirPorPOS(PartOfSpeech.Pronombre, rng).Lemma : "";
				// Construir frase según el orden
				List<string> elementos = new List<string>();
				if (DetoSust && !string.IsNullOrEmpty(pron))
					pron = "";
				switch (order)
				{
					case WordsOrder.SVO:
						if (!string.IsNullOrEmpty(pron)) elementos.Add(pron);
						elementos.Add(det);          // sujeto
						elementos.Add(verbo);        // verbo
						elementos.Add(sust);         // objeto
						if (!string.IsNullOrEmpty(adj)) elementos.Add(adj); // adjetivo del objeto
						break;

					case WordsOrder.VSO:
						elementos.Add(verbo);
						if (!string.IsNullOrEmpty(pron)) elementos.Add(pron);
						elementos.Add(det);
						elementos.Add(sust);
						if (!string.IsNullOrEmpty(adj)) elementos.Add(adj); // adjetivo del objeto
						break;

					case WordsOrder.SOV:
						if (!string.IsNullOrEmpty(pron)) elementos.Add(pron);
						elementos.Add(det);
						elementos.Add(sust);
						if (!string.IsNullOrEmpty(adj)) elementos.Add(adj); // adjetivo del objeto
						elementos.Add(verbo);
						break;

					case WordsOrder.OSV:
						elementos.Add(sust);
						if (!string.IsNullOrEmpty(adj)) elementos.Add(adj); // adjetivo del objeto
						if (!string.IsNullOrEmpty(pron)) elementos.Add(pron);
						elementos.Add(det);
						elementos.Add(verbo);
						break;

					case WordsOrder.OVS:
						elementos.Add(sust);
						if (!string.IsNullOrEmpty(adj)) elementos.Add(adj); // adjetivo del objeto
						elementos.Add(verbo);
						if (!string.IsNullOrEmpty(pron)) elementos.Add(pron);
						elementos.Add(det);
						break;

					case WordsOrder.VOS:
						elementos.Add(verbo);
						elementos.Add(sust);
						if (!string.IsNullOrEmpty(adj)) elementos.Add(adj); // adjetivo del objeto
						if (!string.IsNullOrEmpty(pron)) elementos.Add(pron);
						elementos.Add(det);
						break;

					default:
						if (!string.IsNullOrEmpty(pron)) elementos.Add(pron);
						elementos.Add(det);
						elementos.Add(sust);
						if (!string.IsNullOrEmpty(adj)) elementos.Add(adj); // adjetivo del objeto
						elementos.Add(verbo);
						break;
				}


				var frase = string.Join(" ", elementos);
				return Ortografia.CapitalizeSentence(frase);
			}
			catch (Exception ex)
			{
				return "(no hay suficientes palabras en el léxico para generar una frase)     " + ex.Message;
			}
		}
	}


	/// <summary>
	/// Lexema y formas
	/// </summary>
	[Serializable]
	public class Lexema
	{
		public int Id { get; set; }
		public string Lemma { get; set; }
		public PartOfSpeech POS { get; set; }
		public Dictionary<string, string> Features { get; set; } = new();
		public List<FormaFlexionada> Formas { get; set; } = new();
		public float Frequency { get; set; } = 1.0f; // simplificado

		public override string ToString() => $"{Lemma} ({POS})";
	}
	/// <summary>
	/// Reglas morfológicas (muy simplificadas)
	/// </summary>
	[Serializable]
	public class ReglaMorfologica
	{
		public string Nombre { get; set; }
		public string PatronSuffix { get; set; } // ej: "ar"
		public string Reemplazo { get; set; }    // ej: "ado"

		// Aplica una transformación muy sencilla sobre un lemma
		public string Aplicar(string lemma)
		{
			if (lemma.EndsWith(PatronSuffix))
			{
				return lemma.Substring(0, lemma.Length - PatronSuffix.Length) + Reemplazo;
			}
			return lemma; // no cambia
		}
	}
	/// <summary>
	/// Reglas sintácticas (ejemplos simbólicos)
	/// </summary>
	[Serializable]
	public class ReglaSintactica
	{
		public string Descripcion { get; set; }

		// Aplicar devuelve una representación textual simplificada
		public string Aplicar(List<Lexema> palabras)
		{
			// Ejemplo tonto: solo concatena lemmas
			return string.Join(" ", palabras.Select(p => p.Lemma));
		}
	}
	/// <summary>
	/// Fonología básica
	/// </summary>
	[Serializable]
	public class Fonema
	{
		public string Symbol { get; set; } // por ejemplo: "a", "k", "ɲ"
		public string Sound { get; set; }
		public string Desc { get; set; }

		public override string ToString() => Symbol;
	}
	[Serializable]
	public class Fonologia
	{
		// Mapeo grapheme -> lista de fonemas posibles (muy simplificado)
		public List<Fonema> G2P { get; set; } = new();

		public List<Fonema> Convertir(string palabra)
		{
			var lista = new List<Fonema>();
			foreach (var ch in palabra.ToLower())
			{
				var key = ch.ToString();
				bool A = false;
				Fonema fonema = null;
				foreach (var f in G2P)
				{
					if (f.Symbol == ch.ToString())
					{
						A = true;
						fonema = f;
						break;
					}
				}
				if (A)
					lista.Add(fonema);
				else lista.Add(new Fonema { Symbol = key, Desc = "indefinido" });
			}
			return lista;
		}
	}
	[Serializable]
	public class FormaFlexionada
	{
		public string Forma { get; set; }
		public string Anotaciones { get; set; }

		public FormaFlexionada(string forma, string anotaciones)
		{
			Forma = forma;
			Anotaciones = anotaciones;
		}
	}
	/// <summary>
	/// Corpus e índices simples
	/// </summary>
	[Serializable]
	public class Corpus
	{
		public List<string> Frases { get; set; } = new();
		// índice invertido: palabra -> posiciones (índice de frase)
		public Dictionary<string, List<int>> InvertedIndex { get; set; } = new(StringComparer.OrdinalIgnoreCase);

		public void AddFrase(string frase)
		{
			var idx = Frases.Count;
			Frases.Add(frase);
			foreach (var token in Tokenize(frase))
			{
				if (!InvertedIndex.ContainsKey(token)) InvertedIndex[token] = new List<int>();
				InvertedIndex[token].Add(idx);
			}
		}

		public IEnumerable<int> BuscarConcordancias(string token)
		{
			if (InvertedIndex.TryGetValue(token, out var listas)) return listas;
			return Enumerable.Empty<int>();
		}

		private IEnumerable<string> Tokenize(string s)
		{
			var clean = new string(s.Where(c => !char.IsPunctuation(c)).ToArray());
			return clean.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(t => t.ToLowerInvariant());
		}
	}
	/// <summary>
	/// Modelo de n-gramos muy, muy básico
	/// </summary>
	[Serializable]
	public class NGramModel
	{
		public Dictionary<string, int> Counts { get; set; } = new();

		public void BuildFromCorpus(Corpus corpus, int n = 2)
		{
			Counts.Clear();
			foreach (var frase in corpus.Frases)
			{
				var tokens = frase.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(t => t.ToLowerInvariant()).ToArray();
				for (int i = 0; i + n <= tokens.Length; i++)
				{
					var key = string.Join(" ", tokens.Skip(i).Take(n));
					Counts[key] = Counts.GetValueOrDefault(key) + 1;
				}
			}
		}

		public int Count(string gram) => Counts.GetValueOrDefault(gram);
	}
	/// <summary>
	/// Nodo de Trie simple para autocompletar / búsqueda por prefijo
	/// </summary>
	[Serializable]
	public class TrieNode
	{
		public Dictionary<char, TrieNode> Children { get; set; } = new();
		public List<int> LexemaIds { get; set; } = new();
	}
	/// <summary>
	/// Trie simple para autocompletar / búsqueda por prefijo
	/// </summary>
	[Serializable]
	public class Trie
	{
		private readonly TrieNode root = new();

		public void Insert(string palabra, int lexemaId)
		{
			var node = root;
			foreach (var ch in palabra)
			{
				if (!node.Children.ContainsKey(ch)) node.Children[ch] = new TrieNode();
				node = node.Children[ch];
			}
			node.LexemaIds.Add(lexemaId);
		}

		public List<int> GetWordsByPrefix(string prefijo)
		{
			var node = root;
			foreach (var ch in prefijo)
			{
				if (!node.Children.ContainsKey(ch)) return new List<int>();
				node = node.Children[ch];
			}
			var results = new List<int>();
			Walk(node, results);
			return results;
		}

		private void Walk(TrieNode node, List<int> outList)
		{
			outList.AddRange(node.LexemaIds);
			foreach (var child in node.Children.Values) Walk(child, outList);
		}
	}
	[Serializable]
	public enum WordsOrder
	{
		None,
		SVO,
		VSO,
		OSV,
		OVS,
		SOV,
		VOS,        //no confundir con el pronombre Vos
	}
	[System.Serializable]
	public enum TiempoVerbal
	{
		// Presente
		Presente,            // yo como
		PresenteContinuo,    // estoy comiendo

		// Pasado
		PretéritoPerfecto,   // he comido
		PretéritoIndefinido, // comí
		PretéritoImperfecto, // comía
		Pluscuamperfecto,    // había comido

		// Futuro
		FuturoSimple,        // comeré
		FuturoPerfecto,      // habré comido

		// Condicional
		CondicionalSimple,   // comería
		CondicionalPerfecto, // habría comido

		// Imperativo
		ImperativoAfirmativo, // ¡come!
		ImperativoNegativo,   // ¡no comas!

		// Subjuntivo
		PresenteSubjuntivo,      // (que) coma
		PretéritoImperfectoSubj, // (que) comiera / comiese
		FuturoSubjuntivo          // (que) comiere (muy literario)
	}
	/// <summary>
	/// Ortografía y utilidades pequeñas
	/// </summary>
	[Serializable]
	public class Orthography
	{
		public WordsOrder Order = WordsOrder.SVO;

		//nuevo
		public Dictionary<TiempoVerbal, string> ConjugationSubfixes;
		public string CapitalizeWord(string w)
		{
			if (string.IsNullOrEmpty(w)) return w;
			return char.ToUpperInvariant(w[0]) + w.Substring(1);
		}
		//nuevo
		public string ConjugarVerbo(string Verbo, TiempoVerbal Tiempo)
		{
			return Verbo + ConjugationSubfixes[Tiempo];
		}
		public string CapitalizeSentence(string s)
		{
			if (string.IsNullOrEmpty(s)) return s;
			var trimmed = s.Trim();
			return CapitalizeWord(trimmed) + (trimmed.EndsWith(".") ? "" : ".");
		}
	}
	public static class FonemaExtentions
	{
		public static bool SoyVocal(this Fonema a)
		{
			return FonemaHelper.EsVocal(a.Sound);
		}
		public static bool SoyConsonante(this Fonema a)
		{
			return FonemaHelper.EsConsonante(a.Sound);
		}
	}
	public static class FonemaHelper
	{
		private static readonly string[] Vocales =
		{ 
		// Cerradas
		"i", "y", "ɨ", "ʉ", "ɯ", "u",
		// Casi cerradas
		"ɪ̟", "ʏ̟", "ɪ", "ʏ", "ɪ̈", "ʊ̈", "ɯ̽", "ʊ", "ɯ̞", "ʊ̠",
		// Semicerradas
		"e", "ø", "ɘ", "ɵ", "ɤ", "o",
		// Medias
		"e̞", "ø̞", "ə", "ɵ̞", "ɤ̞", "o̞",
		// Semiabiertas
		"ɛ", "œ", "ɜ", "ɞ", "ʌ", "ɔ",
		// Casi abiertas
		"æ", "œ̞", "ɐ", "ɞ̞", "ʌ̞", "ɔ̞",
		// Abiertas
		"a", "ɶ", "ä", "ɶ̠", "ɑ", "ɒ"
		};

		public static bool EsVocal(string Sonido)
		{
			return Array.Exists(Vocales, v => v == Sonido);
		}

		public static bool EsConsonante(string Sonido)
		{
			return !EsVocal(Sonido);
		}

		public static string TipoFonema(string Sonido)
		{
			return EsVocal(Sonido) ? "Vocal" : "Consonante";
		}
	}
	//t
	public static class LangGen
	{
		//t, recuqerda t fue la palabra mas rara que genero esta sobrecarga
		public static string GenerarPalabra(List<Fonema> fonemas)
		{
			//t
			if (fonemas == null || fonemas.Count == 0)
				return "";
			///t
			// Separar vocales y consonantes
			List<Fonema> Vocales = new List<Fonema>();
			List<Fonema> Consonantes = new List<Fonema>();

			foreach (Fonema f in fonemas)
			{
				if (f.SoyVocal())
					Vocales.Add(f);
				else if (f.SoyConsonante())
					Consonantes.Add(f);
			}

			// Si no hay vocales o consonantes, devolvemos algo básico
			if (Vocales.Count == 0)
				return Consonantes[Random.Range(0, Consonantes.Count)].Symbol;
			if (Consonantes.Count == 0)
				return Vocales[Random.Range(0, Vocales.Count)].Symbol;

			// Longitud aleatoria entre 1 y 20
			int length = Random.Range(3, 7);

			// Construimos la palabra alternando C y V
			bool empezarConConsonante = Random.Range(0, 2) == 0;
			System.Text.StringBuilder palabra = new System.Text.StringBuilder();

			for (int i = 0; i < length; i++)
			{
				if (empezarConConsonante)
				{
					var cons = Consonantes[Random.Range(0, Consonantes.Count)];
					palabra.Append(cons.Symbol);
				}
				else
				{
					var voc = Vocales[Random.Range(0, Vocales.Count)];
					palabra.Append(voc.Symbol);
				}

				// alternar C ↔ V
				empezarConConsonante = !empezarConConsonante;
			}

			return palabra.ToString();
		}
		//todo lo despues d eeste comentario es nuevo
		public static string GenerarPalabra(List<Fonema> fonemas, int minLength, int maxLength)
		{
			if (fonemas == null || fonemas.Count == 0)
				return "";

			if (minLength < 1) minLength = 1;
			if (maxLength < minLength) maxLength = minLength;

			// Separar vocales y consonantes
			List<Fonema> Vocales = new List<Fonema>();
			List<Fonema> Consonantes = new List<Fonema>();

			foreach (Fonema f in fonemas)
			{
				if (f == null || string.IsNullOrEmpty(f.Symbol)) continue;
				if (f.SoyVocal())
					Vocales.Add(f);
				else
					Consonantes.Add(f);
			}

			// --- Fallback robusto: si falta una de las listas, generamos con la disponible
			if (Vocales.Count == 0 || Consonantes.Count == 0)
			{
				// Fuente de fonemas: vocales si hay, si no consonantes, si no todos
				var fuente = (Vocales.Count > 0) ? Vocales : Consonantes;
				if (fuente.Count == 0) fuente = fonemas.Where(x => x != null && !string.IsNullOrEmpty(x.Symbol)).ToList();

				// Log para debug (quítalo si quieres silencio)
				UnityEngine.Debug.Log($"[LangGen] Fallback usado: Vocales={Vocales.Count}, Consonantes={Consonantes.Count}. Generando con fuente única.");

				int length = UnityEngine.Random.Range(minLength, maxLength + 1);
				var sb = new System.Text.StringBuilder();
				for (int i = 0; i < length; i++)
				{
					var f = fuente[UnityEngine.Random.Range(0, fuente.Count)];
					sb.Append(f.Symbol);
				}
				return sb.ToString();
			}

			// Ahora sí: tenemos ambas listas, generamos alternando y respetando min/max
			int targetLength = UnityEngine.Random.Range(minLength, maxLength + 1);
			bool empezarConConsonante = UnityEngine.Random.Range(0, 2) == 0;
			var palabraBuilder = new System.Text.StringBuilder();

			for (int i = 0; i < targetLength; i++)
			{
				if (empezarConConsonante)
				{
					var cons = Consonantes[UnityEngine.Random.Range(0, Consonantes.Count)];
					palabraBuilder.Append(cons.Symbol);
				}
				else
				{
					var voc = Vocales[UnityEngine.Random.Range(0, Vocales.Count)];
					palabraBuilder.Append(voc.Symbol);
				}
				empezarConConsonante = !empezarConConsonante;
			}

			if (palabraBuilder.Length == 0)
			{
				// Último recurso: devolver algo legible aunque raro
				return fonemas[UnityEngine.Random.Range(0, fonemas.Count)].Symbol;
			}

			return palabraBuilder.ToString();
		}

		public static Fonologia GenerarFonologia(List<BasePart> Creature_anatomy)
		{
			bool HasTooth = false;
			bool HasMandibles = false;
			bool HasTonge = false;
			bool HasLips = false;
			bool HasUvula = false;
			foreach (var part in Creature_anatomy)
			{
				if (part.partID == "0") //boca carnivora basica es como la de hormiga
				{
					HasMandibles = true;
				}
				if (part.partID == "1")//Boca Herbibora inicial, es una boca aspiradora
				{

				}
				if (part.partID == "2") //boca omnivora basica es la herbivora pero con dientes
				{
					HasMandibles = true;
					HasTooth = true;
				}
			}
			List<Fonema> G2P = new List<Fonema>();
			G2P.AddRange(FonemaPorRequisito.RequiereRespiracionBasica);
			if (HasMandibles)
			{
				G2P.AddRange(FonemaPorRequisito.requiereMandibula);
			}
			if (HasTooth && HasMandibles)
			{
				G2P.AddRange(FonemaPorRequisito.requiereDientes);
			}
			if (HasTonge)
			{
				G2P.AddRange(FonemaPorRequisito.requiereLengua);
			}
			if (HasLips && HasMandibles)
			{
				G2P.AddRange(FonemaPorRequisito.requiereLabios);
			}
			if (HasTonge && HasMandibles)
			{
				G2P.AddRange(FonemaPorRequisito.requiereMandibulaYLengua);
			}
			if (HasLips && HasTonge)
			{
				G2P.AddRange(FonemaPorRequisito.requiereLabiosYDientes);
			}
			if (HasUvula)
			{
				G2P.AddRange(FonemaPorRequisito.requiereUvula);
			}
			Fonologia fonologia = new Fonologia()
			{
				G2P = G2P,
			};
			return fonologia;
		}
		public static Orthography GenearOrtografia(Fonologia fonologia)
		{
			WordsOrder order = StdUtils.Randomness.GetRandomEnumValue<WordsOrder>();
			Dictionary<TiempoVerbal, string> Tiempos = new Dictionary<TiempoVerbal, string>();
			foreach (TiempoVerbal f in Enum.GetValues(typeof(TiempoVerbal)))
			{
				Tiempos.Add(f, GenerarPalabra(fonologia.G2P, 3, 5));
			}
			return new Orthography()
			{
				Order = order,
				ConjugationSubfixes = Tiempos
			};
		}
		public static (List<string>, List<PartOfSpeech>) GenerarPalabras(Fonologia fonologia, int PalabrasAdicionales)
		{
			List<string> wa = new();
			List<PartOfSpeech> wb = new();

			// Función local para generar y asegurar que no se repita
			string GenerarUnicaPalabra()
			{
				string palabra;
				int intentos = 0;

				palabra = GenerarPalabra(fonologia.G2P);
				intentos++;
				// Evitamos loop infinito si todo choca (muy improbable)
				bool aaa = UnityEngine.Random.Range(0, 2) == 0;
				bool bbb = UnityEngine.Random.Range(0, 2) == 0;
				var cons = bbb ? "h" : "ʔ͡h";

				if (intentos > 50)

					palabra += aaa ? "a" : cons;



				return palabra;
			}

			// Base fija
			wa.Add(GenerarUnicaPalabra());
			wb.Add(PartOfSpeech.Pronombre);
			wa.Add(GenerarUnicaPalabra());
			wb.Add(PartOfSpeech.Pronombre);
			wa.Add(GenerarUnicaPalabra());
			wb.Add(PartOfSpeech.Pronombre);
			wa.Add(GenerarUnicaPalabra());
			wb.Add(PartOfSpeech.Determinante);
			wa.Add(GenerarUnicaPalabra());
			wb.Add(PartOfSpeech.Determinante);
			wa.Add(GenerarUnicaPalabra());
			wb.Add(PartOfSpeech.Determinante);
			wa.Add(GenerarUnicaPalabra());
			wb.Add(PartOfSpeech.Preposicion);
			wa.Add(GenerarUnicaPalabra());
			wb.Add(PartOfSpeech.Preposicion);
			wa.Add(GenerarUnicaPalabra());
			wb.Add(PartOfSpeech.Interjeccion);
			wa.Add(GenerarUnicaPalabra());
			wb.Add(PartOfSpeech.Interjeccion);
			wa.Add(GenerarUnicaPalabra());
			wb.Add(PartOfSpeech.Adjetivo);

			// ---- Palabras extra: repartir entre categorías especificadas ----
			PartOfSpeech[] extraTipos = new PartOfSpeech[]
			{
				PartOfSpeech.Adverbio,
				PartOfSpeech.Verbo,
				PartOfSpeech.Determinante,
				PartOfSpeech.Adjetivo,
				PartOfSpeech.Sustantivo,
				PartOfSpeech.Conjuncion
			};

			for (int i = 0; i < PalabrasAdicionales; i++)
			{
				PartOfSpeech tipo = extraTipos[i % extraTipos.Length]; // ciclo repartido
				wa.Add(GenerarUnicaPalabra());
				wb.Add(tipo);
			}

			return (wa, wb);
		}

		public static Idioma GenerarIdioma(List<BasePart> Creature_anatomy)
		{
			Fonologia f = GenerarFonologia(Creature_anatomy);
			var o = GenearOrtografia(f);
			string Nombre = GenerarPalabra(f.G2P, 5, 10);

			Idioma idioma = new(Nombre)
			{
				Fonologia = f,
				Ortografia = o,
			};
			int aaa = 0;
			var palabras = GenerarPalabras(f, 9999);

			foreach (var ii in palabras.Item1)
			{
				idioma.AgregarLexema(ii, palabras.Item2[aaa]);
				aaa++;
			}
			System.Random TTTTTTTTTT = new(Random.Range(0, 99999));
			int X = 96;
			for (int i = 0; i < X; i++)
			{
				idioma.Corpus.AddFrase(idioma.GenerarFraseSimple(TTTTTTTTTT));
			}
			idioma.NGramModel.BuildFromCorpus(idioma.Corpus);
			return idioma;


		}

	}
	/// <summary>
	/// Programa de ejemplo
	/// </summary>
	public static class ProgramaEjemplo
	{

		public static void LexicoTest()
		{
			var TTTTTTTTTT = new System.Random(UnityEngine.Random.Range(0, 99999));
			var esp = new Idioma("Español juguetón");

			// Agregamos unas palabras de ejemplo (muy pocas, solo para la demo)
			esp.AgregarLexema("el", PartOfSpeech.Pronombre, 1000);
			esp.AgregarLexema("Ella", PartOfSpeech.Determinante, 1000);
			esp.AgregarLexema("Él", PartOfSpeech.Determinante, 1000);
			esp.AgregarLexema("Tú", PartOfSpeech.Determinante, 1000);
			esp.AgregarLexema("Yo", PartOfSpeech.Determinante, 1000);
			esp.AgregarLexema("la", PartOfSpeech.Pronombre, 900);
			esp.AgregarLexema("gato", PartOfSpeech.Sustantivo, 500);
			esp.AgregarLexema("perro", PartOfSpeech.Sustantivo, 500);
			esp.AgregarLexema("fresa", PartOfSpeech.Sustantivo, 200);
			esp.AgregarLexema("taco", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("pizza", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("hamburgesa", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("piña", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("pistache", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("españa", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("italia", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("francia", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("china", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("chile", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("guatemala", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("calcetin", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("méxico", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("Reino Unido", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("Estados Unidos", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("Estados Hundidos", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("Estados Dormidos", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("Estados Comidos", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("Estados Enojados", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("Estados", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("agua", PartOfSpeech.Sustantivo, 700);
			esp.AgregarLexema("baño", PartOfSpeech.Sustantivo, 800);
			esp.AgregarLexema("Coca-cola", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("Jumex", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("come", PartOfSpeech.Verbo, 300);
			esp.AgregarLexema("anexa", PartOfSpeech.Verbo, 300);
			esp.AgregarLexema("duerme", PartOfSpeech.Verbo, 200);
			esp.AgregarLexema("abraza", PartOfSpeech.Verbo, 200);
			esp.AgregarLexema("pequeño", PartOfSpeech.Adjetivo, 100);
			esp.AgregarLexema("de manzana", PartOfSpeech.Adjetivo, 100);
			esp.AgregarLexema("de alpastor", PartOfSpeech.Adjetivo, 100);
			esp.AgregarLexema("de piña", PartOfSpeech.Adjetivo, 100);
			esp.AgregarLexema("grande", PartOfSpeech.Adjetivo, 80);
			esp.AgregarLexema("interesante", PartOfSpeech.Adjetivo, 80);
			esp.AgregarLexema("Árbol", PartOfSpeech.Sustantivo, 180);
			esp.AgregarLexema("florece", PartOfSpeech.Verbo, 180);
			esp.AgregarLexema("coloniza", PartOfSpeech.Verbo, 180);
			esp.AgregarLexema("aturde", PartOfSpeech.Verbo, 180);
			esp.AgregarLexema("toma", PartOfSpeech.Verbo, 300);
			esp.AgregarLexema("escuela", PartOfSpeech.Sustantivo, 300);
			esp.AgregarLexema("donativo", PartOfSpeech.Sustantivo, 300);
			esp.AgregarLexema("congreso", PartOfSpeech.Sustantivo, 300);
			esp.AgregarLexema("opcional", PartOfSpeech.Adjetivo, 300);
			esp.AgregarLexema("odia", PartOfSpeech.Verbo, 300);
			esp.AgregarLexema("tolero", PartOfSpeech.Verbo, 300);
			esp.AgregarLexema("transmite", PartOfSpeech.Verbo, 300);
			esp.AgregarLexema("ama", PartOfSpeech.Verbo, 300);
			esp.AgregarLexema("paga", PartOfSpeech.Verbo, 300);
			esp.AgregarLexema("obligatorio", PartOfSpeech.Adjetivo, 300);
			esp.AgregarLexema("Brindleshireland's National Public Television", PartOfSpeech.Sustantivo, 300);
			esp.AgregarLexema("Brindleshireland's National Public Radio", PartOfSpeech.Sustantivo, 300);
			esp.AgregarLexema("British Broadcasting Corporation", PartOfSpeech.Sustantivo, 300);
			esp.AgregarLexema("Television ", PartOfSpeech.Sustantivo, 500);
			esp.AgregarLexema("BNPTV", PartOfSpeech.Sustantivo, 300);
			esp.AgregarLexema("BNPR", PartOfSpeech.Sustantivo, 300);
			esp.AgregarLexema("BBC", PartOfSpeech.Sustantivo, 300);
			esp.AgregarLexema("TV", PartOfSpeech.Sustantivo, 600);
			esp.AgregarLexema("Noticias", PartOfSpeech.Sustantivo, 1000);
			esp.Ortografia.Order = WordsOrder.SVO;
			// Agregamos algo al corpus
			esp.Corpus.AddFrase("el gato come");
			esp.Corpus.AddFrase("el perro duerme");
			esp.Corpus.AddFrase("la gata es pequeña");

			// Construir modelo n-grama sencillo
			esp.NGramModel.BuildFromCorpus(esp.Corpus, 2);

			// Demostración: buscar por prefijo
			var pref = "In";
			var encontrados = esp.BuscarPorPrefijo(pref);

			Debug.Log($"Palabras que comienzan con '{pref}': {string.Join(", ", encontrados.Select(e => e.Lemma))}");

			// Generar frases
			for (int i = 0; i < 9000; i++)
			{
				Debug.Log($"Frase aleatoria no{i + 1} " + esp.GenerarFraseSimple(TTTTTTTTTT) + '\r' + '\n');
			}


			// Mostrar algunos conteos n-gram
			Debug.Log($"Count('el gato') = {esp.NGramModel.Count("el gato")}" + '\r' + '\n');
			esp.Fonologia.G2P = new List<Fonema>()
			{ 
				// Fonología demo (muy simplificada)
				new Fonema { Symbol = "g", Desc = "Oclusiva velar sonora", Sound = "g" },
				new Fonema { Symbol = "a", Desc = "Vocal abierta central no redondeada", Sound = "ä" },
				new Fonema { Symbol = "t", Desc = "Oclusiva alveolar sorda", Sound = "t" },
				new Fonema { Symbol = "o", Desc = "Vocal media posterior redondeada", Sound = "o̞" },
				new Fonema { Symbol = "i", Desc = "Vocal cerrada anterior no redondeada", Sound = "i" },
				new Fonema { Symbol = "b", Desc = "Oclusiva bilabial sonora", Sound = "b"},
				new Fonema { Symbol = "e", Desc = "Vocal media anterior no redondeada", Sound = "e̞"},
				new Fonema { Symbol = "u", Desc = "Vocal cerrada posterior redondeada", Sound = "u" },
				
				// Letras adicionales
				new Fonema { Symbol = "p", Desc = "Oclusiva bilabial sorda", Sound = "p" },
				new Fonema {Symbol = "v",  Desc = "Fricativa labiodental sonora", Sound = "v"},
				new Fonema { Symbol = "d", Desc = "Oclusiva dental sonora", Sound = "d" },
				new Fonema { Symbol = "k", Desc = "Oclusiva velar sorda", Sound = "k" },
				new Fonema { Symbol = "f", Desc = "Fricativa labiodental sorda", Sound = "f" },
				new Fonema { Symbol = "s", Desc = "Fricativa alveolar sorda", Sound = "s" },
				new Fonema { Symbol = "x", Desc = "Fricativa velar sorda", Sound = "x" },
				new Fonema { Symbol = "j", Desc = "Fricativa velar sorda", Sound = "x" },
				new Fonema { Symbol = "m", Desc = "Nasal bilabial sonora", Sound = "m" },
				new Fonema { Symbol = "n", Desc = "Nasal alveolar sonora", Sound = "n" },
				new Fonema { Symbol = "ñ", Desc = "Nasales palatal sonora", Sound = "ɲ" },
				new Fonema { Symbol = "l", Desc = "Lateral alveolar sonora", Sound = "l" },
				new Fonema { Symbol = "ll", Desc = "Lateral palatal sonora", Sound = "ʎ" },
				new Fonema { Symbol = "r", Desc = "Vibrante simple alveolar sonora", Sound = "ɾ" },
				new Fonema { Symbol = "rr", Desc = "Vibrante múltiple alveolar sonora", Sound = "r" },
				new Fonema { Symbol = "ch", Desc = "Africada postalveolar sorda", Sound = "tʃ" },
				new Fonema { Symbol = "y", Desc = "Fricativa palatal sonora", Sound = "ʝ" },
				new Fonema { Symbol = "w", Desc = "Aproximante labiovelar sonora", Sound = "w" },
				new Fonema { Symbol = "c", Desc = "Oclusiva velar sorda", Sound  = "k"},
				new Fonema { Symbol = "q", Desc = "Oclusiva velar sorda", Sound  = "k"},
				new Fonema { Symbol = "h", Desc = "Oclusiva glotal", Sound  = "ʔ"},
				new Fonema { Symbol = "z", Desc = "Fricativa alveolar sonora", Sound = "z" },
				new Fonema { Symbol = "th", Desc = "Fricativa dental sonora", Sound = "ð"},
				new Fonema { Symbol = "sh", Desc = "Fricativa postalveolar sorda", Sound = "ʃ"},
				new Fonema { Symbol = "ts", Desc = "Africada alveolar sorda", Sound = "t͡s"},
				new Fonema { Symbol = "≻≺", Desc = "Click de mandíbula carnívora primitiva", Sound = "><" }


			};

			var fon = esp.Fonologia.Convertir("gato");
			var fon2 = esp.Fonologia.Convertir("Ineneen");
			Debug.Log("Fonemas de 'gato': " + string.Join(" ", fon.Select(f => f.Sound)) + '\r' + '\n');
			Debug.Log("Fonemas de 'Ineneen': " + string.Join(" ", fon2.Select(f => f.Sound)) + '\r' + '\n');
			int X = 100;
			Debug.Log($"Generando {X} Palabras" + '\r' + '\n');

			for (int iii = 0; iii < X; iii++)
			{
				Debug.Log($"Palabra no{iii} generada: " + LangGen.GenerarPalabra(esp.Fonologia.G2P) + '\r' + '\n');
			}
			// Fin del ejemplo
			Debug.Log("(fin del ejemplo — ahora tu Idioma puede pedir aumento de memoria)" + '\r' + '\n');
		}
		public static void IdiomaTest(PartsDatabase database)
		{
			// Creamos una anatomía básica (boca omnivora + ojo)
			List<BasePart> aaaa = new List<BasePart>()
			{
				database.GetPartByID("2"),  // Boca omnivora (con dientes)
				database.GetPartByID("-1"), // Ojo random (no influye en fonología, pero queda facha)
			};

			// Generamos un idioma con esa anatomía
			Idioma idioma = LangGen.GenerarIdioma(aaaa);

			// Mostrar nombre del idioma
			UnityEngine.Debug.Log($"Idioma generado: {idioma.Nombre}");
			UnityEngine.Debug.Log("=== Fonologia ===");
			foreach (var f in idioma.Fonologia.G2P)
			{
				Debug.Log($"Sonido Grafema {f.Symbol}, AFI {f.Sound}, Desc {f.Desc}");
			}

			// Mostrar algunas palabras clave (pronombres, sustantivos, verbos, etc.)
			UnityEngine.Debug.Log("=== Diccionario inicial ===");
			int ISa = 0;
			foreach (var palabra in idioma.LexicoById) // Solo 10 primeras pa’ no explotar la consola
			{
				UnityEngine.Debug.Log($"{palabra.Value.Lemma} → {palabra.Value.POS}");
				if (ISa == 9)
					break;
				else ISa++;
			}

			// Generar frases random del corpus
			UnityEngine.Debug.Log("=== Frases de ejemplo ===");
			for (int i = 0; i < 5; i++) // 5 frases para test
			{
				string frase = idioma.GenerarFraseSimple(new System.Random());
				UnityEngine.Debug.Log($"Frase {i + 1}: {frase}");
			}

			// Revisar ortografía
			UnityEngine.Debug.Log("=== Ortografía ===");
			UnityEngine.Debug.Log($"Orden de palabras: {idioma.Ortografia.Order}");
			foreach (var tiempo in idioma.Ortografia.ConjugationSubfixes.Take(3))
			{
				UnityEngine.Debug.Log($"Tiempo {tiempo.Key} → sufijo {tiempo.Value}");
			}

			// Mostrar corpus size
			UnityEngine.Debug.Log($"Corpus total: {idioma.Corpus.Frases.Count} frases.");
		}

	}

	/// <summary>
	/// Listas agrupadas por requisitos anatómicos
	/// </summary>
	public static class FonemaPorRequisito
	{
		/// <summary>
		/// Requiere sólo movimiento de mandíbula / apertura (no dientes, no lengua, no labios)
		/// </summary>
		public static List<Fonema> requiereMandibula = new List<Fonema>()
		{
			new Fonema { Symbol = "≻≺", Desc = "Click de mandíbula (chasquido mecanico mandibular)", Sound = "><" }
		};

		/// <summary>
		/// Requiere dientes (contacto dental / labiodental)
		/// </summary>
		public static List<Fonema> requiereDientes = new List<Fonema>()
		{
			new Fonema { Symbol = "t", Desc = "Oclusiva alveolar/dental sorda", Sound = "t" },
			new Fonema { Symbol = "d", Desc = "Oclusiva dental/ alveolar sonora", Sound = "d" },
			new Fonema { Symbol = "th", Desc = "Fricativa dental sonora ", Sound = "ð" },
			new Fonema { Symbol = "f", Desc = "Fricativa labiodental sorda", Sound = "f" },
			new Fonema { Symbol = "v", Desc = "Fricativa labiodental sonora", Sound = "v" }
		};

		/// <summary>
		/// Requiere lengua (contacto o moldeado por lengua: palatal, alveolar, velar posterior por lengua)
		/// </summary>
		public static List<Fonema> requiereLengua = new List<Fonema>()
		{
			// vocales que requieren configuración de lengua
			new Fonema { Symbol = "i", Desc = "Vocal cerrada anterior ", Sound = "i" },
			new Fonema { Symbol = "e", Desc = "Vocal media anterior", Sound = "e̞" },
			new Fonema { Symbol = "o", Desc = "Vocal media posterior", Sound = "o̞" },
			new Fonema { Symbol = "u", Desc = "Vocal cerrada posterior", Sound = "u" },

			// consonantes con articulación lingual
			new Fonema { Symbol = "s", Desc = "Fricativa alveolar sorda", Sound = "s" },
			new Fonema { Symbol = "sh", Desc = "Fricativa postalveolar sorda", Sound = "ʃ" },
			new Fonema { Symbol = "ch", Desc = "Africada postalveolar sorda", Sound = "tʃ" },
			new Fonema { Symbol = "ts", Desc = "Africada alveolar sorda", Sound = "t͡s" },
			new Fonema { Symbol = "n", Desc = "Nasal alveolar sonora", Sound = "n" },
			new Fonema { Symbol = "l", Desc = "Lateral alveolar sonora", Sound = "l" },
			new Fonema { Symbol = "r", Desc = "Vibrante simple alveolar sonora", Sound = "ɾ" },
			new Fonema { Symbol = "rr", Desc = "Vibrante múltiple alveolar sonora", Sound = "r" },
			new Fonema { Symbol = "ñ", Desc = "Nasal palatal sonora", Sound = "ɲ" },
			new Fonema { Symbol = "ll", Desc = "Lateral palatal sonora", Sound = "ʎ" },
			new Fonema { Symbol = "y", Desc = "Fricativa/ aproximante palatal sonora", Sound = "ʝ" },
			new Fonema { Symbol = "j", Desc = "Fricativa velar", Sound = "x" }, // fricativa velar , (requiere posterior de la lengua)
			new Fonema { Symbol = "x", Desc = "Fricativa velar sorda", Sound = "x" }
		};

		/// <summary>
		/// Requiere labios (bilabiales/labiovelar)
		/// </summary>
		public static List<Fonema> requiereLabios = new List<Fonema>()
		{
			new Fonema { Symbol = "m", Desc = "Nasal bilabial sonora", Sound = "m" },
			new Fonema { Symbol = "b", Desc = "Oclusiva bilabial sonora", Sound = "b" },
			new Fonema { Symbol = "p", Desc = "Oclusiva bilabial sorda", Sound = "p" },
			new Fonema { Symbol = "w", Desc = "Aproximante labiovelar sonora", Sound = "w" },

		};
		/// <summary>
		/// Requiere labios Y dientes (labiodentales)
		/// </summary>
		public static List<Fonema> requiereLabiosYDientes = new List<Fonema>
		{
			// labiodentales (requieren labios y dientes) — aparecen también en requerimiento dientes
			new Fonema { Symbol = "f", Desc = "Fricativa labiodental sorda", Sound = "f" },
			new Fonema { Symbol = "v", Desc = "Fricativa labiodental sonora", Sound = "v" }
		};
		/// <summary>
		/// Requiere RequiereRespiracionBasica (sonidos glotales: fricativa/cierre glotal) y A
		/// </summary>
		public static List<Fonema> RequiereRespiracionBasica = new List<Fonema>()
		{
			new Fonema { Symbol = "a", Desc = "Vocal abierta central", Sound = "ä" }, //asi se permiten sonidos por la boca abiera permanentemente
			new Fonema { Symbol = "h", Desc = "Oclusiva glotal", Sound = "ʔ" }, // oclusiva Glotal
			new Fonema { Symbol = "ʔ͡h", Desc = "Africada glotal sorda", Sound = "ʔ͡h" }
		};

		/// <summary>
		/// Requiere mandíbula Y lengua (ambos: paradas/africadas que necesitan contacto lingual + control de presión)
		/// </summary>
		public static List<Fonema> requiereMandibulaYLengua = new List<Fonema>()
		{
		// Velar/pozicionales donde se usa lengua contra velo + cierre mandibular para oclusión/impulsión
		new Fonema { Symbol = "g", Desc = "Oclusiva velar sonora", Sound = "g" },
		new Fonema { Symbol = "k", Desc = "Oclusiva velar sorda", Sound = "k" },

		// Africadas y oclusivas que requieren lengua precisa y cierre mandibular para generar la explosión/africación
		new Fonema { Symbol = "ch", Desc = "Africada postalveolar sorda", Sound = "tʃ" },
		new Fonema { Symbol = "ts", Desc = "Africada alveolar sorda", Sound = "t͡s" },
		new Fonema { Symbol = "t", Desc = "Oclusiva alveolar sorda", Sound = "t" },
		new Fonema { Symbol = "d", Desc = "Oclusiva dental sonora", Sound = "d" }
		};

		/// <summary>
		/// Requiere úvula / cavidad posterior (si la anatomía incluye uvula)
		/// </summary>
		public static List<Fonema> requiereUvula = new List<Fonema>()
		{
			new Fonema { Symbol = "q", Desc = "Oclusiva uvular sorda ", Sound = "q" }
		};
	}

}