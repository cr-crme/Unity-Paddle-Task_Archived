using System.Collections;
using UnityEngine;

public class BallSoundManager : MonoBehaviour {
    
    public AudioSource dropSound;
    public AudioSource successSound;
    public AudioSource effectSource;
    public AudioSource[] bounceSounds;

    // Play high pitch sound when ball is about to drop.
    public void PlayDropSound()
    {
        // Stop the bounce sounds that are currently playing. We dont want to overload the audio.
        dropSound.Stop();
        dropSound.PlayOneShot(dropSound.clip);  
    }

    // Play success sound when ball apex is within target window
    public IEnumerator PlaySuccessSound(float time = 0.0f)
    {
        yield return new WaitForSeconds(time);
        successSound.Stop();
        successSound.PlayOneShot(successSound.clip);
    }
    
    // Non-coroutine version of above function
    public void PlaySuccessSound()
    {
        successSound.Stop();
        successSound.PlayOneShot(successSound.clip);
    }

    public void PlayEffectSound(AudioClip audioClip)
    {
        if (audioClip != null)
        {
            effectSource.PlayOneShot(audioClip);
        }
    }

    // Play a random bounce sound from the array
    public void PlayBounceSound()
    {
        AudioSource sound = bounceSounds[Random.Range(0, bounceSounds.Length)];
        sound.PlayOneShot(sound.clip);
    }
}
