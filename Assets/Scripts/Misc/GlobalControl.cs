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

    // The number of paddles that the player is using.
    public int nbPaddles;

    // Target Line Height
    public TargetEnum.Height targetHeightPreference;

    // The current session
    public SessionType.Session session;

    // Test period of this instance
    public DifficultyChoice startingDifficulty;

    // Time limit for practise condition
    public int practiseMaxTrialTime;

    // Time per level for showcase condition
    public int showcaseTimePerCondition;

    public float timeConversionToMinute = 60f;

    // Time elapsed while game is not paused, in seconds 
    public float elapsedTime { get; private set; } = 0;
    // value affecting various metrics increasing randomness and general difficulty
    public int practiseStartingLevel;

    // Duration for which ball should be held before dropping upon reset
    public int ballResetHoverSeconds;

    // Allow game to be paused
    public bool paused = true;

    // Alter the speed at which physics and other updates occur
    private float _timescale = 1f;
    public float timescale { 
        get { return _timescale; }
        set { _timescale = value; } 
    }

    // Play video at the start
    public bool playVideo;

    // Selected environment
    public int environmentIndex;

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
            // Prevent from dividing by zero
            if (Time.timeScale == 0)
            {
                Debug.LogError("Division by zero found, please report this error");
                return;
            }
            
            elapsedTime += Time.deltaTime / Time.timeScale;
        }
    }
}
