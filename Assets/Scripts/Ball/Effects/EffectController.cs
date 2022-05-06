using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// facilitate management of effects. handles setting shader and particla effects. 
/// </summary>
public class EffectController : MonoBehaviour
{
    private BallSoundManager soundManager;

    public VideoEffect dissolve, respawn, fire, blueFire, embers, blueEmbers;
    public VideoEffect effectTarget;
    VideoEffect activeShaderEffect;

    [SerializeField] private List<FullEffect> scoreDependentEffects;

    void Start()
    {
        soundManager = GetComponent<BallSoundManager>();

        InitializeParticleEffect(dissolve);
        InitializeParticleEffect(respawn);
        respawn.effectTime = GlobalPreferences.Instance.ballResetHoverSeconds;
        InitializeParticleEffect(fire);
        InitializeParticleEffect(blueFire);
        InitializeParticleEffect(embers);
        InitializeParticleEffect(blueEmbers);

        SanityCheckForScoreDependentEffects();
    }

    void InitializeParticleEffect(VideoEffect effect)
    {
        effect.gameObject.SetActive(false);

        EffectParticle effectParticle = effect.GetEffectParticle(effect);
        if (effectParticle == null)
        {
            return;
        }

        GameObject particleParent = effectParticle.particleParent;
        if (particleParent)
        {
            particleParent.transform.SetParent(effectTarget.transform);
            effectTarget.effectParticles.Add(new EffectParticle(effect, particleParent));
        }
    }

    void SanityCheckForScoreDependentEffects()
    {
        // Sanity check
        for (int i = 1; i < scoreDependentEffects.Count; i++)
            if (scoreDependentEffects[i - 1].minimimBouncesBeforeActivating 
                >= scoreDependentEffects[i].minimimBouncesBeforeActivating)
                Debug.LogError("ERROR! Invalid Score effect must be in ascending order");
    }

    public void SelectScoreDependentEffects(float _score)
    {
        for (int i = scoreDependentEffects.Count - 1; i >= 0; i--)
        {
            // If the score is smaller than any minimal score, go to next (reversed order)
            if (_score < scoreDependentEffects[i].minimimBouncesBeforeActivating) continue;

            // If the score is equal, then start the current effect and stop the previous one
            if (_score == scoreDependentEffects[i].minimimBouncesBeforeActivating)
            {
                StartParticleEffect(scoreDependentEffects[i]);
                break;
            }

            // If the score is larger, break as everything will necessarily be larger (reversed order)
            if (_score > scoreDependentEffects[i].minimimBouncesBeforeActivating)
                break;
        }
    }

    public void StartParticleEffect(FullEffect effect)
    {
        foreach (var disableEffect in effect.disableEffects)
        {
            StopParticleEffect(disableEffect);
        }
        StartParticleEffect(effect.visualEffect);
        StartShaderEffect(effect.visualEffect);
        effect.visualEffect.StartEffect();
        PlaySound(effect.audioClip);
    }
    public void StopEffect(FullEffect effect)
    {
        effect.visualEffect.StopEffect();
        StopParticleEffect(effect.visualEffect);
    }

    private void PlaySound(AudioClip _audioClip)
    {
        soundManager.PlayEffectSound(_audioClip);
    }

    public void StartVisualEffect(VideoEffect effect)
    {
        StartParticleEffect(effect);
        StartShaderEffect(effect);

        effectTarget.StartEffect();
    }

    public void StopEffects()
    {
        effectTarget.StopEffect();
    }

    public void StartShaderEffect(VideoEffect effect)
    {
        if (activeShaderEffect == null || activeShaderEffect != effect)
        {
            activeShaderEffect = effect;
            effectTarget.SetEffectProperties(
                effect.effectTime, 
                effect.fadeIn, 
                effect.material,
                effect.ps,
                effect.shaderProperty
            );
        }

        if (effect.material != null)
        {
            effectTarget.renderer.material = effect.material;
        }
    }

    public void StartParticleEffect(VideoEffect effect)
    {
        EffectParticle effectParticle = effectTarget.GetEffectParticle(effect);
        if (effectParticle == null)
        {
            return;
        }

        var particleParent = effectParticle.particleParent;
        particleParent.gameObject.SetActive(true);
    }

    public void StopParticleEffect(VideoEffect effect)
    {
        var effectParticle = effectTarget.GetEffectParticle(effect);
        if (effectParticle == null)
        {
            return;
        }

        effectParticle.particleParent.gameObject.SetActive(false);
    }

    public void StopAllParticleEffects()
    {
        foreach (var particle in effectTarget.effectParticles)
        {
            particle.particleParent.SetActive(false);
        }
    }
}
