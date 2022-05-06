using UnityEngine;

public class DynamicDifficultyAlgorithm
{
    public int ComputeNewLevel(DifficultyManager _difficulty, Trial _trial)
    {
        GlobalPreferences globalControl = GlobalPreferences.Instance;
        int _newLevel = -1;
        if (globalControl.session == SessionType.Session.PRACTISE)
        {
            if (_difficulty.AreTrialConditionsMet(_trial))
            {
                if (_difficulty.currentLevel + 1 < _difficulty.nbLevel)
                    _newLevel = _difficulty.currentLevel + 1;
            }
        } else if (globalControl.session == SessionType.Session.SHOWCASE)
        {
            if (_trial.time > globalControl.showcaseTimePerCondition * globalControl.timeConversionToMinute)
            {
                if (_difficulty.currentLevel + 1 < _difficulty.nbLevel)
                    _newLevel = _difficulty.currentLevel + 2;
            }
        }
        else
        {
            Debug.LogError("Session type not implemented yet");
        }
        return _newLevel;
    }
}
