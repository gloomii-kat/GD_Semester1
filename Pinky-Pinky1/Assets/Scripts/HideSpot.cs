using UnityEngine;

public class HideSpot : MonoBehaviour
{
    [Header("Hide Settings")]
    public int hideSortingOrder = -1;
    public bool autoUnhideOnExit = true;

    [Header("Timer Settings")]
    public float maxHideTime = 5f; // Maximum time player can hide here (set to 0 for no limit)
    public bool kickPlayerOut = true; // Should player be kicked out after time?
    public bool showTimerWarning = true;

    [Header("Visual Feedback")]
    public Color highlightColor = Color.yellow;
    public Color warningColor = Color.red;
    private Color originalColor;
    private SpriteRenderer spotRenderer;

    private PlayerHideController playerHideController;
    [HideInInspector] public bool isActive = false;

    // Timer variables
    private float currentHideTime = 0f;
    private bool timerActive = false;

    void Start()
    {
        spotRenderer = GetComponent<SpriteRenderer>();
        if (spotRenderer != null)
            originalColor = spotRenderer.color;
    }

    void Update()
    {
        // Handle timer when player is hiding
        if (timerActive && isActive && kickPlayerOut && maxHideTime > 0)
        {
            currentHideTime -= Time.deltaTime;

            // Timer warning
            if (showTimerWarning && currentHideTime <= 3f && currentHideTime > 0)
            {
                if (spotRenderer != null)
                    spotRenderer.color = warningColor;
            }

            // Kick player out when timer reaches zero
            if (currentHideTime <= 0f && playerHideController != null)
            {
                playerHideController.ForceUnhide("Time's up! You've been discovered!");
                ResetTimer();
            }
        }
    }

    public void StartHideTimer()
    {
        if (kickPlayerOut && maxHideTime > 0)
        {
            currentHideTime = maxHideTime;
            timerActive = true;
        }
    }

    public void StopTimer()
    {
        timerActive = false;
    }

    public void ResetTimer()
    {
        timerActive = false;
        currentHideTime = 0f;

        if (spotRenderer != null && !isActive)
            spotRenderer.color = originalColor;
    }

    public float GetRemainingTime()
    {
        return currentHideTime;
    }

    public float GetMaxTime()
    {
        return maxHideTime;
    }

    public bool HasTimerWarning()
    {
        return showTimerWarning && currentHideTime <= 3f && currentHideTime > 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerHideController == null)
                playerHideController = other.GetComponent<PlayerHideController>();

            if (playerHideController != null)
            {
                playerHideController.SetCurrentHideSpot(this);

                if (spotRenderer != null && !isActive)
                    spotRenderer.color = highlightColor;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerHideController != null)
            {
                // Only auto-unhide if the player is hiding in THIS spot
                if (isActive && autoUnhideOnExit)
                {
                    playerHideController.Unhide();
                    ResetTimer();
                }

                if (playerHideController.GetCurrentHideSpot() == this)
                    playerHideController.ClearCurrentHideSpot();
            }

            if (spotRenderer != null && !isActive)
                spotRenderer.color = originalColor;
        }
    }
}