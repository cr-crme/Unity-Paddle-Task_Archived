using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicDifficultyAlgorithm
{
    public int ComputeNewLevel(DifficultyManager _difficulty, Trial _trial)
    {
        int _newLevel = -1;
        if (GlobalControl.Instance.session == SessionType.Session.PRACTISE)
        {
            if (_difficulty.AreTrialConditionsMet(_trial))
            {
                if (_difficulty.currentLevel + 1 < _difficulty.nbLevel)
                    _newLevel = _difficulty.currentLevel + 1;
            }
        } else if (GlobalControl.Instance.session == SessionType.Session.SHOWCASE)
        {
            Debug.Log($"Time: {_trial.time}");
            if (_trial.time > GlobalControl.Instance.showcaseTimePerCondition * 60f)
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
