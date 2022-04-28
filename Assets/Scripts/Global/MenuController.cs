using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


/// <summary>
/// Holds functions for responding to and recording preferences on menu.
/// </summary>
public class MenuController : MonoBehaviour {

    private GlobalControl globalControl;
    [SerializeField] private GameObject sessionObject;
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
    public void LoadAllPreferences()
    {
        string[] preferenceList = {
            //"numpaddles"
            "session",
            "nbPaddles",
            "environment",
            "practise_totaltime",
            "practise_level",
            "showcase_timePerTrial",
            "showcase_toggleVideo",
            "targetHeight",
            "targetWidth",
            "hovertime",
        };


        // TODO why not just (Key of PlayerPrefs)?
        foreach (string pref in preferenceList)
        {
            if (PlayerPrefs.HasKey(pref))
            {
                switch (pref)
                {
                    case "session":
                        LoadSessionToMenu();
                        break;
                    case "nbPaddles":
                        LoadNbPaddlesDropdownToMenu();
                        break;
                    case "environment":
                        LoadEnvironmentDropdownToMenu();
                        break;
                    case "practise_totaltime":
                        LoadPractiseTotalTimeToMenu();
                        break;
                    case "practise_level":
                        LoadPractiseLevelToMenu();
                        break;
                    case "showcase_timePerTrial":
                        LoadShowcaseTimePerTrialToMenu();
                        break;
                    case "showcase_toggleVideo":
                        LoadShowcaseVideoToggle();
                        break;
                    case "targetHeight":
                        LoadTargetHeightDropdownToMenu();
                        break;
                    case "targetWidth":
                        LoadTargetWidthSliderToMenu();
                        break;
                    case "hovertime":
                        //LoadHoverTimeToMenu();
                        break;
                    default:
                        break;
                }
            }
        }
    }

    void UpdateConditionalUIObjects()
	{
        ShowProperSessionCanvas();
	}
    #endregion

    #region GenericInformation
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

    [SerializeField] private TMP_Dropdown nbPaddlesDropdown;
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
            RecordNbPaddlesDropdown(PlayerPrefs.GetInt("nbPaddles"));
            SetNbPaddlesDropdown(globalControl.nbPaddles - 1);  // 1-based
        }
    }

    [SerializeField] private TMP_Dropdown environmentDropdown;
    public void RecordEnvironmentDropdown(int _value)
    {
        globalControl.environmentIndex = _value;
        SaveEnvironmentDropdown(_value);
    }
    private void SetEnvironmentDropdown(int _value)
    {
        environmentDropdown.value = _value;
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
            SetEnvironmentDropdown(globalControl.environmentIndex); 
        }
    }
    #endregion

    #region SessionType
    // Records the Session from the dropdown menu
    public void RecordSession(int arg0)
    {
        sessionObject.GetComponent<TMP_Dropdown>().value = arg0;

        if (arg0 == 0)
        {
            globalControl.session = TaskType.Session.PRACTISE;
        }
        else if (arg0 == 1)
        {
            globalControl.session = TaskType.Session.SHOWCASE;
        }
        else
        {
            Debug.LogError("Not implemented Session");
        }
        SaveSession(arg0);
        UpdateConditionalUIObjects();
    }
    private void LoadSessionToMenu()
    {
        if (PlayerPrefs.HasKey("session"))
        {
            RecordSession(PlayerPrefs.GetInt("session"));
        }
    }
    public void SaveSession(int menuInt)
    {
        PlayerPrefs.SetInt("session", menuInt);
        PlayerPrefs.Save();
    }
    #endregion

    #region TrialTime
    [SerializeField] private TMP_Dropdown practiseTotalTimeDropdown;
    [SerializeField] private TMP_Dropdown showcaseTimePerTrialDropdown;
    public void RecordPractiseTotalTime(int _value)
    {
        globalControl.maxTrialTime = _value;
        SavePractiseTotalTime(_value);
    }
    private void SetPractiseTotalTimeDropdown(int _value)
    {
        practiseTotalTimeDropdown.value = _value;
    }
    private void SavePractiseTotalTime(int _value)
    {
        PlayerPrefs.SetInt("practise_totaltime", _value);
        PlayerPrefs.Save();
    }
    private void LoadPractiseTotalTimeToMenu()
    {
        if (PlayerPrefs.HasKey("practise_totaltime"))
        {
            RecordPractiseTotalTime(PlayerPrefs.GetInt("practise_totaltime"));
            SetPractiseTotalTimeDropdown(globalControl.maxTrialTime);
        }
    }

    public void RecordShowcaseTimePerTrial(int _value)
    {
        globalControl.maxTrialTime = _value;
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
            RecordShowcaseTimePerTrial(PlayerPrefs.GetInt("showcase_timePerTrial"));
            SetShowcaseTimePerTrialDropdown(globalControl.maxTrialTime);
        }
    }

    private void ShowProperSessionCanvas()
    {
        if (globalControl.session == TaskType.Session.PRACTISE)
        {
            practiseCanvas.SetActive(true);
            RecordPractiseTotalTime(practiseTotalTimeDropdown.value);

            showcaseCanvas.SetActive(false);

        }
        else if (globalControl.session == TaskType.Session.SHOWCASE)
        {
            showcaseCanvas.SetActive(true);
            RecordShowcaseTimePerTrial(showcaseTimePerTrialDropdown.value);

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
            RecordPractiseLevel(PlayerPrefs.GetInt("practise_level"));
            SetPractiseLevelDropdown(globalControl.level);
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
            RecordShowcaseVideoToggle(
                PlayerPrefs.GetInt("showcase_toggleVideo") == 1 ? true : false
            );
            SetShowcaseVideoToggle(globalControl.playVideo);
        }
    }
    #endregion

    #region Target
    const float INCHES_PER_METER = 39.37f;
    [SerializeField] private TMP_Dropdown targetHeightDropdown;
    [SerializeField] private Slider targetWidthSlider;
    [SerializeField] private TextMeshProUGUI targetWidthText;
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
            RecordTargetHeightDropdown(PlayerPrefs.GetInt("targetHeight"));
            SetTargetHeightDropdown((int)globalControl.targetHeightPreference);
        }
    }

    public void RecordTargetWidthSlider(float _value)
    {
        float targetThresholdInches = _value * 0.5f;  // each notch is 0.5 inches 
        float targetThresholdMeters = targetThresholdInches / INCHES_PER_METER;
        targetWidthText.text = $"+/- {targetThresholdInches.ToString("0.0")} in.\n" +
            $"({_value.ToString("0.0")} in. total)";
        targetWidthSlider.value = _value;
        globalControl.targetWidth = targetThresholdMeters;
        SaveTargetWidthSlider(_value);
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
            RecordTargetWidthSlider(PlayerPrefs.GetFloat("targetWidth"));
            SetTargetWidthSlider(globalControl.targetWidth * INCHES_PER_METER / 0.5f);
        }
    }
    #endregion


    // Record how many seconds the ball should hover for upon reset
    public void UpdateHoverTime(float value)
    {
        Slider s = GameObject.Find("Ball Respawn Time Slider").GetComponent<Slider>();
        TextMeshProUGUI sliderText = GameObject.Find("Time Indicator").GetComponent<TextMeshProUGUI>();

        sliderText.text = value + " seconds";
        s.value = value;
        globalControl.ballResetHoverSeconds = (int)value;

        GetComponent<MenuPlayerPrefs>().SaveHoverTime(value);
    }

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
