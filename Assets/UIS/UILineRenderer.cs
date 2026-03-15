using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Renderizador de líneas para UI Toolkit con auto-fit y alineamiento.
/// Usa AutoFitToPoints = true para que el elemento ajuste su tamaño a los puntos
/// y sea correctamente medido por un ScrollView.
/// </summary>
public class UILineElement : VisualElement
{
	public enum Anchor
	{
		TopLeft, TopCenter, TopRight,
		CenterLeft, Center, CenterRight,
		BottomLeft, BottomCenter, BottomRight
	}

	public new class UxmlFactory : UxmlFactory<UILineElement, UxmlTraits> { }

	public new class UxmlTraits : VisualElement.UxmlTraits
	{
		UxmlFloatAttributeDescription thickness =
			new() { name = "thickness", defaultValue = 2f };

		UxmlColorAttributeDescription color =
			new() { name = "color", defaultValue = Color.white };

		UxmlStringAttributeDescription points =
			new()
			{
				name = "points",
				defaultValue = "(10,10),(200,100),(350,50)"
			};

		UxmlBoolAttributeDescription autoFit =
			new() { name = "auto-fit", defaultValue = true };

		UxmlFloatAttributeDescription padding =
			new() { name = "padding", defaultValue = 6f };

		UxmlFloatAttributeDescription targetWidth =
			new() { name = "target-width", defaultValue = 0f };

		UxmlFloatAttributeDescription targetHeight =
			new() { name = "target-height", defaultValue = 0f };

		UxmlStringAttributeDescription anchor =
			new() { name = "anchor", defaultValue = "TopLeft" };

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);

			var line = (UILineElement)ve;

			line.Thickness = thickness.GetValueFromBag(bag, cc);
			line.Color = color.GetValueFromBag(bag, cc);

			var pointsString = points.GetValueFromBag(bag, cc);
			line.SetPointsFromString(pointsString);

			line.AutoFitToPoints = autoFit.GetValueFromBag(bag, cc);
			line.Padding = padding.GetValueFromBag(bag, cc);
			line.TargetWidth = targetWidth.GetValueFromBag(bag, cc);
			line.TargetHeight = targetHeight.GetValueFromBag(bag, cc);

			var anchorStr = anchor.GetValueFromBag(bag, cc);
			if (System.Enum.TryParse<Anchor>(anchorStr, out var parsed))
				line.ContentAnchor = parsed;
			else
				line.ContentAnchor = Anchor.TopLeft;
		}
	}

	// Public API
	public float Thickness = 2f;
	public Color Color = Color.white;
	public List<Vector2> Points = new();

	// Auto-fit / alignment properties
	public bool AutoFitToPoints = true;
	public float Padding = 6f;
	/// <summary>Si > 0, fuerza ese ancho en lugar de usar bounding box + padding</summary>
	public float TargetWidth = 0f;
	/// <summary>Si > 0, fuerza ese alto en lugar de usar bounding box + padding</summary>
	public float TargetHeight = 0f;
	public Anchor ContentAnchor = Anchor.TopLeft;

	public UILineElement()
	{
		generateVisualContent += Draw;

		// prevenir que el ScrollView lo encoja
		style.flexShrink = 0;
		// posición relativa por defecto (si estaba absoluto, ScrollView no lo medirá)
		style.position = Position.Relative;
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

		// Auto-fit al setear desde UXML o string
		MaybeFitAndApply();
	}

	///<summary>
	///Método público para establecer puntos desde código por  si estas loco o flojo y quieres un setter tonto
	///</summary>
	public void SetPoints(List<Vector2> pts)
	{
		Points = new List<Vector2>(pts); // sustituye la lista interna
		MaybeFitAndApply();
	}

	void Draw(MeshGenerationContext ctx)
	{
		if (Points == null || Points.Count < 2)
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

	// ----------------------
	// Auto-fit / alignment
	// ----------------------

	void MaybeFitAndApply()
	{
		// Si no está activo el auto-fit, solo redibujar
		if (!AutoFitToPoints)
		{
			MarkDirtyRepaint();
			return;
		}

		if (Points == null || Points.Count == 0)
		{
			MarkDirtyRepaint();
			return;
		}

		FitAndAlignPoints(Padding, TargetWidth, TargetHeight, ContentAnchor);
	}

	/// <summary>
	/// Calcula bounding box de Points, normaliza según padding y anchor, y actualiza style.width/height.
	/// Si targetWidth/targetHeight > 0 se toman como tamaño final y se alinea el contenido dentro.
	/// </summary>
	public void FitAndAlignPoints(float padding, float targetWidth, float targetHeight, Anchor anchor)
	{
		// Calcular bounding box original
		float minX = float.MaxValue, minY = float.MaxValue;
		float maxX = float.MinValue, maxY = float.MinValue;
		foreach (var p in Points)
		{
			if (p.x < minX) minX = p.x;
			if (p.y < minY) minY = p.y;
			if (p.x > maxX) maxX = p.x;
			if (p.y > maxY) maxY = p.y;
		}

		float bboxW = Mathf.Max(0.0001f, maxX - minX);
		float bboxH = Mathf.Max(0.0001f, maxY - minY);

		// Tamaño final del elemento
		float finalW = (targetWidth > 0f) ? targetWidth : (bboxW + padding * 2f);
		float finalH = (targetHeight > 0f) ? targetHeight : (bboxH + padding * 2f);

		// Si target es menor que bbox, lo sobreescribimos para que quepa
		if (finalW < bboxW + 2f) finalW = bboxW + 2f;
		if (finalH < bboxH + 2f) finalH = bboxH + 2f;

		// FACTORES de anchor: 0 => left/top, 0.5 => center, 1 => right/bottom
		float ax = 0f, ay = 0f;
		switch (anchor)
		{
			case Anchor.TopLeft: ax = 0f; ay = 0f; break;
			case Anchor.TopCenter: ax = 0.5f; ay = 0f; break;
			case Anchor.TopRight: ax = 1f; ay = 0f; break;
			case Anchor.CenterLeft: ax = 0f; ay = 0.5f; break;
			case Anchor.Center: ax = 0.5f; ay = 0.5f; break;
			case Anchor.CenterRight: ax = 1f; ay = 0.5f; break;
			case Anchor.BottomLeft: ax = 0f; ay = 1f; break;
			case Anchor.BottomCenter: ax = 0.5f; ay = 1f; break;
			case Anchor.BottomRight: ax = 1f; ay = 1f; break;
		}

		// Espacio extra disponible entre finalSize y bbox
		float extraX = finalW - (bboxW + 2f * padding);
		float extraY = finalH - (bboxH + 2f * padding);

		float offsetX = padding + extraX * ax - minX;
		float offsetY = padding + extraY * ay - minY;

		// Normalizar y desplazar puntos
		var newPts = new List<Vector2>(Points.Count);
		for (int i = 0; i < Points.Count; i++)
		{
			var p = Points[i];
			newPts.Add(new Vector2(p.x + offsetX, p.y + offsetY));
		}

		Points = newPts;

		// Aplicar tamaño al elemento para que ScrollView lo mida
		style.width = finalW;
		style.height = finalH;

		// Asegurar que no se encoja en layouts flexibles
		style.flexShrink = 0;

		// Finalmente, redibujar
		MarkDirtyRepaint();
	}



}
