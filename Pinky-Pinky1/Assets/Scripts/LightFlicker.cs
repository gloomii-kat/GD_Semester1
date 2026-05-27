 using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LightFlicker : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject yellowPanel;
    public GameObject blackPanel;
    public UnityEngine.UI.Image yellowLightSprite;
    public UnityEngine.UI.Image blackLightSprite;
    public UnityEngine.UI.Text interactionText;

    [Header("Flicker Settings")]
    public float flickerInterval = 0.15f;
    public int flickerCount = 6;

    [Header("Random Flicker")]
    public bool randomizeInterval = true;
    public float minFlickerInterval = 0.05f;
    public float maxFlickerInterval = 0.25f;

    [Header("Child Reference")]
    public GameObject childObject; // Reference to the Child AI GameObject
    public float escapeSpeedMultiplier = 2f; // How fast the child runs away

    [Header("Audio")]
    private AudioManager AudioManager;

    private bool isYellowActive = true;
    private bool playerInRange = false;
    private bool littleGirlInRange = false;
    private bool isScaring = false;

    private void Awake()
    {
        AudioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    void Start()
    {
        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
    }

    void Update()
    {
        bool bothInRange = playerInRange && littleGirlInRange;

        // Show interaction text if both are in range and not scaring
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(bothInRange && !isScaring);
        }

        // Player presses E while both are in trigger and not already scaring
        if (bothInRange && Input.GetKeyDown(KeyCode.E) && !isScaring)
        {
            StartCoroutine(FlickerScare());
        }
    }

    IEnumerator FlickerScare()
    {
        isScaring = true;

        // Hide interaction text immediately
        if (interactionText != null)
            interactionText.gameObject.SetActive(false);

        // Make the child run away IMMEDIATELY
        TriggerChildEscape();

        // Play sound
        if (AudioManager != null)
        {
            AudioManager.PlaySFX(AudioManager.Flicker);
        }

        bool originalState = isYellowActive;

        // Do the flicker effect (this happens while child is already running)
        for (int i = 0; i < flickerCount; i++)
        {
            isYellowActive = !isYellowActive;
            UpdateLightVisuals();

            float currentInterval = flickerInterval;
            if (randomizeInterval)
                currentInterval = Random.Range(minFlickerInterval, maxFlickerInterval);

            yield return new WaitForSeconds(currentInterval);
        }

        // Restore light to original state
        isYellowActive = originalState;
        UpdateLightVisuals();

        isScaring = false;
    }

    void TriggerChildEscape()
    {
        if (childObject != null)
        {
            ChildAI childAI = childObject.GetComponent<ChildAI>();
            if (childAI != null)
            {
                childAI.TriggerEscape(escapeSpeedMultiplier);
                Debug.Log("Light flicker scared the child! Child is now escaping!");
            }
            else
            {
                Debug.LogError("ChildAI component not found on childObject!");
            }
        }
        else
        {
            Debug.LogError("Child Object reference is not assigned in LightFlicker!");
        }
    }

    void UpdateLightVisuals()
    {
        if (yellowPanel != null) yellowPanel.SetActive(isYellowActive);
        if (blackPanel != null) blackPanel.SetActive(!isYellowActive);
        if (yellowLightSprite != null) yellowLightSprite.enabled = isYellowActive;
        if (blackLightSprite != null) blackLightSprite.enabled = !isYellowActive;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player entered light trigger zone");
        }
        if (other.CompareTag("LittleGirl"))
        {
            littleGirlInRange = true;
            Debug.Log("Little girl entered light trigger zone");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactionText != null)
                interactionText.gameObject.SetActive(false);
        }
        if (other.CompareTag("LittleGirl"))
        {
            littleGirlInRange = false;
        }
    }

    public void TriggerFlickerScare()
    {
        if (!isScaring && playerInRange && littleGirlInRange)
            StartCoroutine(FlickerScare());
    }
}