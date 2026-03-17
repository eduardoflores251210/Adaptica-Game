using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// UILineElement v2 — profesional y sin corrupción de datos.
/// - FitAndAlignPoints calcula tamaño y parámetros de transformación.
/// - Draw aplica transformaciones al vuelo (no modifica Points).
/// - Minimiza allocations para manejar timelines largos.
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
		UxmlFloatAttributeDescription thickness = new() { name = "thickness", defaultValue = 2f };
		UxmlColorAttributeDescription color = new() { name = "color", defaultValue = Color.white };
		UxmlFloatAttributeDescription padding = new() { name = "padding", defaultValue = 6f };
		UxmlFloatAttributeDescription targetWidth = new() { name = "target-width", defaultValue = 0f };
		UxmlFloatAttributeDescription targetHeight = new() { name = "target-height", defaultValue = 0f };
		UxmlStringAttributeDescription anchor = new() { name = "anchor", defaultValue = "TopLeft" };

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			var line = (UILineElement)ve;
			line.Thickness = thickness.GetValueFromBag(bag, cc);
			line.Color = color.GetValueFromBag(bag, cc);
			line.Padding = padding.GetValueFromBag(bag, cc);
			line.TargetWidth = targetWidth.GetValueFromBag(bag, cc);
			line.TargetHeight = targetHeight.GetValueFromBag(bag, cc);

			var anchorStr = anchor.GetValueFromBag(bag, cc);
			if (System.Enum.TryParse<Anchor>(anchorStr, out var parsed)) line.ContentAnchor = parsed;
			else line.ContentAnchor = Anchor.TopLeft;
		}
	}

	// Public API
	public float Thickness = 2f;
	public Color Color = Color.white;
	public List<Vector2> Points { get; private set; } = new List<Vector2>();

	public float Padding = 6f;
	public float TargetWidth = 0f;   // si >0 fija ancho
	public float TargetHeight = 0f;  // si >0 fija alto
	public Anchor ContentAnchor = Anchor.TopLeft;

	// Internal layout/transform state (calculado por FitAndAlignPoints)
	float m_minX, m_minY, m_bboxW, m_bboxH;
	float m_scaleX = 1f, m_scaleY = 1f;
	float m_extraX = 0f, m_extraY = 0f;
	float m_offsetX = 0f, m_offsetY = 0f;

	public UILineElement()
	{
		generateVisualContent += Draw;
		style.flexShrink = 0;
		style.position = Position.Relative;
	}

	/// <summary>
	/// Reemplaza puntos (copia). No modifica los puntos originales después.
	/// </summary>
	public void SetPoints(List<Vector2> pts)
	{
		if (pts == null)
		{
			Points.Clear();
		}
		else
		{
			// copia para evitar referencias externas modificando la lista internamente
			Points.Clear();
			Points.AddRange(pts);
		}

		// recalcular layout/transform si corresponde
		MaybeFitAndApply();
	}

	void MaybeFitAndApply()
	{
	
		if (Points == null || Points.Count == 0)
		{
			MarkDirtyRepaint();
			return;
		}

		// FitAndAlignPoints calcula size y parámetros de transformación,
		// pero NO modifica Points.
		FitAndAlignPoints(Padding, TargetWidth, TargetHeight, ContentAnchor);
	}

	/// <summary>
	/// Calcula bounding box y parámetros de transformación. NO cambia Points.
	/// </summary>
	public void FitAndAlignPoints(float padding, float targetWidth, float targetHeight, Anchor anchor)
	{
		// bounding box
		float minX = float.MaxValue, minY = float.MaxValue;
		float maxX = float.MinValue, maxY = float.MinValue;
		foreach (var p in Points)
		{
			if (p.x < minX) minX = p.x;
			if (p.y < minY) minY = p.y;
			if (p.x > maxX) maxX = p.x;
			if (p.y > maxY) maxY = p.y;
		}

		// defensivo: si los puntos están todos en la misma coordenada
		float bboxW = Mathf.Max(0.0001f, maxX - minX);
		float bboxH = Mathf.Max(0.0001f, maxY - minY);

		// tamaño final del elemento
		float finalW = (targetWidth > 0f) ? targetWidth : (bboxW + padding * 2f);
		float finalH = (targetHeight > 0f) ? targetHeight : (bboxH + padding * 2f);

		if (finalW < bboxW + 2f) finalW = bboxW + 2f;
		if (finalH < bboxH + 2f) finalH = bboxH + 2f;

		// anchor factors
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

		// Escalado para que el bbox encaje en finalW/finalH dentro del padding.
		float availableW = Mathf.Max(0.0001f, finalW - 2f * padding);
		float availableH = Mathf.Max(0.0001f, finalH - 2f * padding);

		float scaleX = availableW / bboxW;
		float scaleY = availableH / bboxH;

		// Decide si quieres escalar X e Y; aquí permitimos escalar ambas.
		// Si prefieres mantener 1:1 podrías usar min(scaleX, scaleY) o 1f.
		// Usaremos scaleX, scaleY para que ocupen todo el area.
		float scaledBBoxW = bboxW * scaleX;
		float scaledBBoxH = bboxH * scaleY;

		// Extra espacio si finalW > scaledBBoxW + 2*padding (puede ocurrir si targetWidth fue mayor)
		float extraX = finalW - (scaledBBoxW + 2f * padding);
		float extraY = finalH - (scaledBBoxH + 2f * padding);

		float offsetX = padding + extraX * ax; // desplazamiento por anchor
		float offsetY = padding + extraY * ay;

		// Guardar estado interno para Draw()
		m_minX = minX;
		m_minY = minY;
		m_bboxW = bboxW;
		m_bboxH = bboxH;
		m_scaleX = scaleX;
		m_scaleY = scaleY;
		m_extraX = extraX;
		m_extraY = extraY;
		m_offsetX = offsetX;
		m_offsetY = offsetY;

		// Aplicar tamaño al elemento para que ScrollView lo mida
		style.width = finalW;
		style.height = finalH;
		style.flexShrink = 0;

		MarkDirtyRepaint();
	}

	void Draw(MeshGenerationContext ctx)
	{
		if (Points == null || Points.Count < 2)
			return;

		// obtener painter2D
		var p = ctx.painter2D;
		p.strokeColor = Color;
		p.lineWidth = Thickness;
		p.lineJoin = LineJoin.Round;
		p.lineCap = LineCap.Round;

		// Mapear el primer punto transformándolo en pixel-space sin crear arrays
		// transform function (inline, evita allocations)
		float minX = m_minX;
		float minY = m_minY;
		float sX = m_scaleX;
		float sY = m_scaleY;
		float offX = m_offsetX;
		float offY = m_offsetY;

		// draw path
		p.BeginPath();

		// Primer punto
		var p0 = Points[0];
		float tx0 = (p0.x - minX) * sX + offX;
		float ty0 = (p0.y - minY) * sY + offY;
		p.MoveTo(new Vector2(tx0, ty0));

		// Resto de puntos (sin allocations)
		for (int i = 1; i < Points.Count; i++)
		{
			var q = Points[i];
			float tx = (q.x - minX) * sX + offX;
			float ty = (q.y - minY) * sY + offY;
			p.LineTo(new Vector2(tx, ty));
		}

		p.Stroke();
	}
}
