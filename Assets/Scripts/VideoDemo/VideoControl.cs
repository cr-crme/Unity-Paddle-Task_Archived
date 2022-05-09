using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoControl : MonoBehaviour
{
    [SerializeField] private TrialsManager trialsManager;
    [SerializeField] private UiManager uiManager;

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
            uiManager.ToggleTimerCountdownCanvas(false);
            globalPauseHandler = GameObject.Find("[SteamVR]").GetComponent<GlobalPauseHandler>();
            float watingTime = 0;  // Start all coroutines video but wait in line the previous has ended
            for (int i = 0; i < videoDatas.Count; i++)
            {
                StartCoroutine(PracticeTimeCoroutine(watingTime, videoDatas[i], i == 0));
                float duration = (float)videoDatas[i].videoClip.length + videoDatas[i].postClipTime + 2f;  // 1f for forcing ball on floor
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

    IEnumerator PlaybackFinishedCoroutine(float _delaySeconds)
    {
        yield return new WaitForSecondsRealtime(_delaySeconds);
        renderTarget.gameObject.SetActive(false);
        player.Stop();
        audioSource.Stop();
        globalPauseHandler.Resume(pauseLockKey);
        isVideoRunning = false;
        uiManager.QuitTask(trialsManager);
    }

    IEnumerator PracticeTimeCoroutine(float _waitingBeforeStartTime, VideoData _videoData, bool _firstTime)
    {
        // Wait until previous video is finished and the ball is droped
        yield return new WaitForSecondsRealtime(_waitingBeforeStartTime);

        // Prepare a new trial
        uiManager.ToggleTimerCountdownCanvas(false);
        trialsManager.ForceLevelChanging(_videoData.difficulty);

        if (!_firstTime)  // There is already a trial started by the ball itself when it is created so no need to start one
            trialsManager.StartNewTrial();

        // Play the video
        pauseLockKey = globalPauseHandler.Pause(-1, true, false);
        player.clip = _videoData.videoClip;
        player.Play();
        audioSource.PlayOneShot(_videoData.audioClip);
        yield return new WaitForSecondsRealtime((float)_videoData.videoClip.length);

        // Give some practise time
        globalPauseHandler.Resume(pauseLockKey);
        if (_firstTime)
        {
            // Since we did started the trial ourselves it does not reset the countdown visual and game is not paused
            uiManager.ToggleTimerCountdownCanvas(true);
            globalPauseHandler.Pause();
        }
        yield return new WaitForSecondsRealtime(_videoData.postClipTime);
        
        // When the practise is over, force the ball to drop
        trialsManager.ForceEndOfTrial(false);
    }
}
