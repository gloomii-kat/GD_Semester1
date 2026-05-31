using UnityEngine;

/// <summary>
/// Attach to each door/exit GameObject.
/// Assign the matching index (0 = first room's door, 1 = second, etc.)
/// </summary>
public class DoorBlocker : MonoBehaviour
{
    [Tooltip("Match this to the room index this door exits FROM")]
    public int doorIndex = 0;

    [Header("References")]
    public Collider2D blockingCollider;   // The collider that stops the player
    private RoomGameManager gameManager;

    public bool IsLocked { get; private set; } = true;

    void Awake()
    {
        gameManager = FindObjectOfType<RoomGameManager>();

        if (blockingCollider == null)
            blockingCollider = GetComponent<Collider2D>();
    }

    public void Lock()
    {
        IsLocked = true;
        if (blockingCollider != null)
        {
            blockingCollider.isTrigger = false; // Solid — physically blocks player
        }
        Debug.Log($"Door {doorIndex} LOCKED");
    }

    public void Unlock()
    {
        IsLocked = false;
        if (blockingCollider != null)
        {
            blockingCollider.isTrigger = true; // Passthrough — player can walk through
        }
        Debug.Log($"Door {doorIndex} UNLOCKED");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Only fires when unlocked (isTrigger = true)
        if (other.CompareTag("Player") && gameManager != null)
        {
            gameManager.OnPlayerPassedThroughDoor(doorIndex);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Only fires when locked (isTrigger = false), player walks into blocked door
        if (collision.gameObject.CompareTag("Player") && gameManager != null)
        {
            gameManager.OnPlayerTriedLockedDoor();
        }
    }
}
