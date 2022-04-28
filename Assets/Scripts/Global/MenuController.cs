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
            { "nbPaddles", (GetNbPaddlesDropdown(), LoadNbPaddlesDropdownToMenu) },
            { "environment", (GetEnvironmentDropdown(), LoadEnvironmentDropdownToMenu) },
            { "session", (GetRecordSession(), LoadSessionToMenu) },
            { "practise_totalTime", (GetPractiseTotalTimeDropdown(), LoadPractiseTotalTimeToMenu) },
            { "showcase_timePerTrial", (GetShowcaseTimePerTrialDropdown(), LoadShowcaseTimePerTrialToMenu) },
            { "practise_level", (GetPractiseLevelDropdown(), LoadPractiseLevelToMenu) },
            { "showcase_toggleVideo", (GetShowcaseVideoToggle(), LoadShowcaseVideoToggleToMenu) },
            { "targetHeight", (GetTargetHeightDropdown(), LoadTargetHeightDropdownToMenu) },
            { "targetWidth", (GetTargetWidthSlider(), LoadTargetWidthSliderToMenu) },
            { "hoverTime", (GetBallHoverTimeSlider(), LoadBallHoverTimeSliderToMenu) },
        };

        // disable VR settings for menu scene
        UnityEngine.XR.XRSettings.enabled = false;
        globalControl.nbPaddles = 1;
        globalControl.participantID = "";

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

    #region GenericInformation
    [SerializeField] private TMP_Dropdown nbPaddlesDropdown;
    [SerializeField] private TMP_Dropdown environmentDropdown;

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

    public void RecordNbPaddlesDropdown(int _value)
    {
        globalControl.nbPaddles = _value + 1;  // 0-based
        SaveNbPaddlesDropdown(_value);
    }
    private void SetNbPaddlesDropdown(int _value)
    {
        nbPaddlesDropdown.value =_value;
    }
    private int GetNbPaddlesDropdown()
    {
        return nbPaddlesDropdown.value;
    }
    private void SaveNbPaddlesDropdown(int _value)
    {
        PlayerPrefs.SetInt("nbPaddles", _value);
        PlayerPrefs.Save();
    }
    private void LoadNbPaddlesDropdownToMenu(bool resetToDefault)
    {
        int _value;
        if (resetToDefault)
            _value = (int)preferenceList["nbPaddles"].Item1;
        else if (PlayerPrefs.HasKey("nbPaddles"))
            _value = PlayerPrefs.GetInt("nbPaddles");
        else
            return;

        RecordNbPaddlesDropdown(_value);
        SetNbPaddlesDropdown(_value);
    }

    public void RecordEnvironmentDropdown(int _value)
    {
        globalControl.environmentIndex = _value;
        SaveEnvironmentDropdown(_value);
    }
    private void SetEnvironmentDropdown(int _value)
    {
        environmentDropdown.value = _value;
    }
    private int GetEnvironmentDropdown()
    {
        return environmentDropdown.value;
    }
    private void SaveEnvironmentDropdown(int _value)
    {
        PlayerPrefs.SetInt("environment", _value);
        PlayerPrefs.Save();
    }
    private void LoadEnvironmentDropdownToMenu(bool resetToDefault)
    {
        int _value;
        if (resetToDefault)
            _value = (int)preferenceList["environment"].Item1;
        else if (PlayerPrefs.HasKey("environment"))
            _value = PlayerPrefs.GetInt("environment");
        else
            return;

        RecordEnvironmentDropdown(_value);
        SetEnvironmentDropdown(_value);
    }
    #endregion

    #region SessionType
    [SerializeField] private TMP_Dropdown sessionDropdown;
    // Records the Session from the dropdown menu
    public void RecordSession(int _value)
    {
        globalControl.session = (TaskType.Session)_value;
        SaveSession(_value);
        UpdateConditionalUIObjects();
    }
    private void SetRecordSession(int _value)
    {
        sessionDropdown.value = _value;
    }
    private int GetRecordSession()
    {
        return sessionDropdown.value;
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
    [SerializeField] private TMP_Dropdown practiseTotalTimeDropdown;
    [SerializeField] private TMP_Dropdown showcaseTimePerTrialDropdown;
    public void RecordPractiseTotalTime(int _value)
    {
        globalControl.practiseMaxTrialTime = _value;
        SavePractiseTotalTime(_value);
    }
    private void SetPractiseTotalTimeDropdown(int _value)
    {
        practiseTotalTimeDropdown.value = _value;
    }
    private int GetPractiseTotalTimeDropdown()
    {
        return practiseTotalTimeDropdown.value;
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
        SetPractiseTotalTimeDropdown(_value);
    }

    public void RecordShowcaseTimePerTrial(int _value)
    {
        globalControl.showcaseTimePerCondition = _value;
        SaveShowcaseTimePerTrial(_value);
    }
    private void SetShowcaseTimePerTrialDropdown(int _value)
    {
        showcaseTimePerTrialDropdown.value = _value;
    }
    private int GetShowcaseTimePerTrialDropdown()
    {
        return showcaseTimePerTrialDropdown.value;
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
        SetShowcaseTimePerTrialDropdown(_value);
    }

    private void ShowProperSessionCanvas()
    {
        if (globalControl.session == TaskType.Session.PRACTISE)
        {
            practiseCanvas.SetActive(true);
            showcaseCanvas.SetActive(false);

        }
        else if (globalControl.session == TaskType.Session.SHOWCASE)
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

    #region Level
    [SerializeField] private TMP_Dropdown practiseLevelDropdown;
    public void RecordPractiseLevel(int _value)
    {
        globalControl.level = _value;
        SavePractiseLevel(_value);
    }
    private void SetPractiseLevelDropdown(int _value)
    {
        practiseLevelDropdown.value = _value;
    }
    private int GetPractiseLevelDropdown()
    {
        return practiseLevelDropdown.value;
    }
    private void SavePractiseLevel(int _value)
    {
        PlayerPrefs.SetInt("practise_level", _value);
        PlayerPrefs.Save();
    }
    private void LoadPractiseLevelToMenu(bool resetToDefault)
    {
        int _value;
        if (resetToDefault)
            _value = (int)preferenceList["practise_level"].Item1;
        else if (PlayerPrefs.HasKey("practise_level"))
            _value = PlayerPrefs.GetInt("practise_level");
        else
            return;

        RecordPractiseLevel(_value);
        SetPractiseLevelDropdown(_value);
    }
    #endregion

    #region VideoTutorial
    [SerializeField] private Toggle showcaseVideoToggle;
    public void RecordShowcaseVideoToggle(bool _value)
    {
        globalControl.playVideo = _value;
        SaveShowcaseVideoToggle(_value);
    }
    private void SetShowcaseVideoToggle(bool _value)
    {
        showcaseVideoToggle.isOn = _value;
    }
    private bool GetShowcaseVideoToggle()
    {
        return showcaseVideoToggle.isOn;
    }
    private void SaveShowcaseVideoToggle(bool _value)
    {
        PlayerPrefs.SetInt("showcase_toggleVideo", _value ? 1 : 0);
        PlayerPrefs.Save();
    }
    private void LoadShowcaseVideoToggleToMenu(bool resetToDefault)
    {
        bool _value;
        if (resetToDefault)
            _value = (bool)preferenceList["showcase_toggleVideo"].Item1;
        else if (PlayerPrefs.HasKey("showcase_toggleVideo"))
            _value = PlayerPrefs.GetInt("showcase_toggleVideo") == 1 ? true : false;
        else
            return;

        RecordShowcaseVideoToggle(_value);
        SetShowcaseVideoToggle(_value);
    }
    #endregion

    #region Target
    [SerializeField] private TMP_Dropdown targetHeightDropdown;
    [SerializeField] private Slider targetWidthSlider;
    [SerializeField] private TextMeshProUGUI targetWidthText;
    const float INCHES_PER_METER = 39.37f;
    public void RecordTargetHeightDropdown(int _value)
    {
        globalControl.targetHeightPreference = (TaskType.TargetHeight)_value;
        SaveTargetHeightDropdown(_value);
    }
    private void SetTargetHeightDropdown(int _value)
    {
        targetHeightDropdown.value = _value;
    }
    private int GetTargetHeightDropdown()
    {
        return targetHeightDropdown.value;
    }
    private void SaveTargetHeightDropdown(int _value)
    {
        PlayerPrefs.SetInt("targetHeight", _value);
        PlayerPrefs.Save();
    }
    private void LoadTargetHeightDropdownToMenu(bool resetToDefault)
    {
        int _value;
        if (resetToDefault)
            _value = (int)preferenceList["targetHeight"].Item1;
        else if (PlayerPrefs.HasKey("targetHeight"))
            _value = PlayerPrefs.GetInt("targetHeight");
        else
            return;

        RecordTargetHeightDropdown(_value);
        SetTargetHeightDropdown(_value);
    }

    public void RecordTargetWidthSlider(float _value)
    {
        globalControl.targetWidth = ComputeTargetWidthInMeter(_value);
        UpdateTargetWidthText(_value);
        SaveTargetWidthSlider(_value);
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
    private void SetTargetWidthSlider(float _value)
    {
        targetWidthSlider.value = _value;
    }
    private float GetTargetWidthSlider()
    {
        return targetWidthSlider.value;
    }
    private void SaveTargetWidthSlider(float _value)
    {
        PlayerPrefs.SetFloat("targetWidth", _value);
        PlayerPrefs.Save();
    }
    private void LoadTargetWidthSliderToMenu(bool resetToDefault)
    {
        float _value;
        if (resetToDefault)
            _value = (float)preferenceList["targetWidth"].Item1;
        else if (PlayerPrefs.HasKey("targetWidth"))
            _value = PlayerPrefs.GetFloat("targetWidth");
        else
            return;

        RecordTargetWidthSlider(_value);
        SetTargetWidthSlider(_value);
        UpdateTargetWidthText(_value);
    }
    #endregion

    #region Ball
    [SerializeField] private Slider ballHoverTimeSlider;
    [SerializeField] private TextMeshProUGUI ballHoverTimeText;
   
    public void RecordBallHoverTimeSlider(float _value)
    {
        globalControl.ballResetHoverSeconds = (int)_value;
        UpdateBallHoverTimeText(_value);
        SaveBallHoverTimeSlider(_value);
    }
    private void UpdateBallHoverTimeText(float _value)
    {
        ballHoverTimeText.text = $"{_value} seconds";
    }
    private void SetBallHoverTimeSlider(float _value)
    {
        ballHoverTimeSlider.value = _value;
    }
    private float GetBallHoverTimeSlider()
    {
        return ballHoverTimeSlider.value;
    }
    private void SaveBallHoverTimeSlider(float _value)
    {
        PlayerPrefs.SetFloat("hoverTime", _value);
        PlayerPrefs.Save();
    }
    private void LoadBallHoverTimeSliderToMenu(bool resetToDefault)
    {
        float _value;
        if (resetToDefault)
            _value = (float)preferenceList["hoverTime"].Item1;
        else if (PlayerPrefs.HasKey("hoverTime"))
            _value = PlayerPrefs.GetFloat("hoverTime");
        else
            return;

        RecordBallHoverTimeSlider(_value);
        SetBallHoverTimeSlider(_value);
        UpdateBallHoverTimeText(_value);
    }
    #endregion

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
}
