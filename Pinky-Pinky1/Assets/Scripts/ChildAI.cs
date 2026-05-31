using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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

    [Header("References")]
    public Transform pinky;

    [Header("Separation")]
    public float separationRadius = 1.5f;      // How close other children can get
    public float separationForce = 15f;         // Force to push away (REDUCED from default)


    // Add to ChildAI.cs - near the top with other variables
    private bool isInitialized = false;

    // State
    private enum ChildState { Wander, Lured, Scared }
    private ChildState state = ChildState.Wander;

    private Path path;
    private int currentWaypoint = 0;
    private Seeker seeker;
    private Rigidbody2D rb;

    private bool isIdle = false;
    private float idleTimer = 0f;
    public Transform currentTarget;

    // Events — RoomManager listens to these
    public static event System.Action OnChildScared;   // Fires when this child gets scared

    // Unity
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        


        // TRY TO FIND ROOM DATA AUTOMATICALLY
        if (!isInitialized)
        {
            RoomData roomData = GetComponentInParent<RoomData>();
            if (roomData != null)
            {
                // RoomData will initialize us, but we need to wait for it
                // So just log and let RoomData call SetRoomReferences
                Debug.Log($"{gameObject.name} waiting for RoomData initialization...");
            }
            else
            {
                // Fallback: try to find references manually
                FindRoomReferences();
            }
        }

        SetNextWanderTarget();
        InvokeRepeating(nameof(UpdatePath), 0f, 0.5f);

        hasBeenScared = false;  // Initialize

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        if (!CompareTag("LittleGirl"))
        {
            tag = "LittleGirl";
            Debug.Log($"Set tag for {gameObject.name} to LittleGirl");
        }

        // EXISTING trigger collider (for detection)
        CircleCollider2D triggerCol = GetComponent<CircleCollider2D>();
        if (triggerCol != null)
        {
            triggerCol.isTrigger = true;  // Keep this as trigger
        }

        // ADD NEW: Physics collider for collision (not trigger)
        CircleCollider2D physicsCol = gameObject.AddComponent<CircleCollider2D>();
        physicsCol.radius = 0.4f;  // Same or slightly smaller than trigger
        physicsCol.isTrigger = false;  // IMPORTANT: Not a trigger!

        // Layer setup (do this in Inspector or code)
        gameObject.layer = LayerMask.NameToLayer("Child");
    }

    void ApplySeparation()
    {
        if (state == ChildState.Scared) return; // Scared children ignore separation

        Collider2D[] nearbyChildren = Physics2D.OverlapCircleAll(rb.position, separationRadius);
        Vector2 separation = Vector2.zero;
        int count = 0;

        foreach (Collider2D col in nearbyChildren)
        {
            if (col.gameObject != gameObject && col.CompareTag("LittleGirl"))
            {
                Vector2 awayFromChild = rb.position - (Vector2)col.transform.position;
                float distance = awayFromChild.magnitude;

                if (distance < separationRadius && distance > 0.01f)
                {
                    // Stronger push when very close, weaker when just touching
                    float strength = 1f - (distance / separationRadius);
                    separation += awayFromChild.normalized * strength;
                    count++;
                }
            }
        }

        if (count > 0)
        {
            separation /= count;
            // Apply gentle separation force (reduced from typical values)
            rb.AddForce(separation * separationForce * Time.deltaTime, ForceMode2D.Force);
        }
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
        ApplySeparation();

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
        if (hasBeenScared) return;
        if (wanderPoints == null || wanderPoints.Length == 0) return;

        // Find which points nearby children are already heading to
        Collider2D[] nearby = Physics2D.OverlapCircleAll(rb.position, separationRadius * 3f);
        List<Transform> takenPoints = new List<Transform>();

        foreach (Collider2D col in nearby)
        {
            if (col.gameObject != gameObject && col.CompareTag("LittleGirl"))
            {
                ChildAI other = col.GetComponent<ChildAI>();
                if (other != null && other.currentTarget != null)
                    takenPoints.Add(other.currentTarget);
            }
        }

        // Prefer points not already targeted
        List<Transform> freePoints = new List<Transform>();
        foreach (Transform point in wanderPoints)
        {
            if (!takenPoints.Contains(point))
                freePoints.Add(point);
        }

        // Pick from free points, fall back to any point if all taken
        List<Transform> pool = freePoints.Count > 0 ? freePoints : new List<Transform>(wanderPoints);
        currentTarget = pool[Random.Range(0, pool.Count)];
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

    // Add this method to receive room references
    public void SetRoomReferences(Transform[] roomWanderPoints, Transform roomExitPoint)
    {
        // Only set if not already initialized or if forced
        if (!isInitialized)
        {
            wanderPoints = roomWanderPoints;
            exitPoint = roomExitPoint;
            isInitialized = true;

            // Re-initialize wander target
            if (state == ChildState.Wander && !hasBeenScared)
            {
                SetNextWanderTarget();
            }
        }
    }

    // Enhanced FindRoomReferences for multiple children
void FindRoomReferences()
    {
        // Try parent first
        Transform currentParent = transform.parent;

        while (currentParent != null)
        {
            // Check if parent has RoomData
            RoomData roomData = currentParent.GetComponent<RoomData>();
            if (roomData != null)
            {
                SetRoomReferences(roomData.wanderPoints, roomData.exitPoint);
                return;
            }

            // Look for WanderPoints and ExitPoint as children of parent
            Transform wanderParent = currentParent.Find("WanderPoints");
            if (wanderParent != null && (wanderPoints == null || wanderPoints.Length == 0))
            {
                List<Transform> points = new List<Transform>();
                foreach (Transform child in wanderParent)
                {
                    points.Add(child);
                }
                wanderPoints = points.ToArray();
            }

            if (exitPoint == null)
            {
                Transform roomExit = currentParent.Find("ExitPoint");
                if (roomExit != null)
                    exitPoint = roomExit;
            }

            // If we found both, we're done
            if (wanderPoints != null && wanderPoints.Length > 0 && exitPoint != null)
            {
                isInitialized = true;
                return;
            }

            currentParent = currentParent.parent;
        }

        // Last resort: search by tag
        if (wanderPoints == null || wanderPoints.Length == 0)
        {
            GameObject[] wanderPointObjects = GameObject.FindGameObjectsWithTag("WanderPoint");
            if (wanderPointObjects.Length > 0)
            {
                wanderPoints = new Transform[wanderPointObjects.Length];
                for (int i = 0; i < wanderPointObjects.Length; i++)
                    wanderPoints[i] = wanderPointObjects[i].transform;
            }
        }
    }
   void OnCollisionStay2D(Collision2D collision)
    {
        // If stuck against another child
        if (collision.gameObject.CompareTag("LittleGirl") && !isIdle && state != ChildState.Scared)
        {
            // Calculate separation force (push away from other child)
            Vector2 awayFromOther = (rb.position - (Vector2)collision.transform.position).normalized;

            // MUCH SMALLER force - using 0.15f instead of 0.5f
            float separationForce = GetCurrentSpeed() * 0.05f * Time.deltaTime;
            rb.AddForce(awayFromOther * separationForce, ForceMode2D.Force);

            // Only check if stuck occasionally (every 60 frames instead of 30)
            if (Time.frameCount % 60 == 0)
            {
                CheckIfStuck();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Initial separation push when they first touch
        if (collision.gameObject.CompareTag("LittleGirl") && !isIdle)
        {
            Vector2 awayFromOther = (rb.position - (Vector2)collision.transform.position).normalized;
            // Gentle initial push
            rb.AddForce(awayFromOther * wanderSpeed * 0.03f, ForceMode2D.Impulse);
        }
    }
    void CheckIfStuck()
    {
        // If velocity is near zero but should be moving
        if (rb.linearVelocity.magnitude < 0.1f && state != ChildState.Scared && !isIdle)
        {
            // Force recalculate path
            if (currentTarget != null && seeker.IsDone())
            {
                seeker.StartPath(rb.position, currentTarget.position, OnPathComplete);
            }

            // Add random force to break deadlock
            Vector2 randomForce = Random.insideUnitCircle * wanderSpeed * 0.3f;
            rb.AddForce(randomForce, ForceMode2D.Impulse);
        }
    }

}