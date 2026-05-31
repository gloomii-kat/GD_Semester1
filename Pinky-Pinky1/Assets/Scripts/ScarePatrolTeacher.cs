using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ScarePatrolTeacher : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    public float waitTimeAtPoints = 1f;

    [Header("Scare Response")]
    public float searchDuration = 5f;        // Time to search when 1 child is scared
    public float searchSpeed = 2.5f;

    [Header("Chase Settings (2+ scared children)")]
    public Transform playerTarget;
    public float chaseSpeed = 4f;
    public float chaseRange = 8f;
    public float loseSightRange = 12f;

    [Header("Scare Detection")]
    public float scareDetectionRadius = 10f;
    private int scaredChildrenCount = 0;

    [Header("Chase Delay")]
    public float initialChaseDelay = 2f;
    private float chaseDelayTimer = 0f;
    private bool canChase = false;

    [Header("References")]
    public NightManager nightManager;
    public GameObject gotchaText;
    public string gameOverSceneName = "GameOver";
    public int gameOverSceneIndex = -1;

    private AudioManager audioManager;
    private Rigidbody2D rb;
    private float currentSpeed;

    // Patrol state tracking
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    private float waitTimer;

    // FSM States
    private enum TeacherState
    {
        Patrol,
        Search,
        Chase
    }

    private TeacherState currentState;
    private float searchTimer;
    private Vector3 searchLocation;
    private bool hasCaughtPlayer = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

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
        currentState = TeacherState.Patrol;
        currentSpeed = patrolSpeed;
        hasCaughtPlayer = false;
        scaredChildrenCount = 0;

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            currentPatrolIndex = 0;
        }

        chaseDelayTimer = initialChaseDelay;
        canChase = false;

        Debug.Log("Scare Patrol Teacher activated - NORMAL PATROL");
    }

    void OnDisable()
    {
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
                Debug.Log("Scare Teacher can now chase!");
            }
        }

        CheckForScaredChildren();

        switch (currentState)
        {
            case TeacherState.Patrol:
                UpdatePatrolState();
                break;
            case TeacherState.Search:
                UpdateSearchState();
                break;
            case TeacherState.Chase:
                UpdateChaseState();
                break;
        }
    }

    void CheckForScaredChildren()
    {
        Collider2D[] children = Physics2D.OverlapCircleAll(transform.position, scareDetectionRadius);
        int scaredCount = 0;
        Vector3 scaredPositionSum = Vector3.zero;

        foreach (Collider2D child in children)
        {
            if (child.CompareTag("Child") && child.GetComponent<ChildAI>() != null)
            {
                ChildAI childAI = child.GetComponent<ChildAI>();
                if (childAI.IsScared())
                {
                    scaredCount++;
                    scaredPositionSum += child.transform.position;
                }
            }
        }

        int previousScaredCount = scaredChildrenCount;
        scaredChildrenCount = scaredCount;

        // React to changes in scared children count
        if (canChase)
        {
            if (scaredChildrenCount >= 2 && currentState != TeacherState.Chase)
            {
                // Two or more scared children - CHASE!
                TransitionToChase();
            }
            else if (scaredChildrenCount == 1 && currentState == TeacherState.Patrol)
            {
                // One scared child - SEARCH
                Vector3 averageScaredPosition = scaredPositionSum / scaredCount;
                TransitionToSearch(averageScaredPosition);
            }
            else if (scaredChildrenCount == 0 && currentState == TeacherState.Search)
            {
                // No scared children anymore - return to patrol
                TransitionToPatrol();
            }
        }
    }

    void UpdatePatrolState()
    {
        // Normal patrol behavior
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
                rb.linearVelocity = Vector2.zero;
                return;
            }
        }

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            MoveTowardsTarget(patrolPoints[currentPatrolIndex].position, patrolSpeed);

            float distanceToPoint = Vector2.Distance(transform.position, patrolPoints[currentPatrolIndex].position);
            if (distanceToPoint < 0.3f)
            {
                isWaiting = true;
                waitTimer = waitTimeAtPoints;
                Debug.Log($"Scare Teacher reached patrol point {currentPatrolIndex}");
            }
        }
    }

    void UpdateSearchState()
    {
        searchTimer -= Time.deltaTime;

        // Move to search location
        MoveTowardsTarget(searchLocation, searchSpeed);

        float distanceToSearchPoint = Vector2.Distance(transform.position, searchLocation);
        if (distanceToSearchPoint < 1f)
        {
            rb.linearVelocity = Vector2.zero;
            // Look around at search point
            Debug.Log("Scare Teacher: Searching area...");
        }

        if (searchTimer <= 0f)
        {
            // Search time expired, go back to patrol
            TransitionToPatrol();
        }
    }

    void UpdateChaseState()
    {
        if (IsPlayerInChaseRange())
        {
            MoveTowardsTarget(playerTarget.position, chaseSpeed);
        }
        else if (IsPlayerInLoseRange())
        {
            // Lost player but still in range - keep chasing a bit
            MoveTowardsTarget(playerTarget.position, chaseSpeed);
        }
        else
        {
            // Player completely out of range - return to patrol
            TransitionToPatrol();
        }
    }

    void MoveTowardsTarget(Vector3 target, float speed)
    {
        Vector2 direction = (target - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    void MoveToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    void TransitionToPatrol()
    {
        currentState = TeacherState.Patrol;
        currentSpeed = patrolSpeed;
        FindNearestPatrolPoint();

        Debug.Log("Scare Teacher: Returned to PATROL");

        if (audioManager != null)
        {
            audioManager.RestoreBackgroundMusic();
        }
    }

    void TransitionToSearch(Vector3 position)
    {
        if (currentState == TeacherState.Chase) return; // Don't interrupt chase

        currentState = TeacherState.Search;
        searchLocation = position;
        searchTimer = searchDuration;
        Debug.Log($"Scare Teacher: SEARCHING at {position} for {searchDuration}s (1 child scared)");
    }

    void TransitionToChase()
    {
        if (currentState == TeacherState.Chase) return;
        if (!canChase) return;

        currentState = TeacherState.Chase;
        currentSpeed = chaseSpeed;
        Debug.Log("Scare Teacher: CHASE - Multiple scared children detected!");

        if (audioManager != null)
        {
            audioManager.PlayChaseMusic();
        }
    }

    void FindNearestPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        float closestDist = Mathf.Infinity;
        int closestIndex = 0;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            float dist = Vector2.Distance(transform.position, patrolPoints[i].position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = i;
            }
        }

        currentPatrolIndex = closestIndex;
        isWaiting = false;
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
        rb.linearVelocity = Vector2.zero;
        Debug.Log("SCARE TEACHER CAUGHT PINKY!");

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
        else if (gameOverSceneIndex >= 0 && gameOverSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(gameOverSceneIndex);
        }
        else
        {
            Debug.LogError("Game Over scene not configured!");
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
        if (patrolPoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform point in patrolPoints)
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
