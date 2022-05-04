using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class FullEffect
{
    public int minimimBouncesBeforeActivating;
    public VideoEffect visualEffect;
    public AudioClip audioClip;
    public List<VideoEffect> disableEffects;

    public FullEffect(
        int _minimimBouncesBeforeActivating,
        VideoEffect _visualEffet, 
        AudioClip _audioEffect,
        List<VideoEffect> _disableEffects
    )
    {
        minimimBouncesBeforeActivating = _minimimBouncesBeforeActivating;
        visualEffect = _visualEffet;
        audioClip = _audioEffect;
        disableEffects = _disableEffects != null ? _disableEffects : new List<VideoEffect>();
    }
}
