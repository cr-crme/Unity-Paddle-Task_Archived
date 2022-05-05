using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicDifficultyAlgorithm
{
    public int ComputeNewLevel(DifficultyManager _difficulty, Trial _trial)
    {
        bool _trialSuccessful = _difficulty.AreTrialConditionsMet(_trial);
        int _newLevel = -1;
        if (_trialSuccessful)
        {
            if (_difficulty.currentLevel + 1 < _difficulty.nbLevel)
                _newLevel = _difficulty.currentLevel + 1;
        }
        return _newLevel;
    }
}
