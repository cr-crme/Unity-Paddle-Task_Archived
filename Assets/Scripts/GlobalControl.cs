﻿// using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Stores calibration data for trial use in a single place.
/// </summary>
public class GlobalControl : MonoBehaviour 
{

    // The single instance of this class
    public static GlobalControl Instance;

    public enum ExplorationMode { NONE, TASK, FORCED };

    public ExplorationMode explorationMode = ExplorationMode.NONE;

    // participant ID to differentiate data files
    public string participantID = "";

    // The number of paddles that the player is using. Usually 1 or 2.
    public int numPaddles = 1;

    // The condition of this instance
    public TaskType.Condition condition = TaskType.Condition.ENHANCED;

    // The Exploration condition of this instance (controls randomized physics)
    public TaskType.ExpCondition expCondition = TaskType.ExpCondition.NORMAL;

    // Target Line Height
    public TaskType.TargetHeight targetHeightPreference = TaskType.TargetHeight.DEFAULT;

    // Target Line Success Threshold
    public float targetRadius = 0.05f;

    // Test period of this instance
    public TaskType.Session session = TaskType.Session.BASELINE;

    // Degrees of Freedom for ball bounce for this instance
    public float degreesOfFreedom = 90;

    // Time limit in minutes after beginning, after which the game will end
    public int maxTrialTime = 0;

    // Time limit in minutes after beginning, after which the game will end
    public int maxBaselineTrialTime = 10;

    // Time limit in minutes after beginning, after which the game will end
    public int maxModerate1TrialTime = 10;

    // Time limit in minutes after beginning, after which the game will end
    public int maxMaximalTrialTime = 10;

    // Time limit in minutes after beginning, after which the game will end
    public int maxModerate2TrialTime = 10;

    // Time elapsed while game is not paused, in seconds 
    public float timeElapsed = 0;

    // Duration for which ball should be held before dropping upon reset
    public int ballResetHoverSeconds = 3;

    // Allow game to be paused
    public bool paused = true;

    // Alter the speed at which physics and other updates occur
    private float _timescale = 1f;
    public float timescale { 
        get { return _timescale; } set { _timescale = value;  Time.timeScale = value; } 
    }

    // Will hide the target height and alter behaviors so they are affected by consecutive hits only
    public bool targetHeightEnabled = true;

    // value affecting various metrics increasing randomness and general difficulty
    public int difficulty = 1;

    // Play video at the start
    public bool playVideo = false;

    // Selected enviornment
    public int environmentOption = 0;

    // all environment prefabs
    public List<GameObject> environments = new List<GameObject>();

    // sometimes data should not be recorded, tracked here.
    public bool recordingData = true;
    
    // some conditions affect this offset
    [NonSerialized]
    public float targetLineHeightOffset = 0;

    /// <summary>
    /// Assign instance to this, or destroy it if Instance already exits and is not this instance.
    /// </summary>
    void Awake()
    {
        if (condition == TaskType.Condition.ENHANCED)
        {
            explorationMode = ExplorationMode.FORCED;
        }

        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!paused)
        {
            if (Time.timeScale == 0)
            {
                Debug.Log($"{nameof(Time.timeScale)}={Time.timeScale}");
                return;
            }
            
            timeElapsed += (Time.deltaTime * (1/Time.timeScale));
            // Debug.Log("not paused: " + timeElapsed);
        }
        else
		{
            // Debug.Log("paused: " + Time.time);
		}

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Application.Quit();
        }
    }
    
    public float GetTimeLimitSeconds()
    {
        return maxTrialTime * 60.0f;
    }

    public float GetTimeElapsed()
    {
        return timeElapsed;
    }

    public void ResetTimeElapsed()
	{
        timeElapsed = 0;
	}
}