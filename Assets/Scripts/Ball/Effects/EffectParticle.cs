using UnityEngine;
using System.Collections;

[System.Serializable]
public class EffectParticle
{
    public VisualEffect effect;
    public GameObject particleParent;

    public EffectParticle(VisualEffect effectVar, GameObject particleParentVar)
    {
        effect = effectVar;
        particleParent = particleParentVar;
    }
}
