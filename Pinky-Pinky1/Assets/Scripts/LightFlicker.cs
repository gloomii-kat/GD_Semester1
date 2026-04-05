 using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LightFlicker : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject yellowPanel;
    public GameObject blackPanel;
    public Image yellowLightSprite;
    public Image blackLightSprite;
    public Text interactionText;

    [Header("Awareness Bar")]
    public AwarenessScript awarenessScript;
    public int awarenessIncreaseAmount = 10;

    [Header("Flicker Settings")]
    public float flickerInterval = 0.15f;
    public int flickerCount = 6;

    [Header("Random Flicker")]
    public bool randomizeInterval = true;
    public float minFlickerInterval = 0.05f;
    public float maxFlickerInterval = 0.25f;

    [Header("Cooldown")]
    public float cooldownDuration = 5f;
    private bool isOnCooldown = false;

    [Header("Audio")]
    AudioManager AudioManager;

    private bool isYellowActive = true;
    private bool playerInRange = false;
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
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isScaring && !isOnCooldown)
        {
            StartCoroutine(FlickerScare());
        }
    }

    IEnumerator FlickerScare()
    {
        isScaring = true;
        isOnCooldown = true;

        if (AudioManager != null)
        {
            AudioManager.PlaySFX(AudioManager.Flicker);
        }

        // Hide interaction text when cooldown starts
        if (playerInRange && interactionText != null)
            interactionText.gameObject.SetActive(false);

        bool originalState = isYellowActive;

        for (int i = 0; i < flickerCount; i++)
        {
            isYellowActive = !isYellowActive;
            UpdateLightVisuals();

            float currentInterval = flickerInterval;
            if (randomizeInterval)
                currentInterval = Random.Range(minFlickerInterval, maxFlickerInterval);

            yield return new WaitForSeconds(currentInterval);
        }

        isYellowActive = originalState;
        UpdateLightVisuals();

        if (playerInRange && awarenessScript != null)
        {
            float newAwareness = awarenessScript.slider.value + awarenessIncreaseAmount;
            newAwareness = Mathf.Min(newAwareness, awarenessScript.slider.maxValue);
            awarenessScript.SetAwareness(Mathf.RoundToInt(newAwareness));
        }

        isScaring = false;

        // Cooldown period begins after flicker completes
        yield return new WaitForSeconds(cooldownDuration);
        isOnCooldown = false;

        // Show interaction text again after cooldown if player is still in range
        if (playerInRange && interactionText != null)
            interactionText.gameObject.SetActive(true);
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
            // Only show interaction text if not on cooldown
            if (interactionText != null && !isOnCooldown)
                interactionText.gameObject.SetActive(true);
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
    }

    public void TriggerFlickerScare()
    {
        if (!isScaring && !isOnCooldown && playerInRange)
            StartCoroutine(FlickerScare());
    }
}