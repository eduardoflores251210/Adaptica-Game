using UnityEngine;
public class DestroyWithTimer : MonoBehaviour
{
	[SerializeField]
	public int Timer;
	[SerializeField]
	public double AliveTime;

	public void Update()
	{
		AliveTime += Time.deltaTime;
		if (AliveTime >= Timer)
			Destroy(gameObject);
	}
}