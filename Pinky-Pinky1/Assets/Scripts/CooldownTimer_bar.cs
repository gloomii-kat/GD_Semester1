using UnityEngine;
using System.Collections;   
using System.Collections.Generic;   
using UnityEngine.UI;

public class CooldownTimer_bar : MonoBehaviour
{
    [SerializeField] private Image uiFill;
    [SerializeField] private Text uiTxt;

    public int Duration;
    private int remainingDuration;
    private Coroutine currentCoroutine;

    private AudioManager AudioManager;

    private void Awake()
    {
        // Find AudioManager
        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
        {
            AudioManager = audioObj.GetComponent<AudioManager>();
        }
        else
        {
            AudioManager = FindFirstObjectByType<AudioManager>();
        }
    }

    private void Start()
    {
        if (uiFill != null)
            uiFill.fillAmount = 1f;
    }

    public void StartCooldown(int duration)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        Duration = duration;
        Begin(Duration);
    }

    private void Begin(int Second)
    {
        remainingDuration = Second;
        if (uiFill != null)
            uiFill.fillAmount = 1f;
        if (uiTxt != null)
            uiTxt.text = $"{Second:00}";

        currentCoroutine = StartCoroutine(UpdateTimer());
    }

    private IEnumerator UpdateTimer()
    {
        while (remainingDuration > 0)
        {
            // Update UI
            if (uiTxt != null)
                uiTxt.text = $"{remainingDuration % 60:00}";
            if (uiFill != null)
                uiFill.fillAmount = Mathf.InverseLerp(0, Duration, remainingDuration);

            remainingDuration--;
            yield return new WaitForSeconds(1f);
        }

        // Timer finished - Play alarm
        if (uiTxt != null)
            uiTxt.text = "00";
        if (uiFill != null)
            uiFill.fillAmount = 0;

        // Play the alarm sound
        if (AudioManager != null && AudioManager.ClockAlarm != null)
        {
            AudioManager.PlaySFX(AudioManager.ClockAlarm);
            Debug.Log("Alarm played!");
        }
        else
        {
            Debug.LogWarning("Can't play alarm: AudioManager or ClockAlarm clip is missing!");
        }

        gameObject.SetActive(false);
        currentCoroutine = null;
    }

    public void StopTimer()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
        gameObject.SetActive(false);
    }


}
