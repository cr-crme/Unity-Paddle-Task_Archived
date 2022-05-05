﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalPauseHandler : MonoBehaviour
{
    [SerializeField, Tooltip("The canvas for pausing")]
    public GameObject pauseIndicator;

    [SerializeField]
    private Ball ball;

    // If any game object requested the indicator to be block (sort of mutex)
    int lockKey = -1;


    public void TogglePause()
    {
        if (GlobalControl.Instance.paused == true)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        if (lockKey > -1)
        {
            Debug.Log("Pausing requested, but it is currently lock");
            return;
        }

        GlobalControl.Instance.paused = true;
        Time.timeScale = 0;
        ball.TriggerPause();

        SetIndicatorVisibility(true);
    }

    public void Resume()
    {
        if (lockKey > -1)
        {
            Debug.Log("Resuming requested, but it is currently lock");
            return;
        }

        GlobalControl.Instance.paused = false;
        Time.timeScale = ball.inHoverMode ? 1f : GlobalControl.Instance.timescale;
        ball.TriggerResume();
        SetIndicatorVisibility(false);
    }

    public int SetIndicatorVisibility(bool _visible, bool _generateLock = false, int _lockKey = -1)
    {
        // If there is an activelock
        if (lockKey > 0)
        {
            // Refuse the modification if the wrong key is provided
            if (_lockKey != lockKey)
            {
                Debug.Log("Pause indicator requested, but it is currently lock");
                return -1;
            }
        }
        lockKey = -1;

        if (_generateLock)
        {
            lockKey = Random.Range(1, 1000);
        }

        pauseIndicator.SetActive(_visible);
        return lockKey;
    }
}
