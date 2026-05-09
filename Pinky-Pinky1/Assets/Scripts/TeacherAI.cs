using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public class TeacherAI : MonoBehaviour
{
    public Transform target;
    public float speed = 150f;
    public float nextWaypointDistance = 3f;

    [Header("Bathroom Patrol Points")]
    public Transform[] patrolPoints;

    [Header("Patrol Behavior")]
    public float timeAtEachPoint = 2f;
    public bool randomPatrolOrder = true;

    [Header("Player Detection")]
    public float detectionRadius = 5f;
    public LayerMask playerLayer;
    public float chaseSpeedMultiplier = 1.5f;
    public float catchDistance = 1.5f;

    private Path path;
    private int currentWaypoint = 0;
    private Seeker seeker;
    private Rigidbody2D rb;
    private float originalSpeed;

    private bool isIdle = false;
    private float idleTimer = 0f;
    private bool isChasing = false;

    private int currentPatrolIndex = 0;
    private List<Transform> activePatrolPoints = new List<Transform>();

    private Transform detectedPlayer = null;
    private bool isTeacherActive = false;

    // Track if we've initialized patrol points
    private bool patrolPointsInitialized = false;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        originalSpeed = speed;

        Debug.Log("TeacherAI Start() called - Component initialized");

        // Initialize patrol points (but don't start moving yet if inactive)
        InitializePatrolPoints();

        // Start coroutines - they'll check isTeacherActive before doing anything
        InvokeRepeating(nameof(UpdatePath), 0f, 0.5f);
        InvokeRepeating(nameof(ScanForPlayer), 0f, 0.3f);

        // Teacher starts inactive
        isTeacherActive = false;
    }

    void InitializePatrolPoints()
    {
        if (patrolPoints != null && patrolPoints.Length > 0 && !patrolPointsInitialized)
        {
            activePatrolPoints.Clear();
            activePatrolPoints.AddRange(patrolPoints);
            if (randomPatrolOrder)
            {
                ShufflePatrolPoints();
            }
            patrolPointsInitialized = true;
            Debug.Log($"Initialized {activePatrolPoints.Count} patrol points");
        }
        else if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning("No patrol points assigned to Teacher!");
        }
    }

    void OnEnable()
    {
        Debug.Log("Teacher GameObject enabled! Starting patrol.");

        // Re-initialize patrol points in case they weren't set in Start
        InitializePatrolPoints();

        isTeacherActive = true;
        isChasing = false;
        isIdle = false;
        detectedPlayer = null;

        // Reset speed to original
        speed = originalSpeed;

        // Reset patrol to first point
        currentPatrolIndex = 0;
        SetNextPatrolTarget();

        // Force an immediate path update
        if (target != null)
        {
            UpdatePath();
        }

        Debug.Log($"Teacher activated - Target set to: {(target != null ? target.name : "NULL")}");
    }

    void OnDisable()
    {
        Debug.Log("Teacher GameObject disabled.");
        isTeacherActive = false;
        isChasing = false;
        isIdle = false;
        detectedPlayer = null;
        path = null; // Clear the path
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        if (!isTeacherActive) return;

        if (isIdle)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0)
            {
                StopIdle();
            }
        }
    }

    void ShufflePatrolPoints()
    {
        for (int i = 0; i < activePatrolPoints.Count; i++)
        {
            Transform temp = activePatrolPoints[i];
            int randomIndex = Random.Range(i, activePatrolPoints.Count);
            activePatrolPoints[i] = activePatrolPoints[randomIndex];
            activePatrolPoints[randomIndex] = temp;
        }
    }

    void SetNextPatrolTarget()
    {
        if (activePatrolPoints.Count == 0)
        {
            Debug.LogWarning("No patrol points assigned!");
            return;
        }

        target = activePatrolPoints[currentPatrolIndex];
        currentPatrolIndex = (currentPatrolIndex + 1) % activePatrolPoints.Count;
        Debug.Log($"Teacher moving to patrol point: {target.name} at position {target.position}");

        // Force path update immediately when target changes
        if (isTeacherActive && !isIdle)
        {
            UpdatePath();
        }
    }

    void ScanForPlayer()
    {
        if (!isTeacherActive) return;
        if (isChasing) return;

        Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, detectionRadius, playerLayer);

        if (players.Length > 0)
        {
            detectedPlayer = players[0].transform;
            EnterChasingState();
        }
    }

    void EnterChasingState()
    {
        if (!isTeacherActive) return;

        isChasing = true;
        isIdle = false;
        speed = originalSpeed * chaseSpeedMultiplier;
        target = detectedPlayer;
        Debug.Log("Teacher saw the player! CHASING!");

        // Force path update for chasing
        UpdatePath();
    }

    void UpdatePath()
    {
        if (!isTeacherActive)
        {
            // Debug.Log("UpdatePath skipped - Teacher not active");
            return;
        }

        if (target == null)
        {
            // Debug.Log("UpdatePath skipped - No target");
            return;
        }

        if (isIdle)
        {
            // Debug.Log("UpdatePath skipped - Teacher is idle");
            return;
        }

        if (seeker == null)
        {
            Debug.LogError("Seeker component is missing!");
            return;
        }

        if (seeker.IsDone())
        {
            // Debug.Log($"Starting path to: {target.name}");
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
            Debug.Log($"Path found! {path.vectorPath.Count} waypoints to destination");
        }
        else
        {
            Debug.LogError($"Path error: {p.error}");
        }
    }

    void FixedUpdate()
    {
        if (!isTeacherActive) return;
        if (isIdle) return;

        if (path == null)
        {
            // Try to get a path if we don't have one
            if (target != null && !isChasing)
            {
                UpdatePath();
            }
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            // Reached destination
            if (!isChasing)
            {
                StartIdle();
            }
            return;
        }

        // Handle chasing state - update target to player position
        if (isChasing && detectedPlayer != null)
        {
            target = detectedPlayer;

            // Check if caught the player
            float distanceToPlayer = Vector2.Distance(rb.position, detectedPlayer.position);
            if (distanceToPlayer <= catchDistance)
            {
                CatchPlayer();
                return;
            }

            // If player escaped too far, stop chasing and return to patrol
            if (distanceToPlayer > detectionRadius * 1.5f)
            {
                StopChasing();
                return;
            }
        }

        // Move towards current waypoint
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;
        rb.AddForce(force);

        // Debug draw line to waypoint
        Debug.DrawLine(rb.position, path.vectorPath[currentWaypoint], Color.red);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
            // Debug.Log($"Reached waypoint {currentWaypoint}/{path.vectorPath.Count}");
        }
    }

    void CatchPlayer()
    {
        Debug.Log("TEACHER CAUGHT THE PLAYER! GAME OVER!");

        if (OnPlayerCaught != null)
        {
            OnPlayerCaught();
        }

        // Disable the teacher GameObject
        gameObject.SetActive(false);
    }

    void StopChasing()
    {
        isChasing = false;
        detectedPlayer = null;
        speed = originalSpeed;
        SetNextPatrolTarget();
        Debug.Log("Lost the player, returning to patrol.");
    }

    void StartIdle()
    {
        isIdle = true;
        idleTimer = timeAtEachPoint;
        rb.linearVelocity = Vector2.zero;
        Debug.Log($"Teacher idling at {target?.name} for {timeAtEachPoint} seconds");
    }

    void StopIdle()
    {
        isIdle = false;
        SetNextPatrolTarget();
    }

    public System.Action OnPlayerCaught;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, catchDistance);

        if (patrolPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform point in patrolPoints)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, 0.4f);
            }
        }
    }
}