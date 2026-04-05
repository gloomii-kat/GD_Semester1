using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BasinScare : MonoBehaviour
{
    public GameObject BasinText;       // "Press E" prompt
    AudioManager AudioManager;     // Scare sound
    public AwarenessScript awarenessScript; // Reference to the AwarenessBar
    public CooldownTimer_bar cooldownBar; // Reference to the cooldown timer bar

    private bool playerInRange = false;
    private bool littleGirlInRange = false;
    private bool isOnCooldown = false;  // Track if cooldown is active

    public int scareAmount = 10; // Amount to increase awareness by
    public int cooldownDuration = 20; // Cooldown duration in seconds

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        AudioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    void Start()
    {
        // Make sure text is hidden at start
        if (BasinText != null)
        {
            BasinText.SetActive(false);
            Debug.Log("Basin text initialized to false");
        }
        else
        {
            Debug.LogError("Basin Text is not assigned in the inspector!");
        }

        // Initialize cooldown bar
        if (cooldownBar != null)
        {
            cooldownBar.Duration = cooldownDuration;
            // Make sure the cooldown bar is initially hidden
            cooldownBar.gameObject.SetActive(false);
            Debug.Log("Cooldown bar initialized");
        }
        else
        {
            Debug.LogError("Cooldown Bar is not assigned in the inspector!");
        }

        // Initialize awareness to 0 if needed
        if (awarenessScript != null)
        {
            // Set max value
            awarenessScript.SetMaxAwareness(150);
            // Set starting value to 0
            awarenessScript.SetAwareness(0);
            Debug.Log("Awareness initialized to 0, max set to 150");
        }
        else
        {
            Debug.LogError("Awareness Script is not assigned in the inspector!");
        }

      
    }

    void Update()
    {
        bool bothInRange = playerInRange && littleGirlInRange;

        // Only show prompt if both are in range AND not on cooldown
        if (BasinText != null)
        {
            BasinText.SetActive(bothInRange && !isOnCooldown);

            // Debug log when text state changes
            if (bothInRange && !isOnCooldown)
            {
                Debug.Log("Both in range, not on cooldown - showing press E text");
            }
            else if (bothInRange && isOnCooldown)
            {
                Debug.Log("Both in range but on cooldown - hiding press E text");
            }
        }

        // Player presses E while both are in trigger AND not on cooldown
        if (bothInRange && !isOnCooldown && Input.GetKeyDown(KeyCode.E))
        {
            StartScare();
        }
    }

    void StartScare()
    {
        Debug.Log("StartScare() called - Starting cooldown");

        // Set cooldown
        isOnCooldown = true;

        // Show and start cooldown bar
        if (cooldownBar != null)
        {
            cooldownBar.gameObject.SetActive(true);
            cooldownBar.StartCooldown(cooldownDuration);
            Debug.Log("Cooldown bar activated and started for " + cooldownDuration + " seconds");
        }
        else
        {
            Debug.LogError("cooldownBar is NULL in StartScare!");
        }

        // Increase awareness
        ScareHerAss(scareAmount);
        Debug.Log("E pressed - Scare triggered! Awareness +" + scareAmount);

        // Play sound
        if (AudioManager != null)
        {
            AudioManager.PlaySFX(AudioManager.RunningWater);
        }

        // Start cooldown routine
        StartCoroutine(CooldownRoutine());
    }

    System.Collections.IEnumerator CooldownRoutine()
    {
        // Wait for cooldown duration
        yield return new WaitForSeconds(cooldownDuration);

        // End cooldown
        isOnCooldown = false;

        // Hide cooldown bar
        if (cooldownBar != null)
        {
            cooldownBar.gameObject.SetActive(false);
        }

        Debug.Log("Cooldown ended - E press available again");
    }

    void ScareHerAss(int amount)
    {
        if (awarenessScript != null && awarenessScript.slider != null)
        {
            // Get current value from slider, add amount
            float newValue = awarenessScript.slider.value + amount;

            // Clamp between min and max
            newValue = Mathf.Clamp(newValue, awarenessScript.slider.minValue, awarenessScript.slider.maxValue);

            // Set the new value
            awarenessScript.SetAwareness((int)newValue);

            Debug.Log("Awareness increased from " + (awarenessScript.slider.value - amount) + " to " + newValue);
        }
        else
        {
            if (awarenessScript == null)
                Debug.LogError("AwarenessScript is null in ScareHerAss!");
            else if (awarenessScript.slider == null)
                Debug.LogError("Slider is null in AwarenessScript!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger entered by: " + other.gameObject.name + " with tag: " + other.tag);

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player entered trigger - Player in range: " + playerInRange);
        }
        if (other.CompareTag("LittleGirl"))
        {
            littleGirlInRange = true;
            Debug.Log("Little girl entered trigger - Girl in range: " + littleGirlInRange);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("Trigger exited by: " + other.gameObject.name + " with tag: " + other.tag);

        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Player left trigger - Player in range: " + playerInRange);

            // Hide text when player leaves
            if (BasinText != null)
                BasinText.SetActive(false);
        }
        if (other.CompareTag("LittleGirl"))
        {
            littleGirlInRange = false;
            Debug.Log("Little girl left trigger - Girl in range: " + littleGirlInRange);
        }
    }

    void OnDrawGizmos()
    {
        // Draw a small sphere to show trigger status
        Gizmos.color = (playerInRange && littleGirlInRange && !isOnCooldown) ? Color.green :
                       (playerInRange && littleGirlInRange && isOnCooldown) ? Color.yellow : Color.red;
        Gizmos.DrawSphere(transform.position + Vector3.up, 0.3f);
    }
}
