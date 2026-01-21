using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class StartVideoPlayer : MonoBehaviour
{
    public VideoPlayer player;
    public bool Done = false;
    bool Prepared;
    bool Startd;
    // Start is called before the first frame update
    void Start()
    {
        
    }

	// Update is called once per frame
	void Update()
    {
        Prepared = player.isPrepared;
        if (!player.isPlaying && Prepared         && !Done)
        {
            player.Play();
            Startd = true;
            return;
        }
        if (Startd && !player.isPlaying)
        {
            Done = true;
        }
    }
}
