using UnityEngine;
using System.Collections;

[System.Serializable]
public class EffectParticle
{
    public VideoEffect effect;
    public GameObject particleParent;

    public EffectParticle(VideoEffect effectVar, GameObject particleParentVar)
    {
        effect = effectVar;
        particleParent = particleParentVar;
    }
}
