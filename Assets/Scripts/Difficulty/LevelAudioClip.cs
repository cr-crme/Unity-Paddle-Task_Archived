using UnityEngine;

[System.Serializable]
public class LevelAudioClip
{
    public AudioClip audioClip;
    public int level;

    public LevelAudioClip(AudioClip _audioClip, int _associatedLevel)
    {
        audioClip = _audioClip;
        level = _associatedLevel;
    }
}
