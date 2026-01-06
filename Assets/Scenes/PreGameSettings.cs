using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class PreGameSettings : MonoBehaviour
{
	public Canvas Menu;
	public Toggle ToggleBloom;
	public StarVisualizer visualizer;
	private List<Volume> volumes = new List<Volume>();

	void Awake()
	{


		if (!Debug.isDebugBuild)
		{
			Destroy(this);
			return;
		}

		DontDestroyOnLoad(gameObject);
		DontDestroyOnLoad(Menu.gameObject);

		if (visualizer != null)
		{
			visualizer.enabled = false;
			visualizer.Generator.enabled = false;
		}

		if (ToggleBloom != null)
			ToggleBloom.onValueChanged.AddListener(SetBloom);

		SceneManager.sceneLoaded += OnSceneLoaded;

		// Inicializar Volumes de la escena actual
		FindAllVolumesInScene();
	}

	void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		FindAllVolumesInScene();
		if (ToggleBloom != null)
			SetBloom(ToggleBloom.isOn);
	}

	void FindAllVolumesInScene()
	{
		volumes.Clear();
		volumes.AddRange(FindObjectsOfType<Volume>());
	}

	void SetBloom(bool isActive)
	{
		foreach (var volume in volumes)
		{
			if (volume != null && volume.profile != null)
			{
				if (volume.profile.TryGet<Bloom>(out var bloom))
				{
					bloom.active = isActive;
				}
			}
		}
	}
}
