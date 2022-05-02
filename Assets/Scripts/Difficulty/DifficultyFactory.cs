using System;
using System.Collections.Generic;

public enum DifficultyChoice { BASE, MODERATE, MAXIMAL };
public class DifficultyFactory
{
    public Dictionary<DifficultyChoice, DifficultyDefinition> trialLevelDefinitions { 
        get; private set; 
    } = new Dictionary<DifficultyChoice, DifficultyDefinition>();

    public DifficultyFactory()
    {
        // TODO: Read this from a json file
        trialLevelDefinitions.Add(
            DifficultyChoice.BASE, 
            new DifficultyDefinition(
                "BASE",
                10.0 * 60,
                5,
                0,
                false,
                new Tuple<double, double>(2, 5),
                new List<bool> { 
                    false, false, false, false, false, true, true, true, true, true, true 
                },
                new List<RandomizableFloat> {
                    new RandomizableFloat(0),
                    new RandomizableFloat(0),
                    new RandomizableFloat(0),
                    new RandomizableFloat(0), 
                    new RandomizableFloat(0), 
                    new RandomizableFloat(0),
                    new RandomizableFloat(0),
                    new RandomizableFloat(0), 
                    new RandomizableFloat(0), 
                    new RandomizableFloat(0.02f), 
                    new RandomizableFloat(0.02f) },
                new List<RandomizableFloat> {
                    new RandomizableFloat(0.3f),
                    new RandomizableFloat(0.3f),
                    new RandomizableFloat(0.4f),
                    new RandomizableFloat(0.45f),
                    new RandomizableFloat(0.5f),
                    new RandomizableFloat(0.55f),
                    new RandomizableFloat(0.6f),
                    new RandomizableFloat(0.7f),
                    new RandomizableFloat(0.9f),
                    new RandomizableFloat(1.0f, 0.1f),
                    new RandomizableFloat(1.1f, 0.15f)
                },
                new List<RandomizableFloat> {
                    new RandomizableFloat(0f),
                    new RandomizableFloat(0f),
                    new RandomizableFloat(0f),
                    new RandomizableFloat(0f),
                    new RandomizableFloat(0f),
                    new RandomizableFloat(0.04f),
                    new RandomizableFloat(0.04f),
                    new RandomizableFloat(0.0375f),
                    new RandomizableFloat(0.035f),
                    new RandomizableFloat(0.0325f),
                    new RandomizableFloat(0.03f)
                },
                (int _currentLevel, TrialData _trialData, DifficultyDefinition _difficultyDefinition) => {
                    // Reminder 0 if bounces < required as they are int
                    return _trialData.nbBounces >= _difficultyDefinition.nbOfBounceRequired;
                }
            )
        );

        trialLevelDefinitions.Add(
            DifficultyChoice.MODERATE,
            new DifficultyDefinition(
                "MODERATE",
                10.0 * 60,
                5,
                5,
                false,
                new Tuple<double, double>(6, 10),
                new List<bool> {
                    false, false, false, false, false, true, true, true, true, true, true 
                },
                new List<RandomizableFloat> {
                    new RandomizableFloat(0),
                    new RandomizableFloat(0),
                    new RandomizableFloat(0),
                    new RandomizableFloat(0),
                    new RandomizableFloat(0),
                    new RandomizableFloat(0),
                    new RandomizableFloat(0),
                    new RandomizableFloat(0),
                    new RandomizableFloat(0),
                    new RandomizableFloat(0.02f),
                    new RandomizableFloat(0.02f) },
                new List<RandomizableFloat> {
                    new RandomizableFloat(0.3f),
                    new RandomizableFloat(0.3f),
                    new RandomizableFloat(0.4f),
                    new RandomizableFloat(0.45f),
                    new RandomizableFloat(0.5f),
                    new RandomizableFloat(0.55f),
                    new RandomizableFloat(0.6f),
                    new RandomizableFloat(0.7f),
                    new RandomizableFloat(0.9f),
                    new RandomizableFloat(1.0f, 0.1f),
                    new RandomizableFloat(1.1f, 0.15f)
                },
                new List<RandomizableFloat> {
                    new RandomizableFloat(0f),
                    new RandomizableFloat(0f),
                    new RandomizableFloat(0f),
                    new RandomizableFloat(0f),
                    new RandomizableFloat(0f),
                    new RandomizableFloat(0.04f),
                    new RandomizableFloat(0.04f),
                    new RandomizableFloat(0.0375f),
                    new RandomizableFloat(0.035f),
                    new RandomizableFloat(0.0325f),
                    new RandomizableFloat(0.03f)
                },
                (int _currentLevel, TrialData _trialData, DifficultyDefinition _difficultyDefinition) =>
                {
                    // Reminder 0 if bounces < required as they are int
                    if (_difficultyDefinition.hasTarget(_currentLevel)) 
                        return _trialData.nbAccurateBounces >= _difficultyDefinition.nbOfAccurateBounceRequired;
                    else
                        return _trialData.nbBounces >= _difficultyDefinition.nbOfBounceRequired;
                }
            )
        );
        trialLevelDefinitions.Add(
            DifficultyChoice.MAXIMAL,
            new DifficultyDefinition(
                "MAXIMAL",
                10.0 * 60,
                5,
                5,
                false,
                new Tuple<double, double>(6, 10),
                new List<bool> {
                    false, false, false, false, false, true, true, true, true, true, true 
                },
                new List<RandomizableFloat> {
                    new RandomizableFloat(0),
                    new RandomizableFloat(0),
                    new RandomizableFloat(0),
                    new RandomizableFloat(0),
                    new RandomizableFloat(0),
                    new RandomizableFloat(0),
                    new RandomizableFloat(0),
                    new RandomizableFloat(0),
                    new RandomizableFloat(0),
                    new RandomizableFloat(0.02f),
                    new RandomizableFloat(0.02f) },
                new List<RandomizableFloat> {
                    new RandomizableFloat(0.3f),
                    new RandomizableFloat(0.3f),
                    new RandomizableFloat(0.4f),
                    new RandomizableFloat(0.45f),
                    new RandomizableFloat(0.5f),
                    new RandomizableFloat(0.55f),
                    new RandomizableFloat(0.6f),
                    new RandomizableFloat(0.7f),
                    new RandomizableFloat(0.9f),
                    new RandomizableFloat(1.0f, 0.1f),
                    new RandomizableFloat(1.1f, 0.15f)
                },
                new List<RandomizableFloat> {
                    new RandomizableFloat(0f),
                    new RandomizableFloat(0f),
                    new RandomizableFloat(0f),
                    new RandomizableFloat(0f),
                    new RandomizableFloat(0f),
                    new RandomizableFloat(0.04f),
                    new RandomizableFloat(0.04f),
                    new RandomizableFloat(0.0375f),
                    new RandomizableFloat(0.035f),
                    new RandomizableFloat(0.0325f),
                    new RandomizableFloat(0.03f)
                },
                (int _currentLevel, TrialData _trialData, DifficultyDefinition _difficultyDefinition) =>
                {
                    // Reminder 0 if bounces < required as they are int
                    return _trialData.nbAccurateBounces >= _difficultyDefinition.nbOfAccurateBounceRequired;
                }
            )
        );
    }
}