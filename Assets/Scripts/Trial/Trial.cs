/// <summary>
/// A class that stores info on each trial relevant to data recording. Every field is
/// public readonly, so can always be accessed, but can only be assigned once in the
/// constructor.
/// </summary>
/// 

public class Trial
{
    public float time { get; private set; }
    public int nbBounces { get; private set; }
    public int nbAccurateBounces { get; private set; }

    public Trial(float _time)
    {
        time = _time;
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