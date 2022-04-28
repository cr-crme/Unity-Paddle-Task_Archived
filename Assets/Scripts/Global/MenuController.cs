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

    #region Initialization
    /// <summary>
    /// Disable VR for menu scene and hide warning text until needed.
    /// </summary>
    void Start()
    {
        globalControl = GlobalControl.Instance;

        // disable VR settings for menu scene
        UnityEngine.XR.XRSettings.enabled = false;
        globalControl.nbPaddles = 1;
        globalControl.participantID = "";

        // Load saved preferences
        LoadAllPreferences();

        UpdateConditionalUIObjects();
    }
    // Loads all saved preferences to the main menu
    private delegate void LoadCallback();
    public void LoadAllPreferences()
    {
        Dictionary<string, LoadCallback> preferenceList
            = new Dictionary<string, LoadCallback>(){
                { "nbPaddles", LoadNbPaddlesDropdownToMenu },
                { "environment", LoadEnvironmentDropdownToMenu },
                { "session", LoadSessionToMenu },
                { "practise_totalTime", LoadPractiseTotalTimeToMenu },
                { "showcase_timePerTrial", LoadShowcaseTimePerTrialToMenu },
                { "practise_level", LoadPractiseLevelToMenu },
                { "showcase_toggleVideo", LoadShowcaseVideoToggle },
                { "targetHeight", LoadTargetHeightDropdownToMenu },
                { "targetWidth", LoadTargetWidthSliderToMenu },
                { "hoverTime", LoadBallHoverTimeSliderToMenu }, 
        };
        foreach (KeyValuePair<string, LoadCallback> callback in preferenceList)
        {
            callback.Value();
        }
    }

    void UpdateConditionalUIObjects()
	{
        ShowProperSessionCanvas();
	}
    #endregion

    #region GenericInformation
    [SerializeField] private TMP_Dropdown nbPaddlesDropdown;

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
        nbPaddlesDropdown.value = _value; 
    }
    private void SaveNbPaddlesDropdown(int _value)
    {
        PlayerPrefs.SetInt("nbPaddles", _value);
        PlayerPrefs.Save();
    }
    private void LoadNbPaddlesDropdownToMenu()
    {
        if (PlayerPrefs.HasKey("nbPaddles"))
        {
            int _value = PlayerPrefs.GetInt("nbPaddles");
            RecordNbPaddlesDropdown(_value);
            SetNbPaddlesDropdown(_value);
        }
    }

    [SerializeField] private TMP_Dropdown environmentDropdown;
    public void RecordEnvironmentDropdown(int _value)
    {
        globalControl.environmentIndex = _value;
        environmentDropdown.value = _value;
        SaveEnvironmentDropdown(_value);
    }
    private void SaveEnvironmentDropdown(int _value)
    {
        PlayerPrefs.SetInt("environment", _value);
        PlayerPrefs.Save();
    }
    private void LoadEnvironmentDropdownToMenu()
    {
        if (PlayerPrefs.HasKey("environment"))
        {
            RecordEnvironmentDropdown(PlayerPrefs.GetInt("environment"));
        }
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
    public void SaveSession(int menuInt)
    {
        PlayerPrefs.SetInt("session", menuInt);
        PlayerPrefs.Save();
    }
    private void LoadSessionToMenu()
    {
        if (PlayerPrefs.HasKey("session"))
        {
            int _value = PlayerPrefs.GetInt("session");
            RecordSession(_value);
            SetRecordSession(_value);
        }
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
    private void SavePractiseTotalTime(int _value)
    {
        PlayerPrefs.SetInt("practise_totalTime", _value);
        PlayerPrefs.Save();
    }
    private void LoadPractiseTotalTimeToMenu()
    {
        if (PlayerPrefs.HasKey("practise_totalTime"))
        {
            int _value = PlayerPrefs.GetInt("practise_totalTime");
            RecordPractiseTotalTime(_value);
            SetPractiseTotalTimeDropdown(_value);
        }
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
    private void SaveShowcaseTimePerTrial(int _value)
    {
        PlayerPrefs.SetInt("showcase_timePerTrial", _value);
        PlayerPrefs.Save();
    }
    private void LoadShowcaseTimePerTrialToMenu()
    {
        if (PlayerPrefs.HasKey("showcase_timePerTrial"))
        {
            int _value = PlayerPrefs.GetInt("showcase_timePerTrial");
            RecordShowcaseTimePerTrial(_value);
            SetShowcaseTimePerTrialDropdown(_value);
        }
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
    private void SavePractiseLevel(int _value)
    {
        PlayerPrefs.SetInt("practise_level", _value);
        PlayerPrefs.Save();
    }
    private void LoadPractiseLevelToMenu()
    {
        if (PlayerPrefs.HasKey("practise_level"))
        {
            int _value = PlayerPrefs.GetInt("practise_level");
            RecordPractiseLevel(_value);
            SetPractiseLevelDropdown(_value);
        }
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
    private void SaveShowcaseVideoToggle(bool _value)
    {
        PlayerPrefs.SetInt("showcase_toggleVideo", _value ? 1 : 0);
        PlayerPrefs.Save();
    }
    private void LoadShowcaseVideoToggle()
    {
        if (PlayerPrefs.HasKey("showcase_toggleVideo"))
        {
            bool _value = PlayerPrefs.GetInt("showcase_toggleVideo") == 1 ? true : false;
            RecordShowcaseVideoToggle(_value);
            SetShowcaseVideoToggle(_value);
        }
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
    private void SaveTargetHeightDropdown(int _value)
    {
        PlayerPrefs.SetInt("targetHeight", _value);
        PlayerPrefs.Save();
    }
    private void LoadTargetHeightDropdownToMenu()
    {
        if (PlayerPrefs.HasKey("targetHeight"))
        {
            int _value = PlayerPrefs.GetInt("targetHeight");
            RecordTargetHeightDropdown(_value);
            SetTargetHeightDropdown(_value);
        }
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
    private void SaveTargetWidthSlider(float _value)
    {
        PlayerPrefs.SetFloat("targetWidth", _value);
        PlayerPrefs.Save();
    }
    private void LoadTargetWidthSliderToMenu()
    {
        if (PlayerPrefs.HasKey("targetWidth"))
        {
            float _value = PlayerPrefs.GetFloat("targetWidth");
            RecordTargetWidthSlider(_value);
            SetTargetWidthSlider(_value);
            UpdateTargetWidthText(_value);
        }
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
    private void SaveBallHoverTimeSlider(float _value)
    {
        PlayerPrefs.SetFloat("hoverTime", _value);
        PlayerPrefs.Save();
    }
    private void LoadBallHoverTimeSliderToMenu()
    {
        if (PlayerPrefs.HasKey("hoverTime"))
        {
            float _value = PlayerPrefs.GetFloat("hoverTime");
            RecordBallHoverTimeSlider(_value);
            SetBallHoverTimeSlider(_value);
            UpdateBallHoverTimeText(_value);
        }
    }
    #endregion

    /// <summary>
    /// Loads next scene if wii is connected and participant ID was entered.
    /// </summary>
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
