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
    [SerializeField] private GameObject targetHeightObject;

    #region Initialization
    /// <summary>
    /// Disable VR for menu scene and hide warning text until needed.
    /// </summary>
    void Start()
    {
        globalControl = GlobalControl.Instance;

        // disable VR settings for menu scene
        UnityEngine.XR.XRSettings.enabled = false;
        globalControl.numPaddles = 1;
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
            "practise_totaltime",
            "practise_level",
            "showcase_timePerTrial",
            "showcase_toggleVideo",
            "hovertime",
            "targetradius",
            "targetheight",
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
                    case "hovertime":
                        //LoadHoverTimeToMenu();
                        break;
                    case "targetradius":
                        //LoadTargetRadiusToMenu();
                        break;
                    case "targetheight":
                        //LoadTargetHeightToMenu();
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
    // Records the Target Line height preference from the dropdown menu
    public void RecordTargetHeight(int arg0)
    {
        targetHeightObject.GetComponent<TMP_Dropdown>().value = arg0;

        if (arg0 == 0)
        {
            globalControl.targetHeightPreference = TaskType.TargetHeight.EYE_LEVEL;
        }
        else if (arg0 == 1)
        {
            globalControl.targetHeightPreference = TaskType.TargetHeight.LOWERED;
        }
        else if (arg0 == 2)
        {
            globalControl.targetHeightPreference = TaskType.TargetHeight.RAISED;
        }
        else
        {
            Debug.LogError("Wrong target height");
        }

        GetComponent<MenuPlayerPrefs>().SaveTargetHeight(arg0);
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

    // Set the window for how far the ball can be from the target line and still count as a success
    public void UpdateTargetRadius(float value)
    {
        const float INCHES_PER_METER = 39.37f;
        const float METERS_PER_INCH = 0.0254f;

        Slider s = GameObject.Find("Success Threshold Slider").GetComponent<Slider>();
        TextMeshProUGUI sliderText = GameObject.Find("Width Indicator").GetComponent<TextMeshProUGUI>();

        float targetThresholdInches = value * 0.5f;
        float targetThresholdMeters = targetThresholdInches * METERS_PER_INCH; // each notch is 0.5 inches 
        sliderText.text = "+/- " + targetThresholdInches.ToString("0.0") + " in.\n(" + value.ToString("0.0") + " in. total)";
        s.value = value;
        globalControl.targetRadius = targetThresholdMeters;

        GetComponent<MenuPlayerPrefs>().SaveTargetRadius(value);
    }
    

    // Records the number of paddles from the dropdown nmenu
    public void RecordNumPaddles(int arg0)
    {
        TMP_Dropdown d = GameObject.Find("Num Paddle Dropdown").GetComponent<TMP_Dropdown>();
        d.value = arg0;

        if (arg0 == 0)
        {
            globalControl.numPaddles = 1;
        }
        else
        {
            globalControl.numPaddles = 2;
        }

        // GetComponent<MenuPlayerPrefs>().SaveNumPaddles(arg0);
    }


    public void RecordEnvironment(int arg0)
	{
        globalControl.environmentOption = arg0;
	}

    /// <summary>
    /// Loads next scene if wii is connected and participant ID was entered.
    /// </summary>
    public void NextScene()
    {
        if (globalControl.numPaddles == 1)
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
