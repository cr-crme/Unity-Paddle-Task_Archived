/// <summary>
/// A class that stores info on each trial relevant to data recording. Every field is
/// public readonly, so can always be accessed, but can only be assigned once in the
/// constructor.
/// </summary>
/// 
using UnityEngine;

public class Trial
{
    private float _startTime;
    public float time { get { return Time.time - _startTime; } }
    public int nbBounces { get; private set; }
    public int nbAccurateBounces { get; private set; }

    public Trial(float _startingTime)
    {
        _startTime = _startingTime;
    }

    public void AddBounce()
    {
        nbBounces++;
    }
    public void AddAccurateBounce()
    {
        nbAccurateBounces++;
    }
}