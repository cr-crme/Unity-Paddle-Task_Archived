using UnityEngine;
using System.Collections.Generic;

public class FullEffect
{
    public VideoEffect visualEffect { get; private set; }
    public AudioClip audioClip { get; private set; }
    public List<VideoEffect> disableEffects { get; private set; }

    public FullEffect(VideoEffect _visualEffet, AudioClip _audioEffect, List<VideoEffect> _disableEffects)
    {
        visualEffect = _visualEffet;
        audioClip = _audioEffect;
        disableEffects = _disableEffects != null ? _disableEffects : new List<VideoEffect>();
    }
}
