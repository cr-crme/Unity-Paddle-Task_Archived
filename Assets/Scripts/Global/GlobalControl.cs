// using NUnit.Framework.Internal;
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

    // participant ID to differentiate data files
    public string participantID = "";

    // The number of paddles that the player is using. Usually 1 or 2.
    public int nbPaddles = 1;

    // Target Line Height
    public TargetEnum.Height targetHeightPreference = TargetEnum.Height.EYE_LEVEL;

    // Target Line Width Acceptance Threshold
    public float targetWidth = 0.05f;

    // Test period of this instance
    public SessionType.Session session = SessionType.Session.PRACTISE;

    // Degrees of Freedom for ball bounce for this instance
    public float degreesOfFreedom = 90;

    // Time limit for practise condition
    public int practiseMaxTrialTime = 0;

    // Time per level for showcase condition
    public int showcaseTimePerCondition = 0;

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


    // value affecting various metrics increasing randomness and general difficulty
    public int level = 1;

    // Play video at the start
    public bool playVideo = false;

    // Selected environment
    public int environmentIndex = 0;

    // all environment prefabs
    public List<GameObject> environments = new List<GameObject>();

    /// <summary>
    /// Assign instance to this, or destroy it if Instance already exits and is not this instance.
    /// </summary>
    void Awake()
    {
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
        return practiseMaxTrialTime * 60.0f;
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
