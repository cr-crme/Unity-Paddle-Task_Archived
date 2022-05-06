using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoControl : MonoBehaviour
{
    public VideoPlayer player;
    public AudioSource audioSource;
    public float postVideoDelay = 3f;
    public GameObject renderTarget;
    public UiManager paddleGame;
    public List<VideoData> videoDatas = new List<VideoData>();
    
    GlobalPauseHandler globalPauseHandler;

    private bool isVideoRunning = false;
    private int pauseLockKey;

    void Start()
    {
        isVideoRunning = GlobalPreferences.Instance.playVideo;

        if (isVideoRunning)
        {
            globalPauseHandler = GameObject.Find("[SteamVR]").GetComponent<GlobalPauseHandler>();
            float watingTime = 0;  // Start all coroutines video but wait in line the previous has ended
            for (int i = 0; i < videoDatas.Count; i++)
            {
                StartCoroutine(PracticeTimeCoroutine(watingTime, videoDatas[i]));
                float duration = (float)videoDatas[i].videoClip.length + videoDatas[i].postClipTime; 
                watingTime += duration;
            }
            StartCoroutine(PlaybackFinishedCoroutine(watingTime + .2f));
        }
        else
        {
            renderTarget.gameObject.SetActive(false);
        }
    }

    public void ForceVideoEndingNow()
    {
        if (!isVideoRunning)
        {
            return;
        }
        StopAllCoroutines();
        StartCoroutine(PlaybackFinishedCoroutine(0f));
    }

    IEnumerator PlaybackFinishedCoroutine(float delaySeconds)
    {
        yield return new WaitForSecondsRealtime(delaySeconds);
        Debug.Log("playback finished");
        renderTarget.gameObject.SetActive(false);
        player.Stop();
        audioSource.Stop();
        globalPauseHandler.Pause(pauseLockKey);
        isVideoRunning = false;
    }

    IEnumerator PracticeTimeCoroutine(float waitingBeforeStartTime, VideoData videoData)
    {
        yield return new WaitForSecondsRealtime(waitingBeforeStartTime);
        pauseLockKey = globalPauseHandler.Pause(-1, true, false);
        player.clip = videoData.videoClip;
        player.Play();
        audioSource.PlayOneShot(videoData.audioClip);
        yield return new WaitForSecondsRealtime((float)videoData.videoClip.length);
        globalPauseHandler.Resume(pauseLockKey);
        yield return new WaitForSecondsRealtime(videoData.postClipTime);
    }
}
