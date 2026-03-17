using ModelosDeIdioma;
using StandartUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class SplashText : MonoBehaviour
{
	public float PulseSpeed = 2f;
	public float PulseAmplitude = 0.1f;
	public float ChangeTimeInMinutes = 6f;

	public float timer = 0f;
	public TMPro.TMP_Text text;

	public static List<string> Lines = new List<string>()
	{
		"Hecho en México",
		"Please don't Make Horrors beyond my comprehendsion",
		"Also Try minecraft",
		"Now in HD",
		"Now in 3D",
		$"Now in {DevInfo.CurrentDevPhase.ToString()}",
		$"Now in {Application.platform}",
		"Press F to pay respects",
		"Loading… forever",
		"Now with more bugs",
		"Guaranteed to crash",
		"Made in 2026",
		"Ultra realistic graphics",
		"Playable in VR… maybe",
		"Your computer will love it",
		"Warning: Highly Bad",
		"Now with 100% more triangles",
		"Physics powered by spaghetti",
		"Optimized for dial-up",
		"Supports 42 controllers simultaneously",
		"Now in 8K 3D HD",
		"Microbes",
		$"NOW WITH {DevInfo.CurrentStagesDone} stages",
		"I am an Apple",
		"NOW WITH LESS apple",
		"NOW with less microscopic sotware",
		"The quik brown HTTPS jumps over the lazy Fox",
		"The quik brown Fox jumps over the lazy HTTPS",
		"The quik brown HTTPS jumps over the lazy HTTPS",
		"The quik brown Dog jumps over the lazy HTTPS",
		"The quik brown HTTPS jumps over the lazy Dog",
		"The quik brown Dog jumps over the lazy Fox",
		"The quik brown Fox jumps over the lazy Fox",
		"The quik brown Dog jumps over the lazy Dog",
		"The quik brown Fox jumps over the lazy Dog",// el original
		"El veloz murciélago hindú comía feliz cardillo y kiwi", //el primo español
		"#include <stdio.h> \n int main(){ printf(\"HELLO\"); return 0;}",
		"now in .NET 6.0.7!!",
		"Legal en españa",
		"<color=#FF0000>N</color>" +
							 "<color=#FF7F00>O</color>" +
							 "<color=#FFFF00>W</color> " +
							 "<color=#00FF00>I</color>" +
							 "<color=#0000FF>N</color> " +
							 "<color=#4B0082>C</color>" +
							 "<color=#8B00FF>o</color>" +
							 "<color=#FF1493>l</color>" +
							 "<color=#00FFFF>o</color>" +
							 "<color=#FFD700>u</color>" +
							 "<color=#FF4500>r</color>",
		"404 SPLASH NOT FOUND",
		"Please don't Make horrific Creatures",
		"Also Try BABA is YOU",
		$"Made by [REPLACE IN RUNTIME]",
		"Evolved from nothing",
		"Now with 100% more chaos",
		"Not responsible for mutation side effects",
		"Insert coin to continue",
		"Loading… your patience",
		"Made with duct tape and dreams",
		"Made with hopes and dreams",
		"Your PC may experience existential dread",
		"Now compatible with your fridge",
		"Ultra immersive experience: eyes optional",
		"Powered by coffee",
		"Powered by Tea",
		"Powered by Juice",
		"Powered by Milk",
		"Microbial life included",
		"May contain traces of bugs",
		"May contain traces of prototaxites",
		"Press any key to regret it",
		"Infinite recursion, infinite fun",
		"Now with more triangles than your GPU can handle",
		"Playable in your dreams",
		"Caution: May cause spontaneous evolution",
		"I am still a work in progress",
		$"{DevInfo.CurrentDevPhase} testing since forever",
		"Not optimized for potato PCs",
		"Guaranteed to confuse your parents",
		"Satisfying your inner biologist since 2026",
		"Supports up to 9001 microbes simultaneously",
		"Easter eggs inside",
		"Shh… the microbes are watching",
		"Now with free existential crises",
		"Now with optional tutorial for humans",
		"Press F to mutate",
		"Loading your inevitable disappointment",
		"Made by someone who likes triangles",
		"Error 0xDEADBEEF",
		"Error 0xCAFEBABE",
		"Simulating evolution since 1982",
		"Now with more meaningless statistics",
		"Try shaking your PC for fun",
		"Compatible with your toaster",
		"Contains 0% calories",
		"Your neurons will thank you",
		"Warning: May induce chaos",
		"Now with more random numbers",
		"Press ALT for surprise",
		"Warning: microbes are plotting",
		"Guaranteed to confuse AI",
		"Now with optional sarcasm",
		"Contains tiny universes",
		"Playable upside down",
		"May trigger nostalgia",
		"Not responsible for lost sleep",
		"Evolving slowly, like Monday mornings",
		"Now with subtle rage",
		"Simulates microbe politics",
		"Now compatible with caffeine",
		"Wah",
		"Waaahoooooooo",
		"100% biodegradable humor",
		"Now with quantum triangles",
		"Warning: may self-replicate",
		"Loading… please wait… or not",
		"Now supports imaginary numbers",
		"Your GPU is a hero",
		"Contains secret memes",
		"Now with more bugs than features",
		"May cause sudden enlightenment",
		"Playable in your dreams, literally",
		"Press CTRL+ALT+EVOLVE",
		"Physics powered by dark magic",
		"Guaranteed to crash gracefully",
		"Contains dangerous levels of fun",
		"Now compatible with your cat",
		"May contain parallel universes",
		"May not Recue Princesess",
		"May not beat Big Turtles",
		"Maybe it needs comic SANS",
		"Now with optional disappointment",
		"Supports teleporting microbes",
		"Your coffee cup is jealous",
		"With Less Sodium",
		"Warning: Game may stare back",
		"Now in ultra-super-mega resolution",
		"FULL of NullReferenceException",
		"Contains easter eggs, some edible",
		"Playable in infinite recursion mode",
		"Warning: May induce laughter",
		"I'm not Fast like hedgehog",
		"Now with optional irony",
		"I am NOT a plummer",
		"Supports microbe social interactions",
		$"Stay Inside... if you are existing in {DateTime.Now.Year-2020} years ago", //time travel confiuson
		"Contains hidden memes for devs",
		"Your mouse will evolve",
		"You need <color=#FF0000><b>DETERMINATION</b></color>",
		"Now with extra chaos points",
		"Playable with eyes closed",
		"Contains secret references to Spore",
		"Now with optional existential dread",
		"Warning: May cause nostalgia for triangles",
		"Simulates microbe existential crisis",
		"Now with optional coffee breaks",
		"Mostly Rogue Planets",
		"As seen on TV",
		"Guaranteed to confuse historians",
		"Playable with quantum uncertainty",
		"Loading… but slowly… like evolution",
		"May contain traces of previous versions",
		"Now with optional sarcasm mode",
		"The penguin Ate all food in the land were the Pink Blob lives",
		"Your keyboard is a hero",
		"Your keyboard is a TRUE hero",
		"May Contain Papyrus",
		"May not Contain TwoTailed Foxes ",
		"Warning: May cause sudden mutation",
		"Playable in imaginary dimensions",
		"Contains tiny references to your childhood",
		"I'm NOT AMERICAN, I'm 🇲🇽🇲🇽🇲🇽🇲🇽🇲🇽",
		"Now with extra randomness",
		"Evolving microbe culture since 2026",
		"MITOSIS",
		"MEIOSIS",
		"Dihidrogen Monoxide",
		"H<sub>2</sub>O",
		"Water",
		"H2O",
		"I love Sodium Chloride it makes food taste Good",
		"LA SILABA TONICA ES TI no DA ni CA", //el juego se llama Adaptica y la gente lo pronuncia Ádaptica o Adáptica o Adapticá pero se pronuncia como Adaptíca pero por reglas del español no hay acento en la i
		"Full of Fungal Spores",
		"<color=#00FF00>GREEN</color>",
		"<color=#FF0000>RED</color>",
		"<color=#0000FF>BLUE</color>",
		"<color=#00FFFF>Cyan</color>",
		"<color=#FF00FF>Magenta</color>",
		"<color=#FFFF00>Yellow</color>",
		"<color=#FFFFFF>White</color>",
		"<color=#000000>BLACK</color>",
		"<color=#FF0000>R</color><color=#00FF00>G</color><color=#0000FF>B</color>",
		"<color=#00FFFF>C</color><color=#FF00FF>M</color><color=#FFFF00>Y</color>",
		"<color=#00FFFF>C</color><color=#FF00FF>M</color><color=#FFFF00>Y</color><color=#000000>K</color>",
		"Supercalifragilisticexpialidocious",
		"suoicod­ilaipxe­citsiligarf­ilac­repuS",
		"Supercalifragilisticoespialidoso",
		"osodilaipxeocitsiligarfilacrepuS",
		"NOW With a Not Scrapped Plant Editor", //spore elimino el suyo
		"Dun dun Next station Pantitlan",
	};
	Idioma esp;
	void Start()
	{
		try
		{
			if (Lines.Contains($"Made by [REPLACE IN RUNTIME]"))
			{
				Lines.Remove($"Made by [REPLACE IN RUNTIME]");
				Lines.Add($"Made by {Application.companyName}");
			}
		}
		catch (Exception e)
		{

		}
		text = gameObject.GetComponent<TMPro.TMP_Text>();
		if (text == null)
		{
			enabled = false;
			return;
		}

		DateTime date = DateTime.Now;
		if (false)
		{
			enabled = false;
			return;
		}
		var TTTTTTTTTT = new System.Random(UnityEngine.Random.Range(0, 99999));
		configurarIdioma();
		for (int i = 0; i < 5; i++)
		{
			Lines.Add( esp.GenerarFraseSimple(TTTTTTTTTT));

		}
		for (int i = 0; i < 10; i++)
		{
			Lines.Add( eng.GenerarFraseSimple(TTTTTTTTTT));

		}


		text.text = Lines[UnityEngine.Random.Range(0, Lines.Count)];
		if (isDebug)
		{
			Debug.Log(StdUtils.General.ListToString(Lines));
		}
	}
	Idioma eng;
	public bool isDebug;

	void configurarIdioma()
	{
		esp = new Idioma("Español juguetón");

		eng = new Idioma("English playful");

		// Sustantivos (nouns)
		eng.AgregarLexema("cat", PartOfSpeech.Sustantivo, 500);
		eng.AgregarLexema("dog", PartOfSpeech.Sustantivo, 500);
		eng.AgregarLexema("pizza", PartOfSpeech.Sustantivo, 400);
		eng.AgregarLexema("apple", PartOfSpeech.Sustantivo, 400);
		eng.AgregarLexema("computer", PartOfSpeech.Sustantivo, 300);
		eng.AgregarLexema("banana", PartOfSpeech.Sustantivo, 300);
		eng.AgregarLexema("unicorn", PartOfSpeech.Sustantivo, 200);
		eng.AgregarLexema("potato", PartOfSpeech.Sustantivo, 200);
		eng.AgregarLexema("TV", PartOfSpeech.Sustantivo, 300);

		// Verbos (verbs)
		eng.AgregarLexema("eats", PartOfSpeech.Verbo, 300);
		eng.AgregarLexema("jumps", PartOfSpeech.Verbo, 200);
		eng.AgregarLexema("runs", PartOfSpeech.Verbo, 200);
		eng.AgregarLexema("sings", PartOfSpeech.Verbo, 150);
		eng.AgregarLexema("loves", PartOfSpeech.Verbo, 300);
		eng.AgregarLexema("hates", PartOfSpeech.Verbo, 300);
		eng.AgregarLexema("plays", PartOfSpeech.Verbo, 200);
		eng.AgregarLexema("eats quickly", PartOfSpeech.Verbo, 150);

		// Adjetivos (adjectives)
		eng.AgregarLexema("big", PartOfSpeech.Adjetivo, 300);
		eng.AgregarLexema("small", PartOfSpeech.Adjetivo, 300);
		eng.AgregarLexema("funny", PartOfSpeech.Adjetivo, 200);
		eng.AgregarLexema("angry", PartOfSpeech.Adjetivo, 200);
		eng.AgregarLexema("delicious", PartOfSpeech.Adjetivo, 250);
		eng.AgregarLexema("happy", PartOfSpeech.Adjetivo, 250);
		eng.AgregarLexema("weird", PartOfSpeech.Adjetivo, 200);

		// Pronombres / determinantes
		eng.AgregarLexema("the", PartOfSpeech.Pronombre, 1000);
		eng.AgregarLexema("a", PartOfSpeech.Pronombre, 1000);
		eng.AgregarLexema("my", PartOfSpeech.Pronombre, 800);
		eng.AgregarLexema("your", PartOfSpeech.Pronombre, 800);
		eng.AgregarLexema("his", PartOfSpeech.Pronombre, 500);
		eng.AgregarLexema("her", PartOfSpeech.Pronombre, 500);
		eng.AgregarLexema("she", PartOfSpeech.Determinante, 500);
		eng.AgregarLexema("I", PartOfSpeech.Determinante, 500);
		eng.AgregarLexema("he", PartOfSpeech.Determinante, 500);
		eng.AgregarLexema("we", PartOfSpeech.Determinante, 500);
		eng.AgregarLexema("you", PartOfSpeech.Determinante, 500);
		eng.AgregarLexema("they", PartOfSpeech.Determinante, 500);

		// Configurar orden de palabras (SVO)
		eng.Ortografia.Order = WordsOrder.SVO;

		// Algunas frases ejemplo
		eng.Corpus.AddFrase("the cat eats");
		eng.Corpus.AddFrase("my dog runs");
		eng.Corpus.AddFrase("a unicorn jumps");
		eng.Corpus.AddFrase("the pizza is delicious");



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
		esp.AgregarLexema("chile", PartOfSpeech.Sustantivo, 600);
		esp.AgregarLexema("guatemala", PartOfSpeech.Sustantivo, 600);
		esp.AgregarLexema("calcetin", PartOfSpeech.Sustantivo, 600);
		esp.AgregarLexema("agua", PartOfSpeech.Sustantivo, 700);
		esp.AgregarLexema("baño", PartOfSpeech.Sustantivo, 800);
		esp.AgregarLexema("Refresco de cola", PartOfSpeech.Sustantivo, 600);
		esp.AgregarLexema("Jugo", PartOfSpeech.Sustantivo, 600);
		esp.AgregarLexema("come", PartOfSpeech.Verbo, 300);
		esp.AgregarLexema("es", PartOfSpeech.Verbo, 300);
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
		esp.AgregarLexema("aturde", PartOfSpeech.Verbo, 180);
		esp.AgregarLexema("toma", PartOfSpeech.Verbo, 300);
		esp.AgregarLexema("donativo", PartOfSpeech.Sustantivo, 300);
		esp.AgregarLexema("opcional", PartOfSpeech.Adjetivo, 300);
		esp.AgregarLexema("odia", PartOfSpeech.Verbo, 300);
		esp.AgregarLexema("tolero", PartOfSpeech.Verbo, 300);
		esp.AgregarLexema("transmite", PartOfSpeech.Verbo, 300);
		esp.AgregarLexema("ama", PartOfSpeech.Verbo, 300);
		esp.AgregarLexema("paga", PartOfSpeech.Verbo, 300);
		esp.AgregarLexema("obligatorio", PartOfSpeech.Adjetivo, 300);
		esp.AgregarLexema("TV", PartOfSpeech.Sustantivo, 600);
		esp.AgregarLexema("Noticias", PartOfSpeech.Sustantivo, 1000);
		esp.Ortografia.Order = WordsOrder.SVO;
		// Agregamos algo al corpus
		esp.Corpus.AddFrase("el gato come");
		esp.Corpus.AddFrase("el perro duerme");
		esp.Corpus.AddFrase("la gata es pequeña");
	}
	//viejo pulso mas bonito pero menos organico
	void Update()
	{
		float pulse = 1f + Mathf.Sin(Time.time * PulseSpeed) * PulseAmplitude; // pulso entre 0.9 y 1.1
		transform.localScale = Vector3.one * pulse;
		timer += Time.deltaTime;
		if (timer > 60f * ChangeTimeInMinutes) 
		{
			text.text = Lines[UnityEngine.Random.Range(0, Lines.Count)]; timer= 0; 
			Debug.Log("NEW MSG " + text.text);
		}
	}
}
