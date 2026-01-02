using UnityEngine;

public class UnderwaterFog : MonoBehaviour
{
	[Header("Underwater Fog Settings")]
	public Color underwaterFogColor = new Color(0.05f, 0.3f, 0.5f);
	public float underwaterFogDensity = 0.06f;

	Color originalFogColor;
	float originalFogDensity;
	bool originalFogEnabled;

	Camera mainCam;

	void Start()
	{
		mainCam = Camera.main;

		originalFogEnabled = RenderSettings.fog;
		originalFogColor = RenderSettings.fogColor;
		originalFogDensity = RenderSettings.fogDensity;
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<Camera>() == mainCam)
		{
			EnableUnderwaterFog();
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (other.GetComponent<Camera>() == mainCam)
		{
			RestoreFog();
		}
	}

	void EnableUnderwaterFog()
	{
		RenderSettings.fog = true;
		RenderSettings.fogMode = FogMode.Exponential;
		RenderSettings.fogColor = underwaterFogColor;
		RenderSettings.fogDensity = underwaterFogDensity;
	}

	void RestoreFog()
	{
		RenderSettings.fog = originalFogEnabled;
		RenderSettings.fogColor = originalFogColor;
		RenderSettings.fogDensity = originalFogDensity;
	}
}