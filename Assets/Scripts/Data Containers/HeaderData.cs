﻿using UnityEngine;
using System.Collections;

public class HeaderData
{
    public readonly SessionType.Session session;
    public readonly int maxTrialTimeMin;
    public readonly float hoverTime;
    public readonly float targetWidth;

    public HeaderData(SessionType.Session s, int maxtime, float htime, float twidth)
    {
        this.session = s;
        this.maxTrialTimeMin = maxtime;
        this.hoverTime = htime;
        this.targetWidth = twidth;
    }
}