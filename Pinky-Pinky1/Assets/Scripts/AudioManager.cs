using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("-----------Audio Sources-----------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("-----------Audio Clip-----------")]
    public AudioClip background;
    public AudioClip DoorBanging;
    public AudioClip Flicker;
    public AudioClip Droplet;
    public AudioClip RunningWater;
    public AudioClip ChildScream;
    public AudioClip Gulp;
    public AudioClip deepBreath;
    public AudioClip MultipleBreaths;
    public AudioClip EvilLaugh;
    public AudioClip ClockTick;
    public AudioClip ClockAlarm;

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        //if (PauseMenu.GameIsPaused)

        SFXSource.PlayOneShot(clip);
    }

    // New public methods to control music
    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            Debug.Log("Music stopped");
        }
    }

    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
            Debug.Log("Music paused");
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
            Debug.Log("Music resumed");
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }
    }

    public float GetMusicVolume()
    {
        return musicSource != null ? musicSource.volume : 0f;
    }

    public bool IsMusicPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }

    // Coroutine for fading out music
    public System.Collections.IEnumerator FadeOutMusic(float duration)
    {
        if (musicSource == null) yield break;

        float startVolume = musicSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume; // Reset volume for next scene
        Debug.Log("Music faded out");
    }

    // Coroutine for fading in music
    public System.Collections.IEnumerator FadeInMusic(float duration, float targetVolume = 1f)
    {
        if (musicSource == null) yield break;

        musicSource.volume = 0f;
        musicSource.Play();

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsedTime / duration);
            yield return null;
        }

        musicSource.volume = targetVolume;
        Debug.Log("Music faded in");
    }

}
