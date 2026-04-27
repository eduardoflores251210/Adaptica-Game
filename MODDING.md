# Modding

Internal name es un nombre interno para que el juego indentifique tu parte
Type es el tipo hay 3 tipos 	
* Animal, (0)
* Vehicle,
* Vegetal


la propiedad stats son propiedades que eventualmente se usaran como stats
Mesh es la malla  
icono es una imagen en BASE64
categories son las categorias donde aparecera la parte estan
en el codigo tengo las categorias 



	Microbe = 0,
	Eyes,
	Cats,// esto es broma pero el juego lo acepta
	Mouths,
	Defense,
	Limbs,
	HandsAndFeet,
	BioWings,
	Decoration,
	VehWings,
	Tires,
	Weapons,
	WindowsAndLights,
	Looks,
	Chasis,
	Leaves,
	Sporangium,
	Flowers,
	Fruits,
	Branchs,
	Roots,
	Plant,
	Vehicle,
	Animal,
	Fungus,
  
  function depende del tipo de parte en vehiculo son:
  
  Wings, 
	Weapon,
	Wheel,
	ChassisPart,
	Lights,
	Windows,
  
  en animales y microbios son:
    none = -1,
    Mouth = 0,
    eye= 1,
    healthincreace= 2,
    VelIncreace = 3
    
y finalmente en plantas y hongos son

    none = -1,
    Leaf = 0,
    Branch  = 1,
    healthincreace= 2,
	Flower_Or_Fruit = 3,
    RootBranch = 4,
    Sporangium = 5, //para plantas primitivas/hongos
    
    
  otras propiedades que pueden tener las partes son:
  	public float attackPower;
		public float movementBoost;
en animales/microbios

    public float SunLightPower;
	public float WaterAbsortionBoost;
    public float AtackPower;
    
en plantas


y finalmente

		public float powerConsumption;
		public float armor;
		public float speedBoost;
    
en vehiculos 

(nota para los que no saben mucho de tipos float es un numero decimal)

**Ejemplo de mod**
```
{
  "InternalName": "TestMod:BasicCilia",
  "Type": 0,
  "displayName": "Cilios Básicos",
  "icon":"iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR4nGP4////fwAJ+wP9KobjigAAAABJRU5ErkJggg==",
  "Mesh": {
    "Vertices": [
      {
        "x": 0.44909989833831787,
        "y": 3.1369869709014893,
        "z": -0.44909989833831787
      },
      {
        "x": 1.5592412948608398,
        "y": -1.5592414140701294,
        "z": -1.5592412948608398
      },
      {
        "x": 0.44909989833831787,
        "y": 3.1369869709014893,
        "z": 0.44909989833831787
      }
   ],
    "Triangles": [
      {
        "p": [
          0,
          1,
          2
        ]
      },
  ]
    
  "categories": ["Microbe", "Limbs"],

  "size": 0.6,
  "healthBoost": 2.0,
  "tags": ["microbe", "movement", "starter"],

  "Stats": {
  "stats": [
    {
      "key": "foo",
      "value": "bar"
    }
  ]
},

  "color": {
    "r": 0.6,
    "g": 0.9,
    "b": 1.0,
    "a": 1.0
  },

  "attackPower": 0.0,
  "movementBoost": 2.5,
  "function": 0

} 
```
