using UnityEngine;
using UnityEngine.UIElements;

public class ToolTipManipulator : Manipulator
{

	private VisualElement element;
	private Label label;
	private const float Padding = 8f;
	private const float Gap = 6f;

	protected override void RegisterCallbacksOnTarget()
	{
		target.RegisterCallback<MouseEnterEvent>(MouseIn);
		target.RegisterCallback<MouseLeaveEvent>(MouseOut);
	}

	protected override void UnregisterCallbacksFromTarget()
	{
		target.UnregisterCallback<MouseEnterEvent>(MouseIn);
		target.UnregisterCallback<MouseLeaveEvent>(MouseOut);
	}

	private void MouseIn(MouseEnterEvent e)
	{
		if (element == null)
		{
			element = new VisualElement();
			element.style.position = Position.Absolute;
			element.style.backgroundColor = Color.blue;
			element.style.visibility = Visibility.Hidden;
			element.pickingMode = PickingMode.Ignore;

			label = new Label(target.tooltip);
			label.style.color = Color.white;
			element.Add(label);

			var root = target.panel?.visualTree;
			if (root == null)
				return;

			root.Add(element);

			// Reposiciona cuando ya tenga tamaño real
			element.RegisterCallback<GeometryChangedEvent>(_ => UpdateTooltipPosition());
		}

		element.style.visibility = Visibility.Visible;
		label.text = target.tooltip;
		UpdateTooltipPosition();
		element.BringToFront();
	}

	private void MouseOut(MouseLeaveEvent e)
	{
		if (element != null)
			element.style.visibility = Visibility.Hidden;
	}

	private void UpdateTooltipPosition()
	{
		if (element == null || target?.panel == null)
			return;

		var root = target.panel.visualTree;

		// Importante: usar el tamaño real del tooltip ya calculado
		float tooltipW = element.resolvedStyle.width;
		float tooltipH = element.resolvedStyle.height;

		if (float.IsNaN(tooltipW) || float.IsNaN(tooltipH))
			return;

		// Convertimos el rect del target al espacio local del root
		Rect targetInRoot = root.WorldToLocal(target.worldBound);

		float rootW = root.layout.width;
		float rootH = root.layout.height;

		float x = targetInRoot.xMin;
		float y = targetInRoot.yMax + Gap;

		// Si se sale por la derecha, lo movemos hacia la izquierda
		if (x + tooltipW + Padding > rootW)
			x = Mathf.Max(Padding, rootW - tooltipW - Padding);

		// Si se sale por abajo, lo colocamos arriba del control
		if (y + tooltipH + Padding > rootH)
			y = Mathf.Max(Padding, targetInRoot.yMin - tooltipH - Gap);

		// Clamp final para no salir del panel
		x = Mathf.Clamp(x, Padding, Mathf.Max(Padding, rootW - tooltipW - Padding));
		y = Mathf.Clamp(y, Padding, Mathf.Max(Padding, rootH - tooltipH - Padding));

		element.style.left = x;
		element.style.top = y;
	}
}