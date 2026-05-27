using UnityEngine;

public class PlayerHideController : MonoBehaviour
{
    private SpriteRenderer playerRenderer;
    private PlayerMovement playerMovement;
    private int originalSortingOrder;

    private bool isHiding = false;
    private HideSpot currentHideSpot = null;

    [Header("Hide Settings")]
    public KeyCode hideKey = KeyCode.F;

   

    void Start()
    {
        playerRenderer = GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMovement>();
        originalSortingOrder = playerRenderer.sortingOrder;
    }

    void Update()
    {
        // Check for F key press
        if (currentHideSpot != null && Input.GetKeyDown(hideKey))
        {
            if (isHiding)
            {
                // Option 1: Player chooses to leave early by pressing F again
                Unhide();
            }
            else
            {
                // Option 2: Player chooses to hide by pressing F
                Hide();
            }
        }

       
    }

    private void Hide()
    {
        if (currentHideSpot == null) return;

        isHiding = true;
        currentHideSpot.isActive = true;

        // Start the timer (will kick player out if time expires)
        currentHideSpot.StartHideTimer();

        // Change player's sorting order to be behind object
        playerRenderer.sortingOrder = currentHideSpot.hideSortingOrder;

        // Disable movement
        if (playerMovement != null)
            playerMovement.canMove = false;

        // Make player semi-transparent
        Color color = playerRenderer.color;
        color.a = 0.5f;
        playerRenderer.color = color;

        Debug.Log($"Hiding in: {currentHideSpot.gameObject.name}. Press F again to leave early.");

        if (currentHideSpot.maxHideTime > 0 && currentHideSpot.kickPlayerOut)
        {
            Debug.Log($"You can hide for {currentHideSpot.maxHideTime} seconds before being discovered!");
        }
    }

    public void Unhide()
    {
        if (!isHiding) return;

        isHiding = false;

        if (currentHideSpot != null)
        {
            currentHideSpot.isActive = false;
            currentHideSpot.StopTimer(); // Stop the timer since player is leaving
            currentHideSpot.ResetTimer();
        }

        // Restore original settings
        playerRenderer.sortingOrder = originalSortingOrder;

        if (playerMovement != null)
            playerMovement.canMove = true;

        Color color = playerRenderer.color;
        color.a = 1f;
        playerRenderer.color = color;

       

        Debug.Log("Left hiding spot early by pressing F");
    }

    public void ForceUnhide(string reason)
    {
        if (!isHiding) return;

        isHiding = false;

        if (currentHideSpot != null)
        {
            currentHideSpot.isActive = false;
            currentHideSpot.ResetTimer();
        }

        playerRenderer.sortingOrder = originalSortingOrder;

        if (playerMovement != null)
            playerMovement.canMove = true;

        Color color = playerRenderer.color;
        color.a = 1f;
        playerRenderer.color = color;

        Debug.Log($"Forced out of hiding spot: {reason}");
    }


    public void SetCurrentHideSpot(HideSpot spot)
    {
        if (isHiding && currentHideSpot != null && currentHideSpot != spot)
        {
            ForceUnhide("Moved while hiding!");
        }

        currentHideSpot = spot;
    }

    public void ClearCurrentHideSpot()
    {
        if (!isHiding)
            currentHideSpot = null;
    }

    public HideSpot GetCurrentHideSpot()
    {
        return currentHideSpot;
    }

    public bool IsHiding()
    {
        return isHiding;
    }
}