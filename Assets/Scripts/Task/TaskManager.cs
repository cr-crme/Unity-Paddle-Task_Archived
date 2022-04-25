using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager
{
    public bool mustSwitchPaddle { get; private set; }

    public TaskManager(bool _mustSwitchPaddle)
    {
        mustSwitchPaddle = _mustSwitchPaddle;
    }
}
