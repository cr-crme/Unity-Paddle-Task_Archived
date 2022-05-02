using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    #region Functions to move out of the file

    #endregion



    [SerializeField, Tooltip("The paddles manager")]
    private PaddlesManager paddlesManager;

    #region Accessors
    #region Ball
    public int nbOfBounceRequired { get { return currentDifficulty.nbOfBounceRequired; } }
    public int nbOfAccurateBounceRequired { get { return currentDifficulty.nbOfAccurateBounceRequired; } }
    public float ballSpeed { get { return currentDifficulty.ballSpeed(currentLevel); } }
    #endregion

    #region Target
    public bool hasTarget { get { return currentDifficulty.hasTarget(currentLevel); } }
    public TargetEnum.Height targetHeight { get { return GlobalControl.Instance.targetHeightPreference; } }
    public float targetHeightOffset { get { return currentDifficulty.targetHeightOffset(currentLevel); } }
    public float targetWidth { get { return currentDifficulty.targetWidth(currentLevel); } }
    #endregion

    #region Trial
    public bool AreAllDifficultiesDone { get { return currentDifficultyChoiceIndex >= difficultyOverSession.Count; } }
    public bool AreTrialConditionsMet(Trial _currentTrial) {
        return currentDifficulty.AreTrialConditionsMet(_currentLevel, _currentTrial); 
    }
    public bool mustSwitchPaddleAfterHitting { get { return paddlesManager.NbPaddles > 1; } }
    public double maximumTrialTime { get { return currentDifficulty.maximumTrialTime; } }
    #endregion
    #endregion



    // Level is the increment inside a difficulty condition 
    #region Level
    private int _currentLevel = 0;
    public int currentLevel { 
        get { return _currentLevel; }
        set
        {
            if (value < 0 || value > currentDifficulty.nbLevel)
            {
                Debug.LogError("Issue setting difficulty, not in expected range: " + value);
                return;
            }
            _currentLevel = value;
        }
    }
    public int ScoreToLevel(double _score)
    {
        return currentDifficulty.ScoreToLevel(_score);
    }
    #endregion



    // Difficulty is a specific set of level conditions
    #region Difficulty
    public string difficultyName { get { return currentDifficulty.name; } }
    private List<DifficultyChoice> difficultyOverSession = new List<DifficultyChoice>()
    {
        DifficultyChoice.BASE,
        DifficultyChoice.MODERATE,
        DifficultyChoice.MAXIMAL
    };
    public void AddDifficultyToSessionList(DifficultyChoice newDifficulty)
    {
        difficultyOverSession.Add(newDifficulty);
    }
    private int currentDifficultyChoiceIndex = 0;
    private DifficultyChoice currentDifficultyChoice { 
        get { return difficultyOverSession[currentDifficultyChoiceIndex];  } 
    }
    private DifficultyFactory difficultyFactory = new DifficultyFactory();
    private DifficultyDefinition currentDifficulty
    {
        get { return difficultyFactory.trialLevelDefinitions[currentDifficultyChoice]; }
    }
    #endregion
}
