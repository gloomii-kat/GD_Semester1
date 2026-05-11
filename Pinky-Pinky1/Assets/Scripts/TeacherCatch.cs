using UnityEngine;

public class TeacherCatch : MonoBehaviour
{
    [Header("References")]
    public NightManager nightManager;
    public PlayerVisibilityController playerVisibility;

    [Tooltip("Tag on your Player GameObject")]
    public string playerTag = "Player";

    private bool hasTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (other.CompareTag(playerTag))
        {
            // Only catch player if they are visible (fully revealed)
            if (playerVisibility != null && playerVisibility.IsVisible())
            {
                hasTriggered = true;
                Debug.Log("Teacher caught the player!");

                if (nightManager != null)
                    nightManager.OnGameOver();
                else
                    Debug.LogError("NightManager not assigned in TeacherCatch!");
            }
        }
    }
}
