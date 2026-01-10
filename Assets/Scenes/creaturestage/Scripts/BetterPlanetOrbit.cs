using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class BetterPlanetOrbit : MonoBehaviour
{
	public Axis RotationAxis;
	public Axis OrbitAxis;
	public float OrbitSpeed = 10f;
	public float OrbitDistance = 5f;
	public float RotationSpeed = 6f;
	public Transform Parent;

	void Start()
	{
		if (Parent == null)
		{
			Debug.Log("AEAEAEAEAEAEAEAEAEAE");
			Destroy(this);
			return;
		}

		Vector3 orbitAxis = AxisToVector(OrbitAxis);
		if (orbitAxis.sqrMagnitude < 0.0001f) orbitAxis = Vector3.up;
		orbitAxis.Normalize();

		Vector3 radial = transform.position - Parent.position;
		if (radial.sqrMagnitude < 0.0001f)
		{
			Vector3 perp = Vector3.Cross(orbitAxis, Vector3.up);
			if (perp.sqrMagnitude < 0.0001f) perp = Vector3.Cross(orbitAxis, Vector3.right);
			perp.Normalize();
			transform.position = Parent.position + perp * OrbitDistance;
		}
		else
		{
			Vector3 radialNorm = radial.normalized;
			if (Mathf.Abs(Vector3.Dot(radialNorm, orbitAxis)) > 0.999f)
			{
				Vector3 perp = Vector3.Cross(orbitAxis, Vector3.up);
				if (perp.sqrMagnitude < 0.0001f) perp = Vector3.Cross(orbitAxis, Vector3.right);
				perp.Normalize();
				transform.position = Parent.position + perp * OrbitDistance;
			}
			else
			{
				transform.position = Parent.position + radialNorm * OrbitDistance;
			}
		}
	}

	void Update()
	{
		if (Parent == null) return;

		Vector3 orbitAxis = AxisToVector(OrbitAxis);
		if (orbitAxis.sqrMagnitude < 0.0001f) orbitAxis = Vector3.up;
		orbitAxis.Normalize();

		if (OrbitSpeed > 0f)
		{
			float angleThisFrame = 360f * Time.deltaTime / OrbitSpeed;
			Vector3 dir = transform.position - Parent.position;
			dir = Quaternion.AngleAxis(angleThisFrame, orbitAxis) * dir;
			dir = dir.normalized * OrbitDistance;
			transform.position = Parent.position + dir;
		}
		else
		{
			Vector3 dir = (transform.position - Parent.position).normalized;
			transform.position = Parent.position + dir * OrbitDistance;
		}

		Vector3 rotationAxis = AxisToVector(RotationAxis);
		if (rotationAxis.sqrMagnitude < 0.0001f) rotationAxis = Vector3.up;
		rotationAxis.Normalize();

		if (RotationSpeed > 0f)
		{
			float rotAngleThisFrame = 360f * Time.deltaTime / RotationSpeed;
			transform.Rotate(rotationAxis, rotAngleThisFrame, Space.World);
		}
	}

	public Vector3 AxisToVector(Axis axis)
	{
		Vector3 res = Vector3.zero;
		if ((axis & Axis.X) != 0) res += Vector3.right;
		if ((axis & Axis.Y) != 0) res += Vector3.up;
		if ((axis & Axis.Z) != 0) res += Vector3.forward;
		return res;
	}
}
