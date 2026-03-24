using UnityEngine;

public class ToiletScare : MonoBehaviour
{
    public GameObject toiletText;       // "Press E" prompt
    public AudioSource scareAudio;      // Scare sound
    public AwarenessScript awarenessScript; // Reference to the AwarenessBar

    private bool playerInRange = false;
    private bool littleGirlInRange = false;
  
    public int scareAmount = 10; // Amount to increase awareness by

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Make sure text is hidden at start
        if (toiletText != null)
        {
            toiletText.SetActive(false);
            Debug.Log("Toilet text initialized to false");
        }
        else
        {
            Debug.LogError("Toilet Text is not assigned in the inspector!");
        }

        // Initialize awareness to 0 if needed
        if (awarenessScript != null)
        {
            // Set max value (assuming you want max awareness to be 100)
            awarenessScript.SetMaxAwareness(150);
            // Set starting value to 0
            awarenessScript.SetAwareness(0);
            Debug.Log("Awareness initialized to 0, max set to 100");
        }
        else
        {
            Debug.LogError("Awareness Script is not assigned in the inspector!");
        }

        // Check audio source
        if (scareAudio == null)
        {
            Debug.LogError("Scare Audio is not assigned in the inspector!");
        }
    }

    void Update()
    {
        bool bothInRange = playerInRange && littleGirlInRange;

        // Only show prompt if both are in the trigger
        if (toiletText != null)
        {
            toiletText.SetActive(bothInRange);

            // Debug log when text state changes
            if (bothInRange)
            {
                Debug.Log("Both in range - showing text");
            }
        }

        // Player presses E while both are in trigger
        if (bothInRange && Input.GetKeyDown(KeyCode.E))
        {
            ScareHerAss(scareAmount);
            Debug.Log("E pressed - Scare triggered! Awareness +" + scareAmount);

            if (scareAudio != null)
            {
                scareAudio.Play();
                Debug.Log("Playing scare sound");
            }
            else
            {
                Debug.LogError("Cannot play sound - AudioSource is null!");
            }

           
        }
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
            if (toiletText != null)
                toiletText.SetActive(false);
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
        Gizmos.color = (playerInRange && littleGirlInRange) ? Color.green : Color.red;
        Gizmos.DrawSphere(transform.position + Vector3.up, 0.3f);
    }
}
