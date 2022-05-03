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
    public PaddleGame paddleGame;
    public List<VideoData> videoDatas = new List<VideoData>();
    
    GlobalControl globalControl;
    GlobalPauseHandler globalPauseHandler;

    private bool isVideoRunning = false;
    private int pauseLockKey = -1;

    void Start()
    {
        globalControl = GlobalControl.Instance;

        if (GlobalControl.Instance.playVideo)
        {
            isVideoRunning = true;
            globalPauseHandler = GameObject.Find("[SteamVR]").GetComponent<GlobalPauseHandler>();
            globalPauseHandler.Pause();
            pauseLockKey = globalPauseHandler.SetIndicatorVisibility(false, true);

            float total = 0;
            for (int i = 0; i < videoDatas.Count; i++)
            {
                StartCoroutine(PracticeTime(total, videoDatas[i]));
                float duration = (float)videoDatas[i].videoClip.length + videoDatas[i].postClipTime; 
                total += duration;
            }
            StartCoroutine(PlaybackFinished(total + .2f));
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
        StartCoroutine(PlaybackFinished(0f));
    }

    IEnumerator PlaybackFinished(float delaySeconds)
    {
        yield return new WaitForSecondsRealtime(delaySeconds);
        Debug.Log("playback finished");
        renderTarget.gameObject.SetActive(false);
        player.Stop();
        audioSource.Stop();
        globalControl.playVideo = false;
        globalPauseHandler.Pause();
        paddleGame.Initialize(false);
        globalPauseHandler.SetIndicatorVisibility(false, false, pauseLockKey);
        isVideoRunning = false;
    }

    IEnumerator PracticeTime(float start, VideoData videoData)
    {
        yield return new WaitForSecondsRealtime(start);
        player.clip = videoData.videoClip;
        player.Play();
        audioSource.PlayOneShot(videoData.audioClip);
        Debug.Log("playing video " + player.clip.name);
        yield return new WaitForSecondsRealtime((float)videoData.videoClip.length);
        player.Pause();
        globalPauseHandler.Resume();
        yield return new WaitForSecondsRealtime(videoData.postClipTime);
        globalPauseHandler.Pause();
    }
}
