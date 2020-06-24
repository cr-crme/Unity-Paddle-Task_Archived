﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoControl : MonoBehaviour
{
    public VideoPlayer player;
    public float postVideoDelay = 3f;
    public GameObject renderTarget;
    public bool editorTesting = false;
    public PaddleGame paddleGame;
    
    VideoClip video;

    void Start()
    {

        video = player.clip; 
        if (GlobalControl.Instance.playVideo)
		{
            float appStartDelay = (float)video.length + postVideoDelay;

#if UNITY_EDITOR
            if (editorTesting)
		    {
                appStartDelay = 10f;
		    }
#endif
            StartCoroutine(PlaybackFinished(appStartDelay));

            player.Play();
		}
		else
		{
            renderTarget.gameObject.SetActive(false);
        }
    }

    IEnumerator PlaybackFinished(float delaySeconds)
	{
        yield return new WaitForSeconds(delaySeconds);
        renderTarget.gameObject.SetActive(false);
        GlobalControl.Instance.recordingData = true;
        paddleGame.StartRecording();
	}

}
