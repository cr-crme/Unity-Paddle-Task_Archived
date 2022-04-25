using UnityEngine;
using System.Collections;

public class HeaderData
{
    public readonly TaskType.Condition condition;
    public readonly TaskType.ExpCondition expCondition;
    public readonly TaskType.Session session;
    public readonly int maxTrialTimeMin;
    public readonly float hoverTime;
    public readonly float targetRadius;

    public HeaderData(TaskType.Condition c, TaskType.ExpCondition ec, TaskType.Session s, int maxtime, float htime, float tradius)
    {
        this.condition = c;
        this.expCondition = ec;
        this.session = s;
        this.maxTrialTimeMin = maxtime;
        this.hoverTime = htime;
        this.targetRadius = tradius;
    }
}