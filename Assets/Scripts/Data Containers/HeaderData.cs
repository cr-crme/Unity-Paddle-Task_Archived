using UnityEngine;
using System.Collections;

public class HeaderData
{
    public readonly TaskType.Session session;
    public readonly int maxTrialTimeMin;
    public readonly float hoverTime;
    public readonly float targetRadius;

    public HeaderData(TaskType.Session s, int maxtime, float htime, float tradius)
    {
        this.session = s;
        this.maxTrialTimeMin = maxtime;
        this.hoverTime = htime;
        this.targetRadius = tradius;
    }
}