using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSetter : MonoBehaviour
{
    public List<MusicChanceSettings> MusicChances; //asignado desde el inspector, con la musica y su probabilidad de ser elegida
    public AudioSource MusicSource; //asignado desde el inspector, el audio source que se usara para reproducir la musica
									// Start is called before the first frame update
	void Start()
    {
        if (MusicSource == null)
            return;
       //aqui se selecciona la musica a reproducir segun las probabilidades asignadas en el inspector
        float totalChance = 0f;
        foreach (var musicChance in MusicChances)
        {
            totalChance += musicChance.Chance;
        }
        float randomValue = UnityEngine.Random.Range(0f, totalChance);
        float cumulativeChance = 0f;
        foreach (var musicChance in MusicChances)
        {
            cumulativeChance += musicChance.Chance;
            if (randomValue <= cumulativeChance)
            {
                MusicSource.clip = musicChance.Music;
                MusicSource.volume = musicChance.DefaultVolume;
                Debug.Log($"Selected music: {musicChance.Music.name} with volume {musicChance.DefaultVolume}");
                break;
            }
		}
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
[Serializable]
public class MusicChanceSettings //no es struct porque unity odia structs.
{
    public float DefaultVolume;
    public float Chance;
	public AudioClip Music;
}