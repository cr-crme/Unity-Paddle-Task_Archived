using System.Collections;
using UnityEngine;

public class BallSoundManager : MonoBehaviour {

    public AudioSource dropBallAtStartSound;
    public AudioSource respawnBall;
    public AudioSource successSound;
    public AudioSource effectSource;
    public AudioSource[] bounceSounds;

    private void Awake()
    {
        dropBallAtStartSound.Stop();
        respawnBall.Stop();
        successSound.Stop();
        effectSource.Stop();
        foreach (AudioSource sound in bounceSounds)
            sound.Stop();
    }

    // Play high pitch sound when ball is about to drop.
    public void PlayDropBallSound()
    {
        // Stop the bounce sounds that are currently playing. We dont want to overload the audio.
        dropBallAtStartSound.Stop();
        dropBallAtStartSound.PlayOneShot(dropBallAtStartSound.clip);
    }

    // Play a sound while the ball respawns
    public void PlayRespawnBallSound()
    {
        // Stop the bounce sounds that are currently playing. We dont want to overload the audio.
        respawnBall.Stop();
        respawnBall.PlayOneShot(respawnBall.clip);  
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
