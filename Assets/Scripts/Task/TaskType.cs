using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskType
{
    public enum Condition { REGULAR, ENHANCED, REDUCED, TARGETLINE };
    public enum Session { BASELINE, ACQUISITION, RETENTION, TRANSFER, SHOWCASE };
    public enum TargetHeight { DEFAULT, LOWERED, RAISED };
    public enum ExpCondition { RANDOM = 0, HEAVIEST = 1, HEAVIER = 2, NORMAL = 3, LIGHTER = 4, LIGHTEST = 5 };
    public enum DifficultyEvaluation { BASE, MODERATE, MAXIMAL, CUSTOM };
    public enum Mindset { GROWTH, CONTROL };
}
