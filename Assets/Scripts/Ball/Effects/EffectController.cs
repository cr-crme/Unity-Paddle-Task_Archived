using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// facilitate management of effects. handles setting shader and particla effects. 
/// </summary>
public class EffectController : MonoBehaviour
{
    [SerializeField]
    private BallSoundManager soundManager;

    public VideoEffect dissolve, respawn, fire, blueFire, embers, blueEmbers;
    public VideoEffect effectTarget;
    VideoEffect activeShaderEffect;

    private List<Tuple<int, FullEffect>> scoreDependentEffects = new List<Tuple<int, FullEffect>>();

    void Start()
    {
        dissolve?.gameObject.SetActive(false);
        respawn?.gameObject.SetActive(false);
        fire?.gameObject.SetActive(false);
        blueFire?.gameObject.SetActive(false);
        embers?.gameObject.SetActive(false);
        blueEmbers?.gameObject.SetActive(false);

        Initialize();
        PopulateScoreDependentEffects();
    }

    void Initialize()
    {
        InitializeParticleEffect(dissolve);
        InitializeParticleEffect(respawn);
        InitializeParticleEffect(fire);
        InitializeParticleEffect(blueFire);
        InitializeParticleEffect(embers);
        InitializeParticleEffect(blueEmbers);
    }
    void InitializeParticleEffect(VideoEffect effect)
    {
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

    void PopulateScoreDependentEffects()
    {
        // Enter score effects in ascending order of the score needed to trigger them
        scoreDependentEffects.Add(new Tuple<int, FullEffect>(10, new FullEffect(embers, null, null)));
        scoreDependentEffects.Add(new Tuple<int, FullEffect>(20, new FullEffect(fire, null, null)));
        scoreDependentEffects.Add(new Tuple<int, FullEffect>(30, new FullEffect(blueEmbers, null, new List<VideoEffect>() { embers })));
        scoreDependentEffects.Add(new Tuple<int, FullEffect>(40, new FullEffect(blueFire, null, new List<VideoEffect>() { fire })));

        // Sanity check
        for (int i = 1; i < scoreDependentEffects.Count; i++)
            if (scoreDependentEffects[i - 1].Item1 >= scoreDependentEffects[i].Item1)
                Debug.LogError("ERROR! Invalid Score effect must be in ascending order");
    }

    public void SelectScoreDependentEffects(float _score)
    {
        for (int i = scoreDependentEffects.Count - 1; i >= 0; i--)
        {
            // If the score is smaller than any minimal score, go to next (reversed order)
            if (_score < scoreDependentEffects[i].Item1) continue;

            // If the score is equal, then start the current effect and stop the previous one
            if (_score == scoreDependentEffects[i].Item1)
            {
                StartEffect(scoreDependentEffects[i].Item2);
                if (i != 0)
                    StopEffect(scoreDependentEffects[i - 1].Item2);
                break;
            }

            // If the score is larger, break as everything will necessarily be larger (reversed order)
            if (_score > scoreDependentEffects[i].Item1)
                break;
        }
    }

    public void StartEffect(FullEffect effect)
    {
        foreach (var disableEffect in effect.disableEffects)
        {
            StopParticleEffect(disableEffect);
        }
        StartVisualEffect(effect.visualEffect);
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
            effectTarget.SetEffect(
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
            Debug.LogError("particle effect not found");
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
