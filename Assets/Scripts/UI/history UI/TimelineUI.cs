using ActualUtils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// TimelineUI: genera puntos para un UILineElement basándose en Saver.CurrentGame.Actions
/// Versión corregida: normaliza el rango, aplica intensidad, y ajusta minWidth para ScrollView.
/// </summary>
public class TimelineUI : MonoBehaviour
{
	[Header("Referencias UI")]
	public UIDocument uiDocument;
	public string elementName = "timelineLine"; // name en UI Builder

	[Header("Layout / mapeo")]
	public bool fitToWidth = true;
	public float xSpacing = 20f;
	public float xPadding = 6f;
	public float yPadding = 6f;
	public float minXSpacing = 2f;

	[Header("Opciones de visualización")]
	public bool autoScale = true;          // si true normaliza por min/max en los datos
	public float verticalMultiplier = 1f;  // multiplicador vertical extra (zoom Y)

	[Header("Opciones de actualización")]
	public bool updateEveryFrame = true;

	// Exposición para debug/inspector
	public List<Vector2> POINTS_EXT = new List<Vector2>();

	// Caches
	private VisualElement root;
	private UILineElement lineElement;

	void Start()
	{
		if (uiDocument == null)
		{
			Debug.LogError("[TimelineUI] uiDocument no asignado. Destruyendo componente.");
			DestroyImmediate(this);
			return;
		}

		root = uiDocument.rootVisualElement;
		if (root == null)
		{
			Debug.LogError("[TimelineUI] rootVisualElement es null en UIDocument.");
			DestroyImmediate(this);
			return;
		}

		lineElement = root.Q<UILineElement>(elementName);
		if (lineElement == null)
		{
			Debug.LogError($"[TimelineUI] No se encontró UILineElement con name='{elementName}'.");
			// no destruyo para permitir agregar en runtime, pero salgo.
			return;
		}

		// Primera actualización (si el layout no está listo, la función lo manejará)
		ManualUpdateTimeline();
	}

	void Update()
	{
		if (updateEveryFrame)
			ManualUpdateTimeline();
	}

	public void ManualUpdateTimeline()
	{
		// Chequeos rápidos
		if (uiDocument == null || root == null)
			return;

		if (lineElement == null)
		{
			lineElement = root.Q<UILineElement>(elementName);
			if (lineElement == null)
			{
				// si no está, no hacemos nada
				return;
			}
		}

		// Recolectar datos
		var rawValues = new List<float>();
		var intensities = new List<float>();

		if (Saver.CurrentGame == null || Saver.CurrentGame.Actions == null)
		{
			// Nada que mostrar
			lineElement.Points.Clear();
			lineElement.MarkDirtyRepaint();
			return;
		}

		// Recorremos actions con índice seguro
		for (int i = 0; i < Saver.CurrentGame.Actions.Count; i++)
		{
			var item = Saver.CurrentGame.Actions[i];
			if (item == null)
			{
				rawValues.Add(0f);
				intensities.Add(0.25f);
				continue;
			}

			float v = item.Path switch
			{
				SerializableTypes.HistoryPaths.Friendly => 1f,
				SerializableTypes.HistoryPaths.Neutral => 0f,
				SerializableTypes.HistoryPaths.Agressive => -1f,
				_ => 0f
			};
			rawValues.Add(v);

			// intensidad con fallback seguro
			float intensity = 0.25f;
			if (item.propieties != null && item.propieties.ContainsKey("intense"))
			{
				if (!float.TryParse(item.propieties["intense"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out intensity))
				{
					intensity = 0.25f;
				}
			}
			intensities.Add(intensity);
		}

		if (rawValues.Count == 0)
		{
			lineElement.Points.Clear();
			lineElement.MarkDirtyRepaint();
			return;
		}

		// Construir valores acumulados (historical) usando intensities
		var values = new List<float>(rawValues.Count);
		float acc = 0f;
		for (int i = 0; i < rawValues.Count; i++)
		{
			acc += rawValues[i] * intensities[i];
			acc = Mathf.Clamp(acc, -1f, 1f);
			values.Add(acc);
		}

		// --- Obtener tamaño del elemento ---
		float width = lineElement.layout.width;
		float height = lineElement.layout.height;

		// Si layout aún no calculado, intentamos resolvedStyle
		if (width <= 0f || height <= 0f)
		{
			width = lineElement.resolvedStyle.width;
			height = lineElement.resolvedStyle.height;
		}

		// Si aún no hay tamaño razonable, devolvemos (esperamos a la próxima pasada)
		// Evita forzar tamaños por defecto que ocultan problemas de layout.
		if (width <= 1f || height <= 1f)
		{
			// Log solo la primera vez para no spamear
			// Si realmente quieres, descomenta el Debug.Log
			// Debug.Log("[TimelineUI] Esperando layout válido (width/height pequeños).");
			return;
		}

		// --- Calcular spacing X ---
		int n = values.Count;
		float spacing;
		if (fitToWidth && n > 1)
		{
			float usable = Mathf.Max(1f, width - 2f * xPadding);
			spacing = usable / Mathf.Max(1, n - 1);
			if (spacing < minXSpacing) spacing = minXSpacing;
		}
		else
		{
			spacing = xSpacing;
		}

		float startX = xPadding;

		// --- Normalizar Y ---
		float minVal = float.MaxValue, maxVal = float.MinValue;
		foreach (var v in values)
		{
			if (v < minVal) minVal = v;
			if (v > maxVal) maxVal = v;
		}

		// Si todos iguales, ampliar un poco el rango para que no quede en una línea
		if (Mathf.Approximately(minVal, maxVal))
		{
			minVal -= 0.5f;
			maxVal += 0.5f;
		}

		float usableHeight = Mathf.Max(1f, height - 2f * yPadding);

		// Construir puntos mapeados
		var pts = new List<Vector2>(n);

		for (int i = 0; i < n; i++)
		{
			float raw = values[i];

			// autoScale usa InverseLerp; si no, asumimos rango -1..1 y aplicamos multiplier
			float normalized;
			if (autoScale)
			{
				normalized = Mathf.InverseLerp(minVal, maxVal, raw); // 0..1
			}
			else
			{
				// si no autoScale, primero clamp al rango [-1,1]
				float clamped = Mathf.Clamp(raw * verticalMultiplier, -1f, 1f);
				normalized = (clamped + 1f) / 2f;
			}

			// si autoScale, todavía aplicamos verticalMultiplier sensato:
			if (autoScale && !Mathf.Approximately(verticalMultiplier, 1f))
			{
				// transformamos normalized centrado en 0.5 y lo escalamos
				float centered = normalized - 0.5f; // -0.5..0.5
				centered *= verticalMultiplier;
				normalized = Mathf.Clamp01(centered + 0.5f);
			}

			float x = startX + i * spacing;
			float y = (1f - normalized) * usableHeight + yPadding;

			// Clamps por seguridad (aunque el diseño ideal es que no haga falta)
			x = Mathf.Clamp(x, 0f, Mathf.Max(0f, width));
			y = Mathf.Clamp(y, 0f, Mathf.Max(0f, height));

			pts.Add(new Vector2(x, y));
		}

		// Forzar minWidth para ScrollView (último punto + padding)
		if (pts.Count > 0)
		{
			float requiredWidth = pts[^1].x + xPadding;
			lineElement.style.width = requiredWidth;
			lineElement.style.minHeight = Mathf.Max(5f, usableHeight * 0.1f);
		}

		// Aplicar puntos al elemento
		POINTS_EXT = pts; // exposicion debug
		lineElement.SetPoints(pts);
		lineElement.MarkDirtyRepaint();
	}
}