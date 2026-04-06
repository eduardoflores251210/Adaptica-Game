using ModelosDeIdioma;
using StandartUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
///<summary>
/// asi es HAY SPLASH TEXT, el texto que sale en la pantalla de carga, es un texto random que se muestra para entretener al jugador mientras espera, y a veces tiene mensajes secretos o referencias a cosas, es un clásico de los juegos, y este no es la excepción, aunque a veces puede ser un poco molesto si te toca uno malo, pero bueno, es parte de la experiencia, y si quieres ver todos los mensajes posibles, puedes revisar el código fuente, o esperar a que salgan todos en el juego, quien sabe, tal vez encuentres uno que te guste mucho y quieras compartirlo con tus amigos, o tal vez encuentres uno que te haga reír mucho y quieras guardarlo como tu favorito, o tal vez encuentres uno que te haga sentir identificado y quieras usarlo como tu lema de vida, en fin, el splash text es una parte importante del juego y merece ser apreciado por su creatividad y humor.
///</summary>
public class SplashText : MonoBehaviour
{
	public float PulseSpeed = 2f;
	public float PulseAmplitude = 0.1f;
	public float ChangeTimeInMinutes = 6f;

	public float timer = 0f;
	public TMPro.TMP_Text text;
	[Header("DEBUG")]
	public bool Override = false;
	public int idx = 0;
	/// <summary>
	/// la lista de mensajes que pueden aparecer en el splash text, algunos son referencias a cosas, otros son chistes internos, otros son frases random, y otros son generados por un modelo de lenguaje, la idea es que haya una gran variedad de mensajes para que cada vez que juegues puedas ver algo diferente, y si quieres agregar tus propios mensajes, puedes hacerlo editando esta lista, solo asegúrate de que sean apropiados y divertidos, y tal vez puedas hacer que tu mensaje sea el próximo en aparecer en el juego, así que no dudes en ser creativo y agregar tus propias frases al splash text, ¡quién sabe! tal vez tu mensaje se convierta en un clásico del juego y sea recordado por los jugadores durante años.
	/// </summary>
	public static List<string> Lines = new List<string>()
	{
		"Hecho en México", //si es verdad, soy MEXICANo,   []MEXICANOS AL GRITO DE GUERRA, el acero aprestad... No no me pondre a cantar el himno por que ni soy Patriotico. solo se me ocurio este splahs trext por  que vi la tele y salio un anuncio que decia ¡Hecho en México! mostrando productos mexicanos, y me parecio gracioso, asi que lo puse como splash text, ademas de que es verdad, el juego esta hecho en México, asi que es un buen mensaje para mostrar en el splash text, ademas de que es un buen mensaje para mostrar el orgullo por el trabajo hecho en México, aunque no se si alguien mas lo va a entender o si alguien mas se va a sentir identificado con ese mensaje, pero bueno, es parte de la experiencia y merece ser apreciado por su creatividad y humor.",
		"Please don't Make Horrors beyond my comprehendsion",
		"Also Try minecraft", //clasico por que mine craft pone Also try terraria.
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
		"Powered by coffee", //falso NO tomo cafe, por eso puse el siguiente de chocolate
		"Powered by cocoa",
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
		"Reduced in Sodium",
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
		"Now with optional cocoa breaks",
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
		"Supercalifragilisticoespialidoso", //aunque suene raro y extravagante si lo dices con soltura sonara armonioso. -referencia a la cancion de Mary Poppins, aunque la version original es supercalifragilisticexpialidocious, la version en español es supercalifragilisticoespialidoso, pero como el juego esta en español e ingles, puse ambas versiones, ademas de que me gusta mas la version en español por que suena mas divertida y extravagante, aunque la version en ingles es mas conocida y tiene un significado mas profundo, asi que puse ambas versiones para que cada quien pueda elegir su favorita, ademas de que es una referencia divertida y apropiada para el juego, ya que el juego trata sobre evolucionar y adaptarse a diferentes situaciones, y esa palabra es un ejemplo perfecto de algo que es complicado pero divertido al mismo tiempo. OK el texto predictivo Genera comentarios largiuisimos.
		"osodilaipxeocitsiligarfilacrepuS",
		"NOW With a Not Scrapped Plant Editor", //spore elimino el suyo
		"Dun dun Next station Pantitlan",
		//v3.0
		"Este juego NO es un verbo. No intentes twilightearlo, ni adapticarlo, ni pantitlanearlo.",
		"Now With Less cubes",
		"Keep clicking. The flour isn't going to sift itself.",
		"That delta is full of runes",
		"Button mashing is a valid baking technique.",
		"0% Raytracing, 100% Hope",
		"CAKE.CAKE.CAKE.CAKE.CAKE.CAKE.CAKE.",
		"IS THAT A SPHERE",
		"Your mouse switch has a lifespan. Use it on the CAKE button.",
		"The mitochondrion is the powerhouse of the crash",
		"Only the determined shall taste the Strawberry Patch v2.0.",
		"Confirmed: The cake is still a lie, but the microbes are real",
		"Warning: Sudden spikes of philosophy detected",
		"Legend says the recipe is guarded by a thousand clicks.",
		"The cake IS NOT A LIE",
		"The settings cake is <color=#FF0000>red</color>",
		"Now with 0% artificial colors and 100% strawberry DNA",
		"Strawberry jam is the new red coloring",
		"Now with organic textures (Strawberry flavor)",
		"IS THAT A SEED? (No, it's a pixel)",
		"The cake in the settings is 100% biodegradable",
		"Check the recipe: No artificial bugs added",
		"Now with yeast: Because we hate flat designs",
		"Our cake has more volume than an the Fruit Company Hardware",
		"Yeast included: Evolution isn't just for microbes",
		"The settings cake: Now 100% more fluffy than the Fruit Company Hardware's dreams",
		"Warning: Excessive yeast may cause the UI to expand",
		"Powered by Theobromine: Because caffeine is too mainstream.",
		"Now with 100% more C8H8N4O2 (That's chocolate for you, mortals).",
		"The dev is currently synthesizing dopamine via dark chocolate.",
		"Warning: High levels of Theobromine detected in the source code.",
		"Chocolate: The only fuel compatible with 2026 evolution.",
		"Our microbes don't like coffee, they prefer a fine Ganache.",
		"Theobromine + Strawberry Jam = The ultimate dev build.",
		"Error 0xCHOCO: Not enough cocoa in the system.",
		"Mitosis: Because copy-paste is a biological right.",
		"Natural selection at 60 FPS.",
		"DNA: The original spaghetti code.",
		"Survival of the fittest (and the least buggy).",
		"Evolution is just a series of hotfixes.",
		"Our microbes have more personality than your ex.",
		"Now with 4D hyper-triangles (Patent pending).",
		"Optimized for quantum potatoes.",
		"Fixed a bug where the universe deleted itself.",
		"Gravity is just a suggestion in version 2026.",
		"If the game stays open for 7 years, it might evolve into a spreadsheet.",
		"This splash text is currently being garbage collected.",
		"Circles are just triangles with social anxiety.",
		"Geometry is hard, let's go eat... nothing. Let's go Evolve.",
		"A sphere is just a 1-sided polygon if you squint hard enough.",
		"Edges are overrated. Points are the future.",
		"Made with 99% Theobromine 0% caffeine and 1% documentation.",
		"The code is staring back at me. Send help.",
		"NOOO LUCY WHY DID YOU MADE A GIANT LOAF", //referencia a  I love lucy
		"If you find a bug, call it a 'feature-rich mutation'.",
		"Don't blink. The microbes might migrate to your desktop.",
		"This loading bar is purely decorative.",
		"It's not a crash, it's a spontaneous reboot of reality.",
		"NOW With less angry cyborg small aliens",//Grox de spore
		"May The Magic Friendship Save You", //MLP
		"No more Carrots Following you", //pikmin
		"SNAILS EVERYWERE NOOOO",
		"You Can't LEAVE the tower",//tangled
		"No Dungeons, PLEASE!",
		"Adaptica!!",
		"Keep clicking. The flour isn't going to sift itself.",
		"/a/ /d/ /a/ /p/ /t/ /i/ /k/ /a/", //si, ALFABETO FONETICO INTERNACIONAL
		"こんにちは、日本",
		"你好，中國",
		"Bonjour la France",
		"Hallo Deutschland und Österreich!!",
		"Hier ist das erste unity fernsehn mit der ADAPTICA", //referencia a "hier ist das erste deutsche Fernsehen mit der Tagesschau "
		"I like Metric.",
		"I like Imperial.",
		"e<sup>iπ</sup>",
		"Rwdyxed im Typos",//"reduced" in typos. FALSO hay mucho typo

		//v3.1
		"Kittens and puppies",
	};
	Idioma esp;//si español 
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
	Idioma eng; // si inglés
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
		esp.AgregarLexema("calcetin", PartOfSpeech.Sustantivo, 600);
		//quite a guatemala por si la gente se ofende por que el programa haya generado algo como Piña come guatemala.
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

		if (Override)
		{
			if (idx > Lines.Count)
			{
				idx = Lines.Count-1;
			}
			text.text = Lines[idx]; timer = 0;

		}
	}
}
