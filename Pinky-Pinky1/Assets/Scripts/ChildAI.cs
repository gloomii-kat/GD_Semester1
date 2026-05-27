using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public class ChildAI : MonoBehaviour
{
    // Inspector

    [Header("Movement")]
    public float wanderSpeed = 150f;
    public float scaredSpeed = 300f;
    public float nextWaypointDistance = 3f;

    [Header("Wander Points")]
    public Transform[] wanderPoints;          // Assign room-specific points in Inspector

    [Header("Idle")]
    public float idleTimeAtPoints = 2f;

    [Header("Scare")]
    public Transform exitPoint;               // Where child runs when scared
    public float scareCooldown = 2f;          // Prevent multiple scares rapidly
    private bool isScareReady = true;
    private bool hasBeenScared = false;       // NEW: Track if child was already scared

    [Header("Lure")]
    public float lureDuration = 3f;           // How long child is attracted to lure object

    // State
    private enum ChildState { Wander, Lured, Scared }
    private ChildState state = ChildState.Wander;

    private Path path;
    private int currentWaypoint = 0;
    private Seeker seeker;
    private Rigidbody2D rb;

    private bool isIdle = false;
    private float idleTimer = 0f;
    private Transform currentTarget;

    // Events — RoomManager listens to these
    public static event System.Action OnChildScared;   // Fires when this child gets scared

    // Unity
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        SetNextWanderTarget();
        InvokeRepeating(nameof(UpdatePath), 0f, 0.5f);

        hasBeenScared = false;  // Initialize
    }

    void Update()
    {
        switch (state)
        {
            case ChildState.Wander:
                HandleIdle();
                break;

            case ChildState.Lured:
                // Lured state - just follows target
                break;

            case ChildState.Scared:
                CheckExitReached();
                break;
        }
    }

    void FixedUpdate()
    {
        if (isIdle || path == null || currentTarget == null) return;
        if (currentWaypoint >= path.vectorPath.Count) return;

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        rb.AddForce(direction * GetCurrentSpeed() * Time.deltaTime);

        if (Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]) < nextWaypointDistance)
            currentWaypoint++;

        if (currentWaypoint >= path.vectorPath.Count && state == ChildState.Wander)
            StartIdle();
    }

    // Pathfinding
    void UpdatePath()
    {
        if (isIdle || currentTarget == null) return;
        if (seeker.IsDone())
            seeker.StartPath(rb.position, currentTarget.position, OnPathComplete);
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    // Wander
    void SetNextWanderTarget()
    {
        if (hasBeenScared) return;  // NEW: Don't set new wander targets if already scared
        if (wanderPoints == null || wanderPoints.Length == 0) return;
        currentTarget = wanderPoints[Random.Range(0, wanderPoints.Length)];
    }

    void HandleIdle()
    {
        if (!isIdle) return;
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0f)
        {
            isIdle = false;
            SetNextWanderTarget();
        }
    }

    void StartIdle()
    {
        if (hasBeenScared) return;  // NEW: Don't idle if already scared
        isIdle = true;
        idleTimer = idleTimeAtPoints;
        rb.linearVelocity = Vector2.zero;
    }

    // Scare (called externally by scare objects like toilet, light, basin)
    public void Scare()
    {
        if (hasBeenScared)          // NEW: Already scared, ignore
        {
            Debug.Log($"{gameObject.name} already scared, ignoring new scare");
            return;
        }

        if (state == ChildState.Scared) return;     // Already scared, ignore
        if (!isScareReady) return;                  // On cooldown

        StartCoroutine(ScareRoutine());
    }

    IEnumerator ScareRoutine()
    {
        hasBeenScared = true;                       // NEW: Mark as permanently scared
        isScareReady = false;
        state = ChildState.Scared;
        isIdle = false;
        rb.linearVelocity = Vector2.zero;

        if (exitPoint != null)
        {
            currentTarget = exitPoint;
            Debug.Log($"{gameObject.name} was scared! Running to exit NOW!");
        }
        else
        {
            Debug.LogError("Exit point not assigned! Child can't escape!");
        }

        OnChildScared?.Invoke();                    // Tell RoomManager a child was scared

        // Wait for cooldown (optional, child is already scared permanently)
        yield return new WaitForSeconds(scareCooldown);
        isScareReady = true;
    }

    void CheckExitReached()
    {
        if (exitPoint == null) return;
        if (Vector2.Distance(rb.position, exitPoint.position) < 0.5f)
        {
            rb.linearVelocity = Vector2.zero;
            gameObject.SetActive(false);
            Debug.Log($"{gameObject.name} escaped the bathroom! She's gone!");
        }
    }

    // Lure (called externally by thrown object)
    public void Lure(Transform lureTarget)
    {
        if (hasBeenScared) return;                 // NEW: Can't lure a scared child
        if (state == ChildState.Scared) return;     // Can't lure a scared child
        StartCoroutine(LureRoutine(lureTarget));
    }

    IEnumerator LureRoutine(Transform lureTarget)
    {
        state = ChildState.Lured;
        isIdle = false;
        currentTarget = lureTarget;

        yield return new WaitForSeconds(lureDuration);

        if (state == ChildState.Lured && !hasBeenScared)  // Only return to wander if not scared
        {
            state = ChildState.Wander;
            SetNextWanderTarget();
        }
    }

    // Backwards compatibility
    public void TriggerEscape(float speedMultiplier)
    {
        Scare();
    }

    // Helpers
    float GetCurrentSpeed()
    {
        return state == ChildState.Scared ? scaredSpeed : wanderSpeed;
    }

    // NEW: Public property to check if child is scared
    public bool IsScared()
    {
        return hasBeenScared;
    }

    void OnDrawGizmosSelected()
    {
        // Draw exit point connection
        if (exitPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, exitPoint.position);
            Gizmos.DrawWireSphere(exitPoint.position, 0.5f);
        }

        // Draw wander points
        if (wanderPoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform point in wanderPoints)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, 0.3f);
            }
        }
    }
}