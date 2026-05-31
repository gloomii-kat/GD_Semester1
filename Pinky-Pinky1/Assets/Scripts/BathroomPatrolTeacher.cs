using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BathroomPatrolTeacher : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] hallwayPatrolPoints;    // Points in the hallway
    public Transform bathroomPoint;             // Point inside bathroom
    public Transform[] bathroomPatrolPoints;   // Points inside bathroom to patrol
    public float patrolSpeed = 2f;
    public float waitTimeAtPoints = 1f;

    [Header("Bathroom Inspection")]
    public float timeBetweenBathroomChecks = 7f;
    public float bathroomInspectionTime = 7f;
    private float bathroomCheckTimer = 0f;
    private bool isInspectingBathroom = false;
    private float inspectionTimer = 0f;

    [Header("Chase Settings")]
    public Transform playerTarget;
    public float chaseSpeed = 4f;
    public float chaseRange = 4f;
    public float loseSightRange = 12f;

    [Header("Chase Delay")]
    public float initialChaseDelay = 2f;
    private float chaseDelayTimer = 0f;
    private bool canChase = false;

    [Header("Scare Detection")]
    public float scareDetectionRadius = 10f;
    public string childTag = "LittleGirl";
    private int scaredChildrenCount = 0;

    [Header("References")]
    public NightManager nightManager;
    public GameObject gotchaText;
    public string gameOverSceneName = "GameOver";
    public int gameOverSceneIndex = -1;

    private AudioManager audioManager;
    private Rigidbody2D rb;

    // Patrol state tracking
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    // FSM States
    private enum TeacherState
    {
        PatrolHallway,
        MovingToBathroom,
        InspectingBathroom,
        Chase,
        Search
    }

    private TeacherState currentState;
    private float searchTimer;
    private Vector3 lastKnownPlayerPosition;
    private bool hasCaughtPlayer = false;

    // Bathroom patrol tracking
    private int currentBathroomPatrolIndex = 0;
    private bool isWaitingInBathroom = false;
    private float bathroomWaitTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 0f;
        }

        GameObject audioObject = GameObject.FindGameObjectWithTag("Audio");
        if (audioObject != null)
        {
            audioManager = audioObject.GetComponent<AudioManager>();
        }

        if (gotchaText != null)
            gotchaText.SetActive(false);
    }

    void OnEnable()
    {
        currentState = TeacherState.PatrolHallway;
        hasCaughtPlayer = false;
        isInspectingBathroom = false;
        scaredChildrenCount = 0;
        isWaiting = false;
        waitTimer = 0f;
        currentPatrolIndex = 0;

        bathroomCheckTimer = Random.Range(timeBetweenBathroomChecks * 0.8f, timeBetweenBathroomChecks * 1.2f);
        chaseDelayTimer = initialChaseDelay;
        canChase = false;

        Debug.Log("Bathroom Patrol Teacher activated");
    }

    void OnDisable()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        if (!canChase)
        {
            chaseDelayTimer -= Time.deltaTime;
            if (chaseDelayTimer <= 0f)
            {
                canChase = true;
            }
        }

        CheckForScaredChildren();

        switch (currentState)
        {
            case TeacherState.PatrolHallway:
                UpdatePatrolHallwayState();
                break;
            case TeacherState.MovingToBathroom:
                UpdateMovingToBathroomState();
                break;
            case TeacherState.InspectingBathroom:
                UpdateInspectingBathroomState();
                break;
            case TeacherState.Chase:
                UpdateChaseState();
                break;
            case TeacherState.Search:
                UpdateSearchState();
                break;
        }
    }

    void CheckForScaredChildren()
    {
        if (scareDetectionRadius <= 0) return;

        GameObject[] littleGirls = GameObject.FindGameObjectsWithTag(childTag);
        int scaredCount = 0;

        foreach (GameObject girl in littleGirls)
        {
            float distance = Vector2.Distance(transform.position, girl.transform.position);
            if (distance <= scareDetectionRadius)
            {
                ChildAI childAI = girl.GetComponent<ChildAI>();
                if (childAI != null && childAI.IsScared())
                {
                    scaredCount++;
                }
            }
        }

        scaredChildrenCount = scaredCount;
    }

    void UpdatePatrolHallwayState()
    {
        // Check for chase (2 scared children)
        if (canChase && scaredChildrenCount >= 2 && IsPlayerInChaseRange())
        {
            TransitionToChase();
            return;
        }

        // Check if it's time to inspect bathroom
        if (!isInspectingBathroom)
        {
            bathroomCheckTimer -= Time.deltaTime;
            if (bathroomCheckTimer <= 0f || (scaredChildrenCount >= 1))
            {
                if (bathroomPoint != null)
                {
                    TransitionToMovingToBathroom();
                }
                return;
            }
        }

        // Handle waiting at patrol points
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                MoveToNextPatrolPoint();
            }
            else
            {
                if (rb != null)
                    rb.linearVelocity = Vector2.zero;
                return;
            }
        }

        // Move to current patrol point
        if (hallwayPatrolPoints != null && hallwayPatrolPoints.Length > 0)
        {
            Transform targetPoint = hallwayPatrolPoints[currentPatrolIndex];
            if (targetPoint != null)
            {
                // Simple movement towards target
                Vector2 direction = (targetPoint.position - transform.position).normalized;
                rb.linearVelocity = direction * patrolSpeed;

                // Check if reached target
                float distance = Vector2.Distance(transform.position, targetPoint.position);
                if (distance < 0.3f)
                {
                    isWaiting = true;
                    waitTimer = waitTimeAtPoints;
                    rb.linearVelocity = Vector2.zero;
                    Debug.Log($"Reached hallway point {currentPatrolIndex}");
                }
            }
        }
    }

    void UpdateMovingToBathroomState()
    {
        if (bathroomPoint == null)
        {
            TransitionToPatrolHallway();
            return;
        }

        // Move towards bathroom
        Vector2 direction = (bathroomPoint.position - transform.position).normalized;
        rb.linearVelocity = direction * (patrolSpeed * 1.2f);

        // Check if reached bathroom
        float distance = Vector2.Distance(transform.position, bathroomPoint.position);
        if (distance < 0.5f)
        {
            rb.linearVelocity = Vector2.zero;
            TransitionToInspectingBathroom();
        }
    }

    void UpdateInspectingBathroomState()
    {
        inspectionTimer -= Time.deltaTime;

        // Patrol bathroom points if available
        if (bathroomPatrolPoints != null && bathroomPatrolPoints.Length > 0 && inspectionTimer > 0f)
        {
            if (!isWaitingInBathroom)
            {
                Transform targetPoint = bathroomPatrolPoints[currentBathroomPatrolIndex];
                if (targetPoint != null)
                {
                    Vector2 direction = (targetPoint.position - transform.position).normalized;
                    rb.linearVelocity = direction * patrolSpeed;

                    // Check if reached bathroom patrol point
                    float distance = Vector2.Distance(transform.position, targetPoint.position);
                    if (distance < 0.3f)
                    {
                        isWaitingInBathroom = true;
                        bathroomWaitTimer = 0.5f;
                        rb.linearVelocity = Vector2.zero;
                    }
                }
            }
            else
            {
                bathroomWaitTimer -= Time.deltaTime;
                if (bathroomWaitTimer <= 0f)
                {
                    isWaitingInBathroom = false;
                    currentBathroomPatrolIndex = (currentBathroomPatrolIndex + 1) % bathroomPatrolPoints.Length;
                }
            }
        }
        else
        {
            // No bathroom patrol points, just stand still
            rb.linearVelocity = Vector2.zero;
        }

        // Check if inspection is done
        if (inspectionTimer <= 0f)
        {
            TransitionToPatrolHallway();
            bathroomCheckTimer = Random.Range(timeBetweenBathroomChecks * 0.8f, timeBetweenBathroomChecks * 1.2f);
        }

        // During inspection, if player is spotted, chase!
        if (canChase && IsPlayerInChaseRange())
        {
            TransitionToChase();
        }
    }

    void UpdateChaseState()
    {
        if (playerTarget == null) return;

        if (IsPlayerInChaseRange())
        {
            lastKnownPlayerPosition = playerTarget.position;

            // Chase the player
            Vector2 direction = (playerTarget.position - transform.position).normalized;
            rb.linearVelocity = direction * chaseSpeed;
        }
        else if (IsPlayerInLoseRange())
        {
            // Still in lose range, keep chasing
            Vector2 direction = (playerTarget.position - transform.position).normalized;
            rb.linearVelocity = direction * chaseSpeed;
        }
        else
        {
            // Player completely out of range
            TransitionToSearch();
        }
    }

    void UpdateSearchState()
    {
        searchTimer -= Time.deltaTime;

        // Move to last known player position
        Vector2 direction = (lastKnownPlayerPosition - transform.position).normalized;
        rb.linearVelocity = direction * (chaseSpeed * 0.7f);

        float distance = Vector2.Distance(transform.position, lastKnownPlayerPosition);
        if (distance < 1f)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (searchTimer <= 0f)
        {
            TransitionToPatrolHallway();
        }

        if (canChase && IsPlayerInChaseRange())
        {
            TransitionToChase();
        }
    }

    void MoveToNextPatrolPoint()
    {
        if (hallwayPatrolPoints == null || hallwayPatrolPoints.Length == 0) return;
        currentPatrolIndex = (currentPatrolIndex + 1) % hallwayPatrolPoints.Length;
        Debug.Log($"Moving to hallway point {currentPatrolIndex}");
    }

    void TransitionToPatrolHallway()
    {
        currentState = TeacherState.PatrolHallway;
        isInspectingBathroom = false;
        isWaiting = false;
        isWaitingInBathroom = false;

        // Find nearest patrol point to continue from
        FindNearestHallwayPoint();

        Debug.Log("Returned to HALLWAY PATROL");

        if (audioManager != null)
        {
            audioManager.RestoreBackgroundMusic();
        }
    }

    void TransitionToMovingToBathroom()
    {
        currentState = TeacherState.MovingToBathroom;
        Debug.Log("Moving to BATHROOM");
    }

    void TransitionToInspectingBathroom()
    {
        currentState = TeacherState.InspectingBathroom;
        isInspectingBathroom = true;
        inspectionTimer = bathroomInspectionTime;
        currentBathroomPatrolIndex = 0;
        isWaitingInBathroom = false;
        Debug.Log($"INSPECTING BATHROOM for {bathroomInspectionTime}s");
    }

    void TransitionToChase()
    {
        if (currentState == TeacherState.Chase) return;
        if (!canChase) return;
        if (playerTarget == null) return;

        currentState = TeacherState.Chase;
        Debug.Log("CHASE - Spotted Pinky!");

        if (audioManager != null)
        {
            audioManager.PlayChaseMusic();
        }
    }

    void TransitionToSearch()
    {
        currentState = TeacherState.Search;
        searchTimer = 3f;
        lastKnownPlayerPosition = playerTarget != null ? playerTarget.position : transform.position;
        Debug.Log("SEARCH - Lost Pinky");
    }

    void FindNearestHallwayPoint()
    {
        if (hallwayPatrolPoints == null || hallwayPatrolPoints.Length == 0) return;

        float closestDist = Mathf.Infinity;
        int closestIndex = 0;

        for (int i = 0; i < hallwayPatrolPoints.Length; i++)
        {
            if (hallwayPatrolPoints[i] == null) continue;

            float dist = Vector2.Distance(transform.position, hallwayPatrolPoints[i].position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = i;
            }
        }

        currentPatrolIndex = closestIndex;
        isWaiting = false;
        Debug.Log($"Resuming patrol from nearest point {currentPatrolIndex}");
    }

    bool IsPlayerInChaseRange()
    {
        if (playerTarget == null) return false;
        return Vector2.Distance(transform.position, playerTarget.position) < chaseRange;
    }

    bool IsPlayerInLoseRange()
    {
        if (playerTarget == null) return false;
        return Vector2.Distance(transform.position, playerTarget.position) < loseSightRange;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasCaughtPlayer) return;

        if (other.CompareTag("Player"))
        {
            CatchPlayer();
        }
    }

    void CatchPlayer()
    {
        hasCaughtPlayer = true;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        Debug.Log("BATHROOM TEACHER CAUGHT PINKY!");

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

    void OnDrawGizmosSelected()
    {
        if (hallwayPatrolPoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform point in hallwayPatrolPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.3f);
                    Gizmos.DrawLine(transform.position, point.position);
                }
            }
        }

        if (bathroomPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(bathroomPoint.position, 0.5f);
        }

        if (bathroomPatrolPoints != null)
        {
            Gizmos.color = Color.magenta;
            foreach (Transform point in bathroomPatrolPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.3f);
                }
            }
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, loseSightRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, scareDetectionRadius);
    }
}