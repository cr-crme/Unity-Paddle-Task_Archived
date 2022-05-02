using UnityEngine;
using System.Collections.Generic;

public class FullEffect
{
	public VisualEffect visualEffect { get; private set; }
	public AudioClip audioClip { get; private set; }
	public List<VisualEffect> disableEffects { get; private set; }

	public FullEffect(VisualEffect _visualEffet, AudioClip _audioEffect, List<VisualEffect> _disableEffects)
	{
		visualEffect = _visualEffet;
		audioClip = _audioEffect;
		disableEffects = _disableEffects != null ? _disableEffects : new List<VisualEffect>();
	}
}
