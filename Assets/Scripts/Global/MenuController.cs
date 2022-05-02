using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


/// <summary>
/// Holds functions for responding to and recording preferences on menu.
/// </summary>
public class MenuController : MonoBehaviour {

    private GlobalControl globalControl;
    [SerializeField] private GameObject practiseCanvas;
    [SerializeField] private GameObject showcaseCanvas;

    // Loads all saved preferences to the main menu
    private delegate void LoadCallback(bool resetToDefault);
    // Default value / callback for Preference loading / Action to setting
    Dictionary<string, (object, LoadCallback)> preferenceList;

    #region Initialization
    /// <summary>
    /// Disable VR for menu scene and hide warning text until needed.
    /// </summary>
    void Start()
    {
        globalControl = GlobalControl.Instance;

        preferenceList = new Dictionary<string, (object, LoadCallback)>(){
            { "nbPaddles", (GetNbPaddles(), LoadNbPaddlesToMenu) },
            { "environment", (GetEnvironment(), LoadEnvironmentToMenu) },
            { "session", (GetRecordSession(), LoadSessionToMenu) },
            { "practise_totalTime", (GetPractiseTotalTime(), LoadPractiseTotalTimeToMenu) },
            { "showcase_timePerTrial", (GetShowcaseTimePerTrial(), LoadShowcaseTimePerTrialToMenu) },
            { "practise_difficulty", (GetPractiseDifficulty(), LoadPractiseDifficulty) },
            { "practise_level", (GetPractiseStartingLevel(), LoadPractiseStartingLevelToMenu) },
            { "showcase_toggleVideo", (GetShowcaseVideo(), LoadShowcaseVideoToMenu) },
            { "targetHeight", (GetTargetHeight(), LoadTargetHeightToMenu) },
            { "targetWidth", (GetTargetWidth(), LoadTargetWidthToMenu) },
            { "hoverTime", (GetBallHoverTime(), LoadBallHoverTimeToMenu) },
        };

        // disable VR settings for menu scene
        UnityEngine.XR.XRSettings.enabled = false;

        // Load saved preferences
        LoadAllPreferences(false);
        UpdateConditionalUIObjects();
    }
    public void LoadAllPreferences(bool resetToDefault)
    {
        foreach (KeyValuePair<string, (object, LoadCallback)> callback in preferenceList)
        {
            callback.Value.Item2(resetToDefault);  // Call the Load callback
        }
    }
    // Clears all saved main menu preferences
    public void ResetPlayerPrefsToDefault()
    {
        PlayerPrefs.DeleteAll();
        LoadAllPreferences(true);
    }

    void UpdateConditionalUIObjects()
    {
        ShowProperSessionCanvas();
    }
    #endregion

    #region Finalization
    public void NextScene()
    {
        if (globalControl.nbPaddles == 1)
        {
            SceneManager.LoadScene("Paddle");
        }
        else
        {
            SceneManager.LoadScene("Paddle 2");
        }
    }

    /// <summary>
    /// Re-enable VR when this script is disabled (since it is disabled on moving into next scene).
    /// </summary>
    void OnDisable()
    {
        UnityEngine.XR.XRSettings.enabled = true;
    }
    #endregion

    #region GenericInformation
    [SerializeField] private TMP_Dropdown nbPaddles;
    [SerializeField] private TMP_Dropdown environment;

    /// <summary>
    /// Records an alphanumeric participant ID. Hit enter to record. May be entered multiple times
    /// but only last submission is used. Called using a dynamic function in the inspector
    /// of the textfield object.
    /// </summary>
    /// <param name="arg0"></param>
    public void RecordID(string arg0)
    {
        globalControl.participantID = arg0;
    }

    public void RecordNbPaddles(int _value)
    {
        globalControl.nbPaddles = _value + 1;  // 0-based
        SaveNbPaddles(_value);
    }
    private void SetNbPaddles(int _value)
    {
        nbPaddles.value =_value;
    }
    private int GetNbPaddles()
    {
        return nbPaddles.value;
    }
    private void SaveNbPaddles(int _value)
    {
        PlayerPrefs.SetInt("nbPaddles", _value);
        PlayerPrefs.Save();
    }
    private void LoadNbPaddlesToMenu(bool resetToDefault)
    {
        int _value;
        if (resetToDefault)
            _value = (int)preferenceList["nbPaddles"].Item1;
        else if (PlayerPrefs.HasKey("nbPaddles"))
            _value = PlayerPrefs.GetInt("nbPaddles");
        else
            return;

        RecordNbPaddles(_value);
        SetNbPaddles(_value);
    }

    public void RecordEnvironment(int _value)
    {
        globalControl.environmentIndex = _value;
        SaveEnvironment(_value);
    }
    private void SetEnvironment(int _value)
    {
        environment.value = _value;
    }
    private int GetEnvironment()
    {
        return environment.value;
    }
    private void SaveEnvironment(int _value)
    {
        PlayerPrefs.SetInt("environment", _value);
        PlayerPrefs.Save();
    }
    private void LoadEnvironmentToMenu(bool resetToDefault)
    {
        int _value;
        if (resetToDefault)
            _value = (int)preferenceList["environment"].Item1;
        else if (PlayerPrefs.HasKey("environment"))
            _value = PlayerPrefs.GetInt("environment");
        else
            return;

        RecordEnvironment(_value);
        SetEnvironment(_value);
    }
    #endregion

    #region SessionType
    [SerializeField] private TMP_Dropdown session;
    // Records the Session from the dropdown menu
    public void RecordSession(int _value)
    {
        globalControl.session = (SessionType.Session)_value;
        SaveSession(_value);
        UpdateConditionalUIObjects();
    }
    private void SetRecordSession(int _value)
    {
        session.value = _value;
    }
    private int GetRecordSession()
    {
        return session.value;
    }
    public void SaveSession(int menuInt)
    {
        PlayerPrefs.SetInt("session", menuInt);
        PlayerPrefs.Save();
    }
    private void LoadSessionToMenu(bool resetToDefault)
    {
        int _value;
        if (resetToDefault)
            _value = (int)preferenceList["session"].Item1;
        else if (PlayerPrefs.HasKey("session"))
            _value = PlayerPrefs.GetInt("session");
        else
            return;

        RecordSession(_value);
        SetRecordSession(_value);
    }
    #endregion

    #region TrialTime
    [SerializeField] private Slider practiseTotalTime;
    [SerializeField] private TextMeshProUGUI practiseTotalTimeText;
    [SerializeField] private TMP_Dropdown showcaseTimePerTrial;
    public void RecordPractiseTotalTime(float _value)
    {
        globalControl.practiseMaxTrialTime = (int)_value;
        UpdatePractiseTotalTimeText((int)_value);
        SavePractiseTotalTime((int)_value);
    }
    private void UpdatePractiseTotalTimeText(int _value)
    {
        if (_value != 0)
        {
            practiseTotalTimeText.text = $"Time: {_value} minutes";
        }
        else
        {
            practiseTotalTimeText.text = $"Time: No Limit";
        }
    }
    private void SetPractiseTotalTime(int _value)
    {
        practiseTotalTime.value = _value;
    }
    private int GetPractiseTotalTime()
    {
        return (int)practiseTotalTime.value;
    }
    private void SavePractiseTotalTime(int _value)
    {
        PlayerPrefs.SetInt("practise_totalTime", _value);
        PlayerPrefs.Save();
    }
    private void LoadPractiseTotalTimeToMenu(bool resetToDefault)
    {
        int _value;
        if (resetToDefault)
            _value = (int)preferenceList["practise_totalTime"].Item1;
        else if (PlayerPrefs.HasKey("practise_totalTime"))
            _value = PlayerPrefs.GetInt("practise_totalTime");
        else
            return;

        RecordPractiseTotalTime(_value);
        SetPractiseTotalTime(_value);
    }

    public void RecordShowcaseTimePerTrial(int _value)
    {
        globalControl.showcaseTimePerCondition = _value;
        SaveShowcaseTimePerTrial(_value);
    }
    private void SetShowcaseTimePerTrial(int _value)
    {
        showcaseTimePerTrial.value = _value;
    }
    private int GetShowcaseTimePerTrial()
    {
        return showcaseTimePerTrial.value;
    }
    private void SaveShowcaseTimePerTrial(int _value)
    {
        PlayerPrefs.SetInt("showcase_timePerTrial", _value);
        PlayerPrefs.Save();
    }
    private void LoadShowcaseTimePerTrialToMenu(bool resetToDefault)
    {
        int _value;
        if (resetToDefault)
            _value = (int)preferenceList["showcase_timePerTrial"].Item1;
        else if (PlayerPrefs.HasKey("showcase_timePerTrial"))
            _value = PlayerPrefs.GetInt("showcase_timePerTrial");
        else
            return;

        RecordShowcaseTimePerTrial(_value);
        SetShowcaseTimePerTrial(_value);
    }

    private void ShowProperSessionCanvas()
    {
        if (globalControl.session == SessionType.Session.PRACTISE)
        {
            practiseCanvas.SetActive(true);
            showcaseCanvas.SetActive(false);

        }
        else if (globalControl.session == SessionType.Session.SHOWCASE)
        {
            showcaseCanvas.SetActive(true);
            practiseCanvas.SetActive(false);

        }
        else
        {
            Debug.LogError("Not implemented Session");
        }
    }
    #endregion


    #region Difficulty
    [SerializeField] private TMP_Dropdown practiseDifficulty;
    public void RecordPractiseDifficulty(int _value)
    {
        globalControl.practiseDifficulty = (DifficultyChoice)_value;
        SavePractiseDifficulty(_value);
    }
    private void SetPractiseDifficulty(int _value)
    {
        practiseDifficulty.value = _value;
    }
    private int GetPractiseDifficulty()
    {
        return practiseDifficulty.value;
    }
    private void SavePractiseDifficulty(int _value)
    {
        PlayerPrefs.SetInt("practise_difficulty", _value);
        PlayerPrefs.Save();
    }
    private void LoadPractiseDifficulty(bool resetToDefault)
    {
        int _value;
        if (resetToDefault)
            _value = (int)preferenceList["practise_difficulty"].Item1;
        else if (PlayerPrefs.HasKey("practise_difficulty"))
            _value = PlayerPrefs.GetInt("practise_difficulty");
        else
            return;

        RecordPractiseDifficulty(_value);
        SetPractiseDifficulty(_value);
    }
    #endregion

    #region Level
    [SerializeField] private Slider practiseStartingLevel;
    [SerializeField] private TextMeshProUGUI practiseStartingLevelText;
    public void RecordPractiseStartingLevel(float _value)
    {
        globalControl.level = (int)_value;
        UpdatePractiseStartingLevelText((int)_value);
        SavePractiseStartingLevel((int)_value);
    }
    private void UpdatePractiseStartingLevelText(int _value)
    {
        practiseStartingLevelText.text = $"Level {_value}";
    }
    private void SetPractiseStartingLevel(int _value)
    {
        practiseStartingLevel.value = _value;
    }
    private int GetPractiseStartingLevel()
    {
        return (int)practiseStartingLevel.value;
    }
    private void SavePractiseStartingLevel(int _value)
    {
        PlayerPrefs.SetInt("practise_level", _value);
        PlayerPrefs.Save();
    }
    private void LoadPractiseStartingLevelToMenu(bool resetToDefault)
    {
        int _value;
        if (resetToDefault)
            _value = (int)preferenceList["practise_level"].Item1;
        else if (PlayerPrefs.HasKey("practise_level"))
            _value = PlayerPrefs.GetInt("practise_level");
        else
            return;

        RecordPractiseStartingLevel(_value);
        SetPractiseStartingLevel(_value);
    }
    #endregion

    #region VideoTutorial
    [SerializeField] private Toggle showcaseVideo;
    public void RecordShowcaseVideo(bool _value)
    {
        globalControl.playVideo = _value;
        SaveShowcaseVideo(_value);
    }
    private void SetShowcaseVideo(bool _value)
    {
        showcaseVideo.isOn = _value;
    }
    private bool GetShowcaseVideo()
    {
        return showcaseVideo.isOn;
    }
    private void SaveShowcaseVideo(bool _value)
    {
        PlayerPrefs.SetInt("showcase_toggleVideo", _value ? 1 : 0);
        PlayerPrefs.Save();
    }
    private void LoadShowcaseVideoToMenu(bool resetToDefault)
    {
        bool _value;
        if (resetToDefault)
            _value = (bool)preferenceList["showcase_toggleVideo"].Item1;
        else if (PlayerPrefs.HasKey("showcase_toggleVideo"))
            _value = PlayerPrefs.GetInt("showcase_toggleVideo") == 1 ? true : false;
        else
            return;

        RecordShowcaseVideo(_value);
        SetShowcaseVideo(_value);
    }
    #endregion

    #region Target
    [SerializeField] private TMP_Dropdown targetHeight;
    [SerializeField] private Slider targetWidth;
    [SerializeField] private TextMeshProUGUI targetWidthText;
    const float INCHES_PER_METER = 39.37f;
    public void RecordTargetHeight(int _value)
    {
        globalControl.targetHeightPreference = (TargetEnum.Height)_value;
        SaveTargetHeight(_value);
    }
    private void SetTargetHeight(int _value)
    {
        targetHeight.value = _value;
    }
    private int GetTargetHeight()
    {
        return targetHeight.value;
    }
    private void SaveTargetHeight(int _value)
    {
        PlayerPrefs.SetInt("targetHeight", _value);
        PlayerPrefs.Save();
    }
    private void LoadTargetHeightToMenu(bool resetToDefault)
    {
        int _value;
        if (resetToDefault)
            _value = (int)preferenceList["targetHeight"].Item1;
        else if (PlayerPrefs.HasKey("targetHeight"))
            _value = PlayerPrefs.GetInt("targetHeight");
        else
            return;

        RecordTargetHeight(_value);
        SetTargetHeight(_value);
    }

    public void RecordTargetWidth(float _value)
    {
        globalControl.targetWidth = ComputeTargetWidthInMeter(_value);
        UpdateTargetWidthText(_value);
        SaveTargetWidth(_value);
    }
    private void UpdateTargetWidthText(float _value)
    {
        targetWidthText.text =
            $"+/- {ComputeTargetWidthInInches(_value).ToString("0.0")} in.\n" +
            $"({_value.ToString("0.0")} in. total)";
    }
    private float ComputeTargetWidthInInches(float _value)
    {
        return _value * 0.5f;  // each notch is 0.5 inches 
    }
    private float ComputeTargetWidthInMeter(float _value)
    {
        return ComputeTargetWidthInInches(_value) / INCHES_PER_METER;
    }
    private void SetTargetWidth(float _value)
    {
        targetWidth.value = _value;
    }
    private float GetTargetWidth()
    {
        return targetWidth.value;
    }
    private void SaveTargetWidth(float _value)
    {
        PlayerPrefs.SetFloat("targetWidth", _value);
        PlayerPrefs.Save();
    }
    private void LoadTargetWidthToMenu(bool resetToDefault)
    {
        float _value;
        if (resetToDefault)
            _value = (float)preferenceList["targetWidth"].Item1;
        else if (PlayerPrefs.HasKey("targetWidth"))
            _value = PlayerPrefs.GetFloat("targetWidth");
        else
            return;

        RecordTargetWidth(_value);
        SetTargetWidth(_value);
    }
    #endregion

    #region Ball
    [SerializeField] private Slider ballHoverTime;
    [SerializeField] private TextMeshProUGUI ballHoverTimeText;
   
    public void RecordBallHoverTime(float _value)
    {
        globalControl.ballResetHoverSeconds = (int)_value;
        UpdateBallHoverTimeText(_value);
        SaveBallHoverTime(_value);
    }
    private void UpdateBallHoverTimeText(float _value)
    {
        ballHoverTimeText.text = $"{_value} seconds";
    }
    private void SetBallHoverTime(float _value)
    {
        ballHoverTime.value = _value;
    }
    private float GetBallHoverTime()
    {
        return ballHoverTime.value;
    }
    private void SaveBallHoverTime(float _value)
    {
        PlayerPrefs.SetFloat("hoverTime", _value);
        PlayerPrefs.Save();
    }
    private void LoadBallHoverTimeToMenu(bool resetToDefault)
    {
        float _value;
        if (resetToDefault)
            _value = (float)preferenceList["hoverTime"].Item1;
        else if (PlayerPrefs.HasKey("hoverTime"))
            _value = PlayerPrefs.GetFloat("hoverTime");
        else
            return;

        RecordBallHoverTime(_value);
        SetBallHoverTime(_value);
    }
    #endregion

}
