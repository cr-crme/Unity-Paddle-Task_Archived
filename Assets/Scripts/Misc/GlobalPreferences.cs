using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Stores calibration data for trial use in a single place.
/// </summary>
public class GlobalPreferences : MonoBehaviour 
{

    // The single instance of this class
    public static GlobalPreferences Instance;

    // participant ID to differentiate data files
    public string participantID { get; private set; } = "Default";
    public void SetParticipantID(string _value)
    {
        participantID = _value == "" ? "Default" : _value;
    }

    // The number of paddles that the player is using.
    public PaddleChoice paddleChoice { get; private set; }
    public void SetPaddleChoice(PaddleChoice _value) { paddleChoice = _value; }


    // Target Line Height
    public TargetEnum.Height targetBaseHeight { get; private set; }
    public void SetTargetBaseHeight(TargetEnum.Height _value) { targetBaseHeight = _value; }


    // The current session
    public SessionType.Session session { get; private set; }
    public void SetSession(SessionType.Session _value) { session = _value; }

    // Test period of this instance
    public DifficultyChoice startingDifficulty { get; private set; }
    public void SetStartingDifficulty(DifficultyChoice _value) { startingDifficulty = _value; }

    // Time limit for practise condition
    public int practiseMaxTrialTime { get; private set; }
    public void SetPractiseMaxTrialTime(int _value) { practiseMaxTrialTime = _value; }

    // Time per level for showcase condition
    public int showcaseTimePerCondition { get; private set; }
    public void SetShowcaseTimePerCondition(int _value) { showcaseTimePerCondition = _value; }

    public float timeConversionToMinute { get; private set; } = 60f;

    // value affecting various metrics increasing randomness and general difficulty
    public int practiseStartingLevel { get; private set; }
    public void SetPractiseStartingLevel(int _value) { practiseStartingLevel = _value; }

    // Duration for which ball should be held before dropping upon reset
    public int ballResetHoverSeconds { get; private set; }
    public void SetBallResetHoverSeconds(int _value) { ballResetHoverSeconds = _value; }

    // Play video at the start
    public bool playVideo { get; private set; }
    public void SetPlayVideo(bool _value) { playVideo = _value; }

    // Selected environment
    public int environmentIndex { get; private set; }
    public void SetEnvironmentIndex(int _value) { environmentIndex = _value; }


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
}
