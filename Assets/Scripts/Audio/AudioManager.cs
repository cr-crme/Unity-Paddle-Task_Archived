using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    [SerializeField]
    private AudioSource levelAudioSource;

    /// <summary>
    /// list of the audio clips played at the beginning of difficulties in some cases
    /// </summary>
    [SerializeField]
    List<LevelAudioClip> showcaseLevelAudioClips = new List<LevelAudioClip>();

    public void PlayShowcaseDifficultyAudioClip(int _level)
    {
        foreach (LevelAudioClip clip in showcaseLevelAudioClips)
        {
            if (clip.level == _level)
            {
                levelAudioSource.PlayOneShot(clip.audioClip);
            }
        }
    }
}
