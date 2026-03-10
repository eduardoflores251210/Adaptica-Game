using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Renderizador de lineas para UI Toolkit
/// sera usado en la pantalla de Historia para mostrar el progreso del jugador
/// </summary>
public class UILineElement : VisualElement
{
	public new class UxmlFactory : UxmlFactory<UILineElement, UxmlTraits> { }

	public new class UxmlTraits : VisualElement.UxmlTraits
	{
		UxmlFloatAttributeDescription thickness =
			new() { name = "thickness", defaultValue = 2f };

		UxmlColorAttributeDescription color =
			new() { name = "color", defaultValue = Color.white };

		// 👇 EL TRUCO
		UxmlStringAttributeDescription points =
			new()
			{
				name = "points",
				defaultValue = "(10,10),(200,100),(350,50)"
			};

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);

			var line = (UILineElement)ve;

			line.Thickness = thickness.GetValueFromBag(bag, cc);
			line.Color = color.GetValueFromBag(bag, cc);

			var pointsString = points.GetValueFromBag(bag, cc);
			line.SetPointsFromString(pointsString);
		}
	}

	public float Thickness = 2f;
	public Color Color = Color.white;
	public List<Vector2> Points = new();

	public UILineElement()
	{
		generateVisualContent += Draw;
	}

	// 🔥 Parseo del string
	public void SetPointsFromString(string value)
	{
		Points.Clear();

		if (string.IsNullOrWhiteSpace(value))
			return;

		var entries = value.Split(')');

		foreach (var entry in entries)
		{
			var clean = entry.Replace("(", "").Replace(",", " ").Trim();
			if (string.IsNullOrEmpty(clean))
				continue;

			var parts = clean.Split(' ');
			if (parts.Length < 2)
				continue;

			if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
				float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
			{
				Points.Add(new Vector2(x, y));
			}
		}

		MarkDirtyRepaint();
	}

	void Draw(MeshGenerationContext ctx)
	{
		if (Points.Count < 2)
			return;

		var p = ctx.painter2D;
		p.strokeColor = Color;
		p.lineWidth = Thickness;
		p.lineJoin = LineJoin.Round;
		p.lineCap = LineCap.Round;

		p.BeginPath();
		p.MoveTo(Points[0]);

		for (int i = 1; i < Points.Count; i++)
			p.LineTo(Points[i]);

		p.Stroke();
	}
	// Método público para establecer puntos desde código
	public void SetPoints(List<Vector2> pts)
	{
		Points = new List<Vector2>(pts); // sustituye la lista interna
		MarkDirtyRepaint(); // fuerza el redibujado
	}

}
