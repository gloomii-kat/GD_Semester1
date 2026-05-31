using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TeacherChase : MonoBehaviour
{
    [Header("Chase Settings")]
    public Transform playerTarget;
    public float chaseSpeed = 4f;

    private RoomGameManager roomGameManager;

    [Header("Chase Detection")]
    public CircleCollider2D detectionCollider;  // Single collider for chase detection (on parent)
    public float detectionRange = 12f;          // Visual reference only

    [Header("Chase Delay")]
    public float initialChaseDelay = 2f;
    private float chaseDelayTimer = 0f;
    private bool canChase = false;

    [Header("End Condition")]
    public PolygonCollider2D catchCollider;  // Assign the polygon trigger collider in inspector
    public GameObject gotchaText;
    public string gameOverSceneName = "GameOver";
    public int gameOverSceneIndex = 3;

    [Header("Hide Settings")]
    [Tooltip("Can the teacher catch the player while hiding?")]
    public bool canCatchWhileHiding = false;

    [Header("References")]
    public TeacherPatrol patrolScript;

    private AudioManager audioManager;
    private Rigidbody2D rb;
    private PlayerHideController playerHideController;

    // Chase state
    public bool IsChasing { get; private set; } = false;
    private bool isSearching = false;
    private float searchTimer = 0f;
    private Vector3 lastKnownPlayerPosition;
    private bool hasCaughtPlayer = false;
    private int scaredChildrenCount = 0;

    // Collider tracking
    private bool isPlayerInDetectionRange = false;

    // Movement
    private Vector2 currentMoveDirection = Vector2.zero;
    private float currentSpeed = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        roomGameManager = Object.FindFirstObjectByType<RoomGameManager>();

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 0f;
        }

        // Find patrol script if not assigned
        if (patrolScript == null)
            patrolScript = GetComponent<TeacherPatrol>();

        GameObject audioObject = GameObject.FindGameObjectWithTag("Audio");
        if (audioObject != null)
        {
            audioManager = audioObject.GetComponent<AudioManager>();
        }

        if (gotchaText != null)
            gotchaText.SetActive(false);

        // Setup detection collider on parent
        SetupDetectionCollider();

        // Find player's hide controller
        if (playerTarget != null)
        {
            playerHideController = playerTarget.GetComponent<PlayerHideController>();
        }

        // Ensure catch collider is set up correctly
        if (catchCollider != null)
        {
            catchCollider.isTrigger = true;
            Debug.Log($"Catch collider assigned: {catchCollider.gameObject.name}");
        }
        else
        {
            Debug.LogError("Catch collider not assigned to TeacherChase! Please assign the PolygonCollider2D that should catch the player.");
        }
    }

    void SetupDetectionCollider()
    {
        // Create detection collider if not assigned
        if (detectionCollider == null)
        {
            detectionCollider = gameObject.AddComponent<CircleCollider2D>();
            detectionCollider.isTrigger = true;
            detectionCollider.radius = detectionRange;
            Debug.Log("Created Detection Collider automatically on parent");
        }
        else
        {
            detectionCollider.isTrigger = true;
            detectionCollider.radius = detectionRange;
        }
    }

    void OnEnable()
    {
        IsChasing = false;
        isSearching = false;
        hasCaughtPlayer = false;
        scaredChildrenCount = 0;
        isPlayerInDetectionRange = false;

        chaseDelayTimer = initialChaseDelay;
        canChase = false;
    }

    void OnDisable()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        // Update chase delay timer
        if (!canChase)
        {
            chaseDelayTimer -= Time.deltaTime;
            if (chaseDelayTimer <= 0f)
            {
                canChase = true;
                Debug.Log("Teacher can now chase!");
            }
        }

        // Check for scared children
        CheckScaredChildrenCount();

        // Handle chase logic
        if (canChase && !hasCaughtPlayer)
        {
            if (!IsChasing && !isSearching)
            {
                // Check if should start chasing (2+ scared children AND player in detection range)
                if (scaredChildrenCount >= 2 && isPlayerInDetectionRange)
                {
                    StartChase();
                }
            }
            else if (IsChasing)
            {
                UpdateChase();
            }
            else if (isSearching)
            {
                UpdateSearch();
            }
        }

        // Apply chase movement
        if ((IsChasing || isSearching) && rb != null)
        {
            rb.linearVelocity = currentMoveDirection * currentSpeed;
        }
    }

    void CheckScaredChildrenCount()
    {
        if (roomGameManager != null)
            scaredChildrenCount = roomGameManager.GetTotalScaredChildren();
    }

    void StartChase()
    {
        IsChasing = true;
        isSearching = false;

        // Stop patrol movement
        if (patrolScript != null)
            patrolScript.StopPatrolMovement();

        Debug.Log($"CHASE STARTED - {scaredChildrenCount} scared children detected!");

        if (audioManager != null)
        {
            audioManager.PlayChaseMusic();
        }
    }

    void UpdateChase()
    {
        if (playerTarget == null) return;

        if (isPlayerInDetectionRange)
        {
            // Player is in range - chase them
            lastKnownPlayerPosition = playerTarget.position;
            Vector2 direction = (playerTarget.position - transform.position).normalized;
            currentMoveDirection = direction;
            currentSpeed = chaseSpeed;
        }
        else
        {
            // Player left the detection range - start searching
            TransitionToSearch();
        }
    }

    void TransitionToSearch()
    {
        IsChasing = false;
        isSearching = true;
        searchTimer = 3f;
        lastKnownPlayerPosition = playerTarget != null ? playerTarget.position : transform.position;
        Debug.Log("SEARCHING - Lost Pinky");
    }

    void UpdateSearch()
    {
        searchTimer -= Time.deltaTime;

        // Move to last known player position
        Vector2 direction = (lastKnownPlayerPosition - transform.position).normalized;
        currentMoveDirection = direction;
        currentSpeed = chaseSpeed * 0.7f;

        float distance = Vector2.Distance(transform.position, lastKnownPlayerPosition);
        if (distance < 1f)
        {
            currentMoveDirection = Vector2.zero;
            currentSpeed = 0f;
        }

        if (searchTimer <= 0f)
        {
            StopChase();
        }

        // If player is spotted again during search, resume chase
        if (isPlayerInDetectionRange)
        {
            StartChase();
        }
    }

    void StopChase()
    {
        IsChasing = false;
        isSearching = false;
        currentMoveDirection = Vector2.zero;
        currentSpeed = 0f;

        // Resume patrol
        if (patrolScript != null)
            patrolScript.ResumePatrol();

        Debug.Log("CHASE ENDED - Returning to patrol");

        if (audioManager != null)
        {
            audioManager.RestoreBackgroundMusic();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Detection range — 'other' is the player's collider entering OUR trigger zone
        isPlayerInDetectionRange = true;
        Debug.Log("Player entered DETECTION range");

        // Also immediately check catch collider overlap
        CheckCatchCollider(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInDetectionRange = false;
        Debug.Log("Player left detection range");
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || hasCaughtPlayer) return;

        // Check catch during any active chase state (chasing OR searching)
        if (IsChasing || isSearching)
        {
            CheckCatchCollider(other);
        }
    }

    // Extracted helper — reused by Enter and Stay
    void CheckCatchCollider(Collider2D playerCollider)
    {
        if (catchCollider == null || hasCaughtPlayer) return;
        if (!catchCollider.IsTouching(playerCollider)) return;

        bool isPlayerHiding = playerHideController != null && playerHideController.IsHiding();

        if (!isPlayerHiding || canCatchWhileHiding)
        {
            Debug.Log(isPlayerHiding ? "Teacher caught Pinky while hiding!" : "Player touched CATCH collider!");
            CatchPlayer();
        }
        else
        {
            Debug.Log("Teacher tried to catch Pinky, but Pinky is hiding!");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Backup catch method using collision
        if (collision.gameObject.CompareTag("Player") && !hasCaughtPlayer && IsChasing)
        {
            // Check if the collision involved our catch collider
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.collider == catchCollider)
                {
                    Debug.Log("PLAYER collided with catch collider!");

                    bool isPlayerHiding = false;
                    if (playerHideController != null)
                    {
                        isPlayerHiding = playerHideController.IsHiding();
                    }

                    if (!isPlayerHiding || canCatchWhileHiding)
                    {
                        CatchPlayer();
                    }
                    break;
                }
            }
        }
    }

    void CatchPlayer()
    {
        hasCaughtPlayer = true;
        IsChasing = false;
        isSearching = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        Debug.Log("TEACHER CAUGHT PINKY!");

        if (gotchaText != null)
        {
            gotchaText.SetActive(true);
            StartCoroutine(HideGotchaTextAfterDelay(2f));
        }

        if (audioManager != null)
        {
            audioManager.PlaySFX(audioManager.DoorBanging);
        }

        LoadGameOverScene();
        StartCoroutine(DisableAfterDelay(2f));
    }

    void LoadGameOverScene()
    {
        if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
        else if (gameOverSceneIndex >= 0)
        {
            SceneManager.LoadScene(gameOverSceneIndex);
        }
    }

    IEnumerator HideGotchaTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (gotchaText != null)
            gotchaText.SetActive(false);
    }

    IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    // Public method to set detection range
    public void SetDetectionRange(float newRange)
    {
        detectionRange = newRange;
        if (detectionCollider != null)
            detectionCollider.radius = detectionRange;
    }

    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw catch collider if assigned
        if (catchCollider != null)
        {
            Gizmos.color = Color.magenta;
            Vector2[] points = catchCollider.points;
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 currentPoint = catchCollider.transform.TransformPoint(points[i]);
                Vector2 nextPoint = catchCollider.transform.TransformPoint(points[(i + 1) % points.Length]);
                Gizmos.DrawLine(currentPoint, nextPoint);
            }
        }
    }
}
