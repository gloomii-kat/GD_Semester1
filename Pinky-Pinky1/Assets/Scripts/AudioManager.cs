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

}
