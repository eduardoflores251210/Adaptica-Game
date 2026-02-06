using UnityEngine;
public class DestroyWithTimer : MonoBehaviour
{
	[SerializeField] //redundante pero bueno
	public int Timer;
	[SerializeField] //redundante pero bueno
	public double AliveTime;

	public void Update()
	{
		AliveTime += Time.deltaTime;
		if (AliveTime >= Timer)
			Destroy(gameObject); // autodestruccion
	}
}