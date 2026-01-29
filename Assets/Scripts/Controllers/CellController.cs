using SerializableTypes; //remanente de cuando los 3 serializables estaban en Utils.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using StandartUtilities;
using ModelosDeIdioma; //No se cuando probe aqui el generador de idiomas
using SerializableTypes.Biology;
using ActualUtils;
/// <summary>
/// Contolador principal del microbio en juego ademas tiene IA para NPC
/// aunque son muy tontos por ahora
/// </summary>

public class CellController : MonoBehaviour
{
	[Header("ParametrosIA")]
	public bool isAI;
	public string ID ="0";
	[Header("Referencias")]
	public RectTransform Cursor;         // Cursor UI (en pantalla)
	public PartsDatabase Parts;
	public Material BaseMat;
	public Transform Cam;
	public Transform BG;
	[Header("Cursor")]
	public float CursorSpeed = 10f;      // Sensibilidad del cursor UI

	[Header("Input")]
    public float YCamOffset =10;
    public InputActionAsset inputActions;
	public float BaseSpeedMultiplier;
	public float SpeedMultiplier;
	private InputAction MoveMicrobe;
	private InputAction moveCursorAction;
	public bool isOnCreatureStage = false; //esto acirvara en el futuro el salto
	[Header("PlayInfo")]
	public float Health = 20;
	public float MaxHealth = 20;
	public float CurrentEvoPoints = 0;
	public float MaxEvoPointsGotStat = 0;
	public float StageProgress = 0;
	public Diets CreatureDiet;
	public GéneroBiológico CurrentGen = GéneroBiológico.None;
	private Vector2 cursorPosition = Vector2.zero;
	private Vector3 targetPosition;
	private float wanderRadius = 5f;      // radio de deambulación
	public float viewRadius = 0;
	InputActionMap map;
	private void OnEnable()
	{
		if (isAI) return;
		map = inputActions.FindActionMap("CellMov", true);
		map.Enable();

		MoveMicrobe = map.FindAction("L", true);
		moveCursorAction = map.FindAction("R", true);

		moveCursorAction.performed += OnMoveCursor;





		moveCursorAction.performed += ctx => MoveCursor(ctx.ReadValue<Vector2>(),ctx.control.device);
	}
	private void Start()
	{
		SetupMicrobe();
	}

	private void OnDisable()
	{
		if (isAI)
			targetPosition = GetRandomWanderPosition();

		if (isAI) return;
		var map = inputActions.FindActionMap("CellMov", true);
		map.Disable();

		// Desuscripción correcta en OnDisable
		moveCursorAction.performed -= OnMoveCursor;

	}

	private void LateUpdate()
	{
		if (isAI) return;
		if (Pointer.current != null && Cursor != null)
		{
			Vector2 PointerScreenPos = Pointer.current.position.ReadValue();
			Vector2 localPoint;

			RectTransform parentRect = Cursor.parent as RectTransform;

			// Usar null para cámara si el Canvas es Screen Space Overlay
			Camera uiCamera = null;
			Canvas canvas = Cursor.GetComponentInParent<Canvas>();
			if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
				uiCamera = canvas.worldCamera;

			bool valid = RectTransformUtility.ScreenPointToLocalPointInRectangle(
				parentRect,
				PointerScreenPos,
				uiCamera,
				out localPoint
			);

			if (valid)
			{
				cursorPosition = localPoint;
			}
			if (Cursor.TryGetComponent<Image>(out var Eñe))
			{
				Eñe.enabled = false;
			}
		}
		if (Pointer.current == null)
		{
			if (Cursor.TryGetComponent<Image>(out var Eñe))
			{
				Eñe.enabled = true;
			}
		}

		// Mover el cursor UI
		if (Cursor != null)
		{
			Cursor.anchoredPosition = cursorPosition;
		}
	}


	private void MoverMicrobio(Vector2 input)
	{
		transform.Translate(new(input.x,0,input.y), Space.Self);
	}


	private void OnMoveCursor(InputAction.CallbackContext ctx) => MoveCursor(ctx.ReadValue<Vector2>(), ctx.control.device);

	private void FixedUpdate()
	{
		if (isAI)
		{
			// Obtener todas las bocas de los hijos
			MouthComp[] mouths = GetComponentsInChildren<MouthComp>();
			if (mouths.Length == 0)
			{
				Debug.LogWarning("AI sin bocas detectadas");
				return;
			}

			// Detectar comida en rango
			float scaledViewRadius = viewRadius * ((transform.localScale.x + transform.localScale.y + transform.localScale.z) / 3);
			Collider[] hits = Physics.OverlapSphere(transform.position, scaledViewRadius);

			FoodComp closestFood = null;
			MouthComp closestMouth = null;
			float closestDist = Mathf.Infinity;

			foreach (var hit in hits)
			{
				if (hit.TryGetComponent<FoodComp>(out var food))
				{
					if (!MouthComp.IsFoodCompatibleWithDiet(food.tipo, CreatureDiet))
						continue;

					// Buscar la boca más cercana a este alimento
					foreach (var mouth in mouths)
					{
						float dist = Vector3.Distance(mouth.transform.position, food.transform.position);
						if (dist < closestDist)
						{
							closestDist = dist;
							closestFood = food;
							closestMouth = mouth;
						}
					}
				}
			}

			// Establecer objetivo desde la boca más cercana
			if (closestFood != null && closestMouth != null)
			{
				targetPosition = closestFood.transform.position;
				// Opcional: mover el microbio de forma que la boca llegue primero
				Vector3 offset = closestMouth.transform.position - transform.position;
				targetPosition -= offset;
			}
			else if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
			{
				targetPosition = GetRandomWanderPosition();
			}

			// Moverse hacia el objetivo
			Vector3 direction = (targetPosition - transform.position).normalized;
			transform.Translate(direction * (BaseSpeedMultiplier * SpeedMultiplier) * Time.fixedDeltaTime, Space.World);

			// Rotar suavemente hacia el objetivo
			if (direction != Vector3.zero)
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.1f);
		}

		// Control manual
		if (!isAI && MoveMicrobe != null && MoveMicrobe.IsPressed())
		{
			Vector2 input = MoveMicrobe.ReadValue<Vector2>();
			Vector2 movimiento = input * (BaseSpeedMultiplier * SpeedMultiplier) * Time.fixedDeltaTime;

			// Mover el microbio
			MoverMicrobio(new Vector3(0, movimiento.y, 0));
			transform.Rotate(new Vector3(0, input.x, 0), Space.World);
			if (!isOnCreatureStage)
			{
				Cam.position = transform.position + new Vector3(0, YCamOffset, 0);
				BG.position = transform.position + new Vector3(0, -4.62f, 0);
			}
			//Debug.Log($"Movimiento por FixedUpdate: {movimiento} (m por frame)");
			float velocidadReal = movimiento.magnitude / Time.fixedDeltaTime;
			//Debug.Log($"Velocidad real estimada (m/s): {velocidadReal}");
		}
	}


	private Vector3 GetRandomWanderPosition()
	{
		Vector2 circle = Random.insideUnitCircle * wanderRadius;
		Vector3 pos = transform.position + new Vector3(circle.x, 0, circle.y);
		return pos;
	}


	private void MoveCursor(Vector2 input,InputDevice a)
	{
		//Debug.Log(a.GetType().ToString());
		if (!(a is Pointer))
		{
			Debug.Log("Adsfd");
			cursorPosition += input * CursorSpeed;
		}
		else SetCursor();
	}
	void SetCursor()
	{
		cursorPosition = Pointer.current.position.value;
	}

	/// <summary>
	/// Configura el/la Microbi@
	/// </summary>
	void SetupMicrobe()
	{
		Vector3 OldPos = transform.position;
		var aa = LoadMicrobe();
		MeshRenderer renderer = GetComponent<MeshRenderer>();
		if (renderer == null)
			renderer = gameObject.AddComponent<MeshRenderer>();

		Material mat = new Material(BaseMat);
		transform.position = Vector3.zero;
		// Determinar género si hay ambos disponibles
		if (aa.HasMale)
		{
			CurrentGen = StandartUtilities.StdUtils.Randomness.CoinFlip() ?
						 GéneroBiológico.Female :
						 GéneroBiológico.Male;
		}
		if (Saver.HasLoadedAnySave())
		{
			var CD = Saver.CurrentGame.CellGameData;
			CurrentGen = CD.Gender;
			StageProgress = CD.Progress;
			MaxEvoPointsGotStat = CD.MaxDNA_Got;
			Health = CD.PlayerHealth;
			CurrentEvoPoints = CD.DNA_Amount;
		}
		// Seleccionar color y partes según género
		Color color;
		var partsList = new List<SerializedPartData>(); 

		if (CurrentGen == GéneroBiológico.Female || CurrentGen == GéneroBiológico.None)
		{
			color = aa.FemaleColor;
			partsList = aa.PartsF;
		}
		else // GéneroBiológico.Male
		{
			color = aa.MaleColor;
			partsList = aa.PartsM;
		}

		mat.color = color;
		renderer.material = mat;
		int HeCount=0;
		int CaCount=0;
		int OmCount=0;

		// Instanciar las partes
		foreach (var ff in partsList)
		{
			var GO = Instantiate(Parts.GetPartByID(ff.Id).prefab);
			GO.transform.SetPositionAndRotation(ff.transform.Pos, Quaternion.Euler(ff.transform.Rot));
			GO.transform.localScale = ff.transform.Scale;
			GO.transform.SetParent(transform, true);

			if (Parts.GetPartByID(ff.Id) is BiologicalPart bio && bio.function == BiologicalPartFunction.Mouth)
			{
				var rb = GO.AddComponent<Rigidbody>();
				rb.useGravity = false;
				rb.isKinematic = true;

				var MC = GO.AddComponent<MouthComp>();
				MC.cellController = this;
				if (bio.tags.Contains("Carn"))
				{
					MC.ComidasQuePuedeComer = Diets.Carnivore;
					CaCount++;
				}
				if (bio.tags.Contains("Herb"))
				{
					MC.ComidasQuePuedeComer = Diets.Herbivore;
					HeCount++;
				}
				if (bio.tags.Contains("Omn"))
				{
					MC.ComidasQuePuedeComer = Diets.Herbivore;
					OmCount++;
				}
			}
		}
		if (HeCount > 0 && CaCount == 0)
		{
			CreatureDiet = Diets.Herbivore;
		}
		if (CaCount > 0 && HeCount == 0)
		{
			CreatureDiet = Diets.Carnivore;
		}
		if (HeCount > 0 && CaCount > 0 || OmCount > 0)
		{
			CreatureDiet = Diets.Omnivore;
		}
		if (HeCount == 0 && CaCount == 0 && OmCount == 0)
		{
			CreatureDiet = Diets.none;
		}
		SpeedMultiplier = CalculateSpeedMultiplier(aa, CurrentGen);
		viewRadius = CalculateViewRadius(LoadMicrobe(), CurrentGen);
		// Configurar mesh
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		if (meshFilter == null)
			meshFilter = gameObject.AddComponent<MeshFilter>();

		var mesh = (UnityEngine.Mesh)aa.Mesh; // mesh es del tipo StandartUtilities.StdUtils.Serializable.Mesh
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.RecalculateTangents();
		meshFilter.mesh = mesh;
		ID = MicrobeData.GenerateMicrobeID(aa);
		transform.position = OldPos;
		if (!isAI)
			PlayerManager.RegisterPlayer(this, Stages.Microbe);
	}

	/// <summary>
	/// Carga El Microbio en base al paquete recibido
	/// o crea uno nuevo para casos de emergencia
	/// </summary>
	/// <returns>Un Microbio nueva o cargada</returns>
	MicrobeData LoadMicrobe()
	{
		string BAseOBJ = "# 8 vertices del cubo\r\nv -1 -1 -1\r\nv  1 -1 -1\r\nv  1  1 -1\r\nv -1  1 -1\r\nv -1 -1  1\r\nv  1 -1  1\r\nv  1  1  1\r\nv -1  1  1\r\n\r\n# 12 triángulos (2 por cara) - normales invertidas\r\n# Cara frontal\r\nf 3 2 1\r\nf 4 3 1\r\n\r\n# Cara trasera\r\nf 7 8 5\r\nf 6 7 5\r\n\r\n# Cara izquierda\r\nf 8 4 1\r\nf 5 8 1\r\n\r\n# Cara derecha\r\nf 7 6 2\r\nf 3 7 2\r\n\r\n# Cara superior\r\nf 7 3 4\r\nf 8 7 4\r\n\r\n# Cara inferior\r\nf 6 5 1\r\nf 2 6 1\r\n";
		MicrobeData Tempdata;
		var EmergencyMEsh = StandartUtilities.StdUtils.Serializable.Mesh.FromObjString(BAseOBJ);
		Tempdata = new MicrobeData("AAAAA",
								  "AAAA",
								  false,
								  reproductionTypes.SingleCell,
								  ReproductionMethod.Mitosis,
								  new List<SerializedPartData>() {
									  new("0", new StdUtils.Serializable.Transform(Vector3.forward, Quaternion.identity.eulerAngles, Vector3.one)),
									  new("-1", new StdUtils.Serializable.Transform(Vector3.up, Quaternion.identity.eulerAngles, Vector3.one)) 
								  },
								  null,
								  Color.magenta,
								  Color.blue,
								  new() { new() { radius = 3 } }, mesh: EmergencyMEsh);
	   
		if (CrossScenePackageSender.Instance == null) return Tempdata;
		if (!CrossScenePackageSender.Instance.IsThereAnyTypedMailForMe<MicrobeData>(gameObject, out var Mail))
		{
			if (!Saver.HasLoadedAnySave())
				return Tempdata;
			else
			{
				if (Saver.TryToLoadLastMicrobeRevision(Saver.CurrentSaveName, out var MC))
				{
					return MC;
				}
				else return Tempdata;
			}

		}
		else return Mail[0].Contents; 
	}
	float CalculateSpeedMultiplier(MicrobeData MicrobeData, GéneroBiológico género)
	{

		List <SerializedPartData> Tempdata;
		if (MicrobeData.HasMale) 
		{
			Tempdata = Tempdata = género switch
			{
				GéneroBiológico.None => MicrobeData.PartsF,
				GéneroBiológico.Male => MicrobeData.PartsM,
				GéneroBiológico.Female => MicrobeData.PartsF,
				_ => MicrobeData.PartsF,
			};
			
		}else Tempdata = MicrobeData.PartsF;
		float Mult = 0;
		foreach (var part in Tempdata)
		{
			var PP = Parts.GetPartByID(part.Id);
			if (PP is BiologicalPart bioPart)
			{
				Mult += bioPart.movementBoost;
			}
		}

		return 1+(Mult*(0.125f));
	}
	float CalculateViewRadius(MicrobeData MicrobeData, GéneroBiológico género)
	{

		List<SerializedPartData> Tempdata;
		if (MicrobeData.HasMale)
		{
			Tempdata = Tempdata = género switch
			{
				GéneroBiológico.None => MicrobeData.PartsF,
				GéneroBiológico.Male => MicrobeData.PartsM,
				GéneroBiológico.Female => MicrobeData.PartsF,
				_ => MicrobeData.PartsF,
			};

		} else Tempdata = MicrobeData.PartsF;
		List<float> Rad = new();
		foreach (var part in Tempdata)
		{
			var PP = Parts.GetPartByID(part.Id);
			if (PP is BiologicalPart bioPart)
			{ try
				{
					if (bioPart.function == BiologicalPartFunction.eye)
						Rad.Add(float.Parse(bioPart.Stats.GetValue("ViewRadius")));
				} catch (System.Exception)
				{

				}
			}
		}
		if (Rad.Count == 0)
		{
			Rad.Add(0);
			Rad.Add(0);
			Rad.Add(0);
			Rad.Add(0);
			Rad.Add(0);
		}	
		return Mathf.Max(Rad.ToArray());

	}
	private void OnDestroy()
	{
		if (!isAI && PlayerManager.Player == this)
			PlayerManager.UnRegisterPlayer();
	}
}
