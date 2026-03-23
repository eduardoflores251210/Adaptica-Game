using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.ReorderableList.Internal;
using UnityEngine;
using UnityEngine.UIElements;

public class ToolTipManipulator : Manipulator
{

	private VisualElement element;
	public ToolTipManipulator()
	{
	}

	protected override void RegisterCallbacksOnTarget()
	{
		target.RegisterCallback<MouseEnterEvent>(MouseIn);
		target.RegisterCallback<MouseOutEvent>(MouseOut);
	}

	protected override void UnregisterCallbacksFromTarget()
	{
		target.UnregisterCallback<MouseEnterEvent>(MouseIn);
		target.UnregisterCallback<MouseOutEvent>(MouseOut);
	}

	private void MouseIn(MouseEnterEvent e)
	{
		if (element == null)
		{
			element = new VisualElement();
			element.style.backgroundColor = Color.blue;
			element.style.position = Position.Absolute;
			element.style.left = this.target.worldBound.center.x;
			element.style.top = this.target.worldBound.yMin;

			var label = new Label(this.target.tooltip);
			label.style.color = Color.white;

			element.Add(label);

			// Reemplazo de UiHelper.FindRootElement
			var root = target.panel?.visualTree;
			if (root != null)
				root.Add(element);
		}

		element.style.visibility = Visibility.Visible;
		element.BringToFront();
	}

	private void MouseOut(MouseOutEvent e)
	{
		element.style.visibility = Visibility.Hidden;
	}
}