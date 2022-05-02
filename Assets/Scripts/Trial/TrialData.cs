/// <summary>
/// A class that stores info on each trial relevant to data recording. Every field is
/// public readonly, so can always be accessed, but can only be assigned once in the
/// constructor.
/// </summary>
/// 

public class TrialData
{
    public float time { get; private set; }
    public int nbBounces { get; private set; }
    public int nbAccurateBounces { get; private set; }

    public TrialData(
        float _time,
        int _nbBounces,
        int _nbAccurateBounces
    )
    {
        time = _time;
        nbBounces = _nbBounces;
        nbAccurateBounces = _nbAccurateBounces;
    }

    public void AddBounce(bool isAccurate)
    {
        nbBounces++;
        if (isAccurate)
            nbAccurateBounces++;
    }
}