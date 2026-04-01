using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer; // Reference to the AudioMixer
    [SerializeField] private Slider musicSlider; // Reference to the UI Slider for volume control

    public void SetMusicVolume()
    {
        float volume = musicSlider.value; // Get the slider value (0 to 1)
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume)*20); // Convert to decibels and set the volume
    }
}
