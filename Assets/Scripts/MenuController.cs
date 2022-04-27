﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Reflection;

/// <summary>
/// Holds functions for responding to and recording preferences on menu.
/// </summary>
public class MenuController : MonoBehaviour {

    public GameObject maxTrialTimeObject, maxDifficultyTrialTimeObject;

    /// <summary>
    /// Disable VR for menu scene and hide warning text until needed.
    /// </summary>
    void Start()
    {
        // disable VR settings for menu scene
        UnityEngine.XR.XRSettings.enabled = false;
        GlobalControl.Instance.numPaddles = 1;
        GlobalControl.Instance.participantID = "";
        GlobalControl.Instance.explorationMode = GlobalControl.ExplorationMode.NONE;

        // Load saved preferences
        GetComponent<MenuPlayerPrefs>().LoadAllPreferences();

        UpdateConditionalUIObjects();
    }

    void UpdateConditionalUIObjects()
	{
        bool baseline = GlobalControl.Instance.session == TaskType.Session.BASELINE;
		maxTrialTimeObject.SetActive(!baseline);
        maxDifficultyTrialTimeObject.SetActive(baseline);
		
        
	}

    /// <summary>
    /// Records an alphanumeric participant ID. Hit enter to record. May be entered multiple times
    /// but only last submission is used. Called using a dynamic function in the inspector
    /// of the textfield object.
    /// </summary>
    /// <param name="arg0"></param>
    public void RecordID(string arg0)
    {
        GlobalControl.Instance.participantID = arg0;
    }


    /// <summary>
    /// Records an float representing degrees of freedom in the xz plane.
    /// </summary>
    /// <param name="arg0"></param>
    public void RecordDegrees()
    {
        GlobalControl.Instance.degreesOfFreedom = (GlobalControl.Instance.condition == TaskType.Condition.REDUCED) ? 0.0f : 90.0f;
        // GetComponent<MenuPlayerPrefs>().SaveDOF(GlobalControl.Instance.degreesOfFreedom);
    }

    /// <summary>
    /// Records an int representing max number of trials allowed for this instance.
    /// </summary>
    /// <param name="arg0"></param>
    public void RecordMaxTrials(int arg0)
    {
        if (maxTrialTimeObject.activeInHierarchy)
        {
            TMP_Dropdown d = GameObject.Find("Max Trial Time Dropdown").GetComponent<TMP_Dropdown>();
            d.value = arg0;
            GlobalControl.Instance.maxTrialTime = arg0;
        }
    }

    public void RecordMaxTrialsBaseline(int arg0)
	{
        RecordDifficultyMaxTrials(arg0, GameObject.Find("Max Baseline Trial Time Dropdown").GetComponent<TMP_Dropdown>());
        GlobalControl.Instance.maxBaselineTrialTime = arg0 != 0 ? arg0 : 10;
	}

    public void RecordMaxTrialsModerate1(int arg0)
    {
        RecordDifficultyMaxTrials(arg0, GameObject.Find("Max Moderate1 Trial Time Dropdown").GetComponent<TMP_Dropdown>());
        GlobalControl.Instance.maxModerate1TrialTime = arg0 != 0 ? arg0 : 10;
    }

    public void RecordMaxTrialsMaximal(int arg0)
    {
        RecordDifficultyMaxTrials(arg0, GameObject.Find("Max Maximal Trial Time Dropdown").GetComponent<TMP_Dropdown>());
        GlobalControl.Instance.maxMaximalTrialTime = arg0 != 0 ? arg0 : 10;
    }

    public void RecordMaxTrialsModerate2(int arg0)
    {
        RecordDifficultyMaxTrials(arg0, GameObject.Find("Max Moderate2 Trial Time Dropdown").GetComponent<TMP_Dropdown>());
        GlobalControl.Instance.maxModerate2TrialTime = arg0 != 0 ? arg0 : 10;
    }

    public void RecordDifficultyMaxTrials(int arg0, TMP_Dropdown trialsDropdown)
	{
        if (maxDifficultyTrialTimeObject.activeInHierarchy)
        {
            trialsDropdown.value = arg0;
        }

        GetComponent<MenuPlayerPrefs>().SaveMaxTrials(arg0);
    }

    // Record how many seconds the ball should hover for upon reset
    public void UpdateHoverTime(float value)
    {
        Slider s = GameObject.Find("Ball Respawn Time Slider").GetComponent<Slider>();
        TextMeshProUGUI sliderText = GameObject.Find("Time Indicator").GetComponent<TextMeshProUGUI>();

        sliderText.text = value + " seconds";
        s.value = value;
        GlobalControl.Instance.ballResetHoverSeconds = (int)value;

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
        GlobalControl.Instance.targetRadius = targetThresholdMeters;

        GetComponent<MenuPlayerPrefs>().SaveTargetRadius(value);
    }
    

    // Records the Condition from the dropdown menu
    public void RecordCondition(int arg0)
    {
        TMP_Dropdown d = GameObject.Find("Condition Dropdown").GetComponent<TMP_Dropdown>();
        d.value = arg0;

        switch (arg0)
        {
            case 0:
                GlobalControl.Instance.condition = TaskType.Condition.REGULAR;
                break;
            case 1:
                GlobalControl.Instance.condition = TaskType.Condition.ENHANCED;
                break;
            case 2:
                GlobalControl.Instance.condition = TaskType.Condition.REDUCED;
                break;
            case 3:
                GlobalControl.Instance.condition = TaskType.Condition.TARGETLINE;
                break;

            default:
                GlobalControl.Instance.condition = TaskType.Condition.REGULAR;
                break;
        }

        GetComponent<MenuPlayerPrefs>().SaveCondition(arg0);
    }

    // Records the functional Exploration mode, tied to Condition dropdown menu
    public void RecordExplorationMode(int arg0)
    {
        if (arg0 == 1)
        {
            GlobalControl.Instance.explorationMode = GlobalControl.ExplorationMode.FORCED;
        }
        else
        {
            GlobalControl.Instance.explorationMode = GlobalControl.ExplorationMode.NONE;
        }

        GetComponent<MenuPlayerPrefs>().SaveExplorationMode(arg0);
    }

    // Records the Condition from the dropdown menu
    public void RecordExpCond(int arg0)
    {
        TMP_Dropdown d = GameObject.Find("ExpCondition Dropdown").GetComponent<TMP_Dropdown>();
        d.value = arg0;

        switch (arg0)
        {
            case 0:
                GlobalControl.Instance.expCondition = TaskType.ExpCondition.RANDOM;
                break;
            case 1:
                GlobalControl.Instance.expCondition = TaskType.ExpCondition.LIGHTEST;
                break;
            case 2:
                GlobalControl.Instance.expCondition = TaskType.ExpCondition.LIGHTER;
                break;
            case 3:
                GlobalControl.Instance.expCondition = TaskType.ExpCondition.NORMAL;
                break;
            case 4:
                GlobalControl.Instance.expCondition = TaskType.ExpCondition.HEAVIER;
                break;
            case 5:
                GlobalControl.Instance.expCondition = TaskType.ExpCondition.HEAVIEST;
                break;
            default:
                GlobalControl.Instance.expCondition = TaskType.ExpCondition.RANDOM;
                break;
        }

        GetComponent<MenuPlayerPrefs>().SaveExpCondition(arg0);
    }

    // Records the Session from the dropdown menu
    public void RecordSession(int arg0)
    {
        TMP_Dropdown d = GameObject.Find("Session Dropdown").GetComponent<TMP_Dropdown>();
        d.value = arg0;

        if (arg0 == 0)
        {
            GlobalControl.Instance.session = TaskType.Session.BASELINE;
        }
        else if (arg0 == 1)
        {
            GlobalControl.Instance.session = TaskType.Session.ACQUISITION;
        }
        else if (arg0 == 2)
        {
            GlobalControl.Instance.session = TaskType.Session.RETENTION;
        }
        else if (arg0 == 3)
        {
            GlobalControl.Instance.session = TaskType.Session.TRANSFER;
        }
        else if (arg0 == 4)
        {
            GlobalControl.Instance.session = TaskType.Session.SHOWCASE;
        }


        GetComponent<MenuPlayerPrefs>().SaveSession(arg0);
        UpdateConditionalUIObjects();
    }

    // Records the Target Line height preference from the dropdown menu
    public void RecordTargetHeight(int arg0)
    {
        TMP_Dropdown d = GameObject.Find("Target Height Dropdown").GetComponent<TMP_Dropdown>();
        d.value = arg0;

        if (arg0 == 0)
        {
            GlobalControl.Instance.targetHeightPreference = TaskType.TargetHeight.DEFAULT;
        }
        if (arg0 == 1)
        {
            GlobalControl.Instance.targetHeightPreference = TaskType.TargetHeight.LOWERED;
        }
        if (arg0 == 2)
        {
            GlobalControl.Instance.targetHeightPreference = TaskType.TargetHeight.RAISED;
        }

        GetComponent<MenuPlayerPrefs>().SaveTargetHeight(arg0);
    }

    // Records the number of paddles from the dropdown nmenu
    public void RecordNumPaddles(int arg0)
    {
        TMP_Dropdown d = GameObject.Find("Num Paddle Dropdown").GetComponent<TMP_Dropdown>();
        d.value = arg0;

        if (arg0 == 0)
        {
            GlobalControl.Instance.numPaddles = 1;
        }
        else
        {
            GlobalControl.Instance.numPaddles = 2;
        }

        // GetComponent<MenuPlayerPrefs>().SaveNumPaddles(arg0);
    }

    public void RecordTimescale(string arg0)
	{
        int parsed;
        if (Int32.TryParse(arg0, out parsed))
		{
            GlobalControl.Instance.timescale = parsed;
		}
    }

    public void RecordDifficulty(string arg0)
    {
        int parsed;
        if (Int32.TryParse(arg0, out parsed))
        {
            GlobalControl.Instance.difficulty = parsed;
        }
    }

    public void RecordTargetHeight(bool arg0)
	{
        GlobalControl.Instance.targetHeightEnabled = arg0;
	}

    public void RecordPlayVideo(bool arg0)
	{
        GlobalControl.Instance.playVideo = arg0;
        GlobalControl.Instance.recordingData = !arg0;
	}

    public void RecordEnvironment(int arg0)
	{
        GlobalControl.Instance.environmentOption = arg0;
	}

    /// <summary>
    /// Loads next scene if wii is connected and participant ID was entered.
    /// </summary>
    public void NextScene()
    {
        if (GlobalControl.Instance.numPaddles == 1)
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