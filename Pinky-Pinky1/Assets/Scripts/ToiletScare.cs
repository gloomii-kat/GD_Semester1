using UnityEngine;

public class ToiletScare : MonoBehaviour
{
    public GameObject toiletText;       // "Press E" prompt
    private AudioManager AudioManager;  // Scare sound

    private bool playerInRange = false;
    private bool littleGirlInRange = false;

    private void Awake()
    {
        AudioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

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
    }

    void Update()
    {
        bool bothInRange = playerInRange && littleGirlInRange;

        // Show prompt if both are in range
        if (toiletText != null)
        {
            toiletText.SetActive(bothInRange);

            if (bothInRange)
            {
                Debug.Log("Both in range - showing text");
            }
        }

        // Player presses E while both are in trigger
        if (bothInRange && Input.GetKeyDown(KeyCode.E))
        {
            StartScare();
        }
    }

    void StartScare()
    {
        Debug.Log("E pressed - Scare triggered!");

        // Play sound
        if (AudioManager != null)
        {
            AudioManager.PlaySFX(AudioManager.DoorBanging);
        }

        // You can add other scare effects here (camera shake, particle effects, etc.)
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
