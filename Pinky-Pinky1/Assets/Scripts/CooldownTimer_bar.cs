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
        while (remainingDuration >= 0)
        {
           
            int seconds = remainingDuration % 60;

            if (uiTxt != null)
            {
                // Show minutes:seconds format
                uiTxt.text = $"{seconds:00}";
            }

            // Update the fill amount of the cooldown bar
            if (uiFill != null)
            {
                uiFill.fillAmount = Mathf.InverseLerp(0, Duration, remainingDuration);
            }

            remainingDuration--; // Decrease the remaining duration
            yield return new WaitForSeconds(1f);
        }

        // Cooldown finished - hide the bar
        gameObject.SetActive(false);
        currentCoroutine = null;
    }
}
