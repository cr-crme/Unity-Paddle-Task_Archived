using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    [SerializeField, Tooltip("The paddles manager")]
    private PaddlesManager paddlesManager;

    DifficultyFactory difficultyFactory = new DifficultyFactory();
    private DifficultyDefinition difficulty;

    private void Awake()
    {
        difficulty = difficultyFactory.trialLevelDefinitions[GlobalControl.Instance.difficulty];
    }

    #region Accessors
    #region Ball
    public int nbOfBounceRequired { get { return difficulty.nbOfBounceRequired; } }
    public int nbOfAccurateBounceRequired { get { return difficulty.nbOfAccurateBounceRequired; } }
    public float ballSpeed { get { return difficulty.ballSpeed(currentLevel); } }
    #endregion


    #region Target
    public bool hasTarget { get { return difficulty.hasTarget(currentLevel); } }
    public TargetEnum.Height targetHeight { get { return GlobalControl.Instance.targetHeightPreference; } }
    public float targetHeightOffset { get { return difficulty.targetHeightOffset(currentLevel); } }
    public float targetWidth { get { return difficulty.targetWidth(currentLevel); } }
    #endregion


    #region Trial
    public bool AreTrialConditionsMet(Trial _currentTrial) {
        return difficulty.AreTrialConditionsMet(_currentLevel, _currentTrial);
    }
    public bool mustSwitchPaddleAfterHitting { get { return paddlesManager.NbPaddles > 1; } }
    #endregion
    #endregion



    // Level is the increment inside a difficulty condition 
    #region Level
    public int nbLevel { get { return difficulty.nbLevel; } }
    private int _currentLevel = 0;
    public int currentLevel { get { return _currentLevel; } }
    public void SetCurrentLevel(int value)
    {
        if (value < 0 || value >= difficulty.nbLevel)
        {
            Debug.LogError("Issue setting difficulty, not in expected range: " + value);
            return;
        }
        _currentLevel = value;

        GlobalControl.Instance.timescale = ballSpeed;
    }
    #endregion

}
