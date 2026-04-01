using UnityEngine;
using System.Collections;   
using System.Collections.Generic;   
using UnityEngine.UI;

public class CooldownTimer_bar : MonoBehaviour
{
    [SerializeField] private Image uiFill; // Reference to the UI Image component representing the cooldown bar
    [SerializeField] private Text uiTxt;

    public int Duration;
    private int remainingDuration;
    private Coroutine currentCoroutine;

    AudioManager AudioManager;

    private void Awake()
    {
        AudioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    private void Start()
    {
        // Don't start automatically - wait for StartCooldown to be called
        if (uiFill != null)
            uiFill.fillAmount = 1f;
    }

    public void StartCooldown(int duration)
    {
        // Stop any existing cooldown
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        // Set duration and start
        Duration = duration;
        Begin(Duration);
    }

    private void Begin(int Second)
    {
        remainingDuration = Second;
        if (uiFill != null)
            uiFill.fillAmount = 1f;
        currentCoroutine = StartCoroutine(UpdateTimer());
    }

    private IEnumerator UpdateTimer()
    {
        while (remainingDuration > 0) // Change to > 0 to handle the last second specifically
        {
            // Play tick sound at the start of every second
            //if (AudioManager != null)
            //{
            //    AudioManager.PlaySFX(AudioManager.ClockTick);
            //}

            // Update UI
            if (uiTxt != null) uiTxt.text = $"{remainingDuration % 60:00}";
            if (uiFill != null) uiFill.fillAmount = Mathf.InverseLerp(0, Duration, remainingDuration);

            remainingDuration--;
            yield return new WaitForSeconds(1f);
        }

        // Timer is now officially 0
        if (uiTxt != null) uiTxt.text = "00";
        if (uiFill != null) uiFill.fillAmount = 0;

        // Play completion sound
        if (AudioManager != null)
        {
            AudioManager.PlaySFX(AudioManager.ClockAlarm);
        }else         {
            Debug.LogWarning("AudioManager not found. Cannot play ClockAlarm sound.");
        }

        gameObject.SetActive(false);
        currentCoroutine = null;
    }
}
