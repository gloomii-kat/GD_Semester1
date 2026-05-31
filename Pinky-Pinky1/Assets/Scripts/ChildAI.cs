using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildAI : MonoBehaviour
{
    [Header("Movement")]
    public float wanderSpeed = 150f;
    public float scaredSpeed = 300f;
    public float nextWaypointDistance = 3f;

    [Header("Wander Points")]
    public Transform[] wanderPoints;

    [Header("Idle")]
    public float idleTimeAtPoints = 2f;

    [Header("Scare")]
    public Transform exitPoint;
    public float scareCooldown = 2f;
    private bool isScareReady = true;
    private bool hasBeenScared = false;

    [Header("Lure")]
    public float lureDuration = 3f;

    [Header("References")]
    public Transform pinky;

    [Header("Separation")]
    public float separationRadius = 1.5f;
    public float separationForce = 15f;

    // Internal state
    private bool isInitialized = false;

    private enum ChildState { Wander, Lured, Scared }
    private ChildState state = ChildState.Wander;

    private Path path;
    private int currentWaypoint = 0;
    private Seeker seeker;
    private Rigidbody2D rb;

    private bool isIdle = false;
    private float idleTimer = 0f;

    // Public so nearby children can check each other's targets
    public Transform currentTarget;

    // Cooldown to prevent SetNextWanderTarget being called too rapidly on collision
    private float retargetCooldown = 0f;

    public static event System.Action OnChildScared;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        if (!isInitialized)
        {
            RoomData roomData = GetComponentInParent<RoomData>();
            if (roomData != null)
            {
                Debug.Log($"{gameObject.name} waiting for RoomData initialization...");
            }
            else
            {
                FindRoomReferences();
            }
        }

        hasBeenScared = false;

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

        // Keep existing trigger collider as-is
        CircleCollider2D triggerCol = GetComponent<CircleCollider2D>();
        if (triggerCol != null)
            triggerCol.isTrigger = true;

        // Add a small solid collider for physical separation
        CircleCollider2D physicsCol = gameObject.AddComponent<CircleCollider2D>();
        physicsCol.radius = 0.4f;
        physicsCol.isTrigger = false;

        // Safe layer assignment — only set if the layer actually exists
        int childLayer = LayerMask.NameToLayer("Child");
        if (childLayer != -1)
            gameObject.layer = childLayer;
        else
            Debug.LogWarning($"{gameObject.name}: 'Child' layer not found. Add it in Edit > Project Settings > Tags and Layers, or ignore this if you're not using layers.");

        SetNextWanderTarget();
        InvokeRepeating(nameof(UpdatePath), 0f, 0.5f);
    }

    void ApplySeparation()
    {
        if (state == ChildState.Scared) return;

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
                    float strength = 1f - (distance / separationRadius);
                    separation += awayFromChild.normalized * strength;
                    count++;
                }
            }
        }

        if (count > 0)
        {
            separation /= count;
            rb.AddForce(separation * separationForce * Time.deltaTime, ForceMode2D.Force);
        }
    }

    void Update()
    {
        if (retargetCooldown > 0f)
            retargetCooldown -= Time.deltaTime;

        switch (state)
        {
            case ChildState.Wander:
                HandleIdle();
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

    void SetNextWanderTarget()
    {
        if (hasBeenScared) return;
        if (wanderPoints == null || wanderPoints.Length == 0) return;

        if (rb == null)
        {
            currentTarget = wanderPoints[Random.Range(0, wanderPoints.Length)];
            return;
        }

        // Find which points nearby children are already heading to
        Collider2D[] nearby = Physics2D.OverlapCircleAll(rb.position, separationRadius * 4f);
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

        // Prefer points not already targeted by someone nearby
        List<Transform> freePoints = new List<Transform>();
        foreach (Transform point in wanderPoints)
            if (!takenPoints.Contains(point))
                freePoints.Add(point);

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
        if (hasBeenScared) return;
        isIdle = true;
        idleTimer = idleTimeAtPoints;
        rb.linearVelocity = Vector2.zero;
    }

    public void Scare()
    {
        if (hasBeenScared) return;
        if (state == ChildState.Scared) return;
        if (!isScareReady) return;
        StartCoroutine(ScareRoutine());
    }

    IEnumerator ScareRoutine()
    {
        hasBeenScared = true;
        isScareReady = false;
        state = ChildState.Scared;
        isIdle = false;
        rb.linearVelocity = Vector2.zero;

        if (exitPoint != null)
        {
            currentTarget = exitPoint;
            Debug.Log($"{gameObject.name} was scared! Running to exit!");
        }
        else
        {
            Debug.LogError($"{gameObject.name}: Exit point not assigned!");
        }

        OnChildScared?.Invoke();

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
            Debug.Log($"{gameObject.name} escaped!");
        }
    }

    public void Lure(Transform lureTarget)
    {
        if (hasBeenScared) return;
        if (state == ChildState.Scared) return;
        StartCoroutine(LureRoutine(lureTarget));
    }

    IEnumerator LureRoutine(Transform lureTarget)
    {
        state = ChildState.Lured;
        isIdle = false;
        currentTarget = lureTarget;

        yield return new WaitForSeconds(lureDuration);

        if (state == ChildState.Lured && !hasBeenScared)
        {
            state = ChildState.Wander;
            SetNextWanderTarget();
        }
    }

    public void TriggerEscape(float speedMultiplier) => Scare();

    float GetCurrentSpeed() => state == ChildState.Scared ? scaredSpeed : wanderSpeed;

    public bool IsScared() => hasBeenScared;

    public void SetRoomReferences(Transform[] roomWanderPoints, Transform roomExitPoint)
    {
        if (!isInitialized)
        {
            wanderPoints = roomWanderPoints;
            exitPoint = roomExitPoint;
            isInitialized = true;

            if (state == ChildState.Wander && !hasBeenScared)
                SetNextWanderTarget();
        }
    }

    void FindRoomReferences()
    {
        Transform currentParent = transform.parent;

        while (currentParent != null)
        {
            RoomData roomData = currentParent.GetComponent<RoomData>();
            if (roomData != null)
            {
                SetRoomReferences(roomData.wanderPoints, roomData.exitPoint);
                return;
            }

            Transform wanderParent = currentParent.Find("WanderPoints");
            if (wanderParent != null && (wanderPoints == null || wanderPoints.Length == 0))
            {
                List<Transform> points = new List<Transform>();
                foreach (Transform child in wanderParent)
                    points.Add(child);
                wanderPoints = points.ToArray();
            }

            if (exitPoint == null)
            {
                Transform roomExit = currentParent.Find("ExitPoint");
                if (roomExit != null)
                    exitPoint = roomExit;
            }

            if (wanderPoints != null && wanderPoints.Length > 0 && exitPoint != null)
            {
                isInitialized = true;
                return;
            }

            currentParent = currentParent.parent;
        }

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
        if (!collision.gameObject.CompareTag("LittleGirl") || isIdle || state == ChildState.Scared) return;

        Vector2 awayFromOther = (rb.position - (Vector2)collision.transform.position).normalized;
        float force = GetCurrentSpeed() * 0.02f * Time.deltaTime;
        rb.AddForce(awayFromOther * force, ForceMode2D.Force);

        if (Time.frameCount % 60 == 0)
            CheckIfStuck();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("LittleGirl") || isIdle || hasBeenScared) return;

        // Push apart
        Vector2 awayFromOther = (rb.position - (Vector2)collision.transform.position).normalized;
        rb.AddForce(awayFromOther * wanderSpeed * 0.03f, ForceMode2D.Impulse);

        // Pick a new target — but only if cooldown has expired so we don't spam this
        if (retargetCooldown <= 0f)
        {
            SetNextWanderTarget();
            retargetCooldown = 1f; // Wait 1 second before retargeting again
        }
    }

    void CheckIfStuck()
    {
        if (rb.linearVelocity.magnitude < 0.1f && state != ChildState.Scared && !isIdle)
        {
            if (currentTarget != null && seeker.IsDone())
                seeker.StartPath(rb.position, currentTarget.position, OnPathComplete);

            Vector2 randomForce = Random.insideUnitCircle * wanderSpeed * 0.3f;
            rb.AddForce(randomForce, ForceMode2D.Impulse);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Dotted trail to current target
        if (currentTarget != null)
        {
            Gizmos.color = state == ChildState.Scared ? Color.red : Color.cyan;
            Vector3 start = transform.position;
            Vector3 end = currentTarget.position;
            float totalDist = Vector3.Distance(start, end);
            int dotCount = Mathf.FloorToInt(totalDist / 0.3f);

            for (int i = 0; i <= dotCount; i++)
            {
                float t = dotCount == 0 ? 0 : (float)i / dotCount;
                Gizmos.DrawSphere(Vector3.Lerp(start, end, t), 0.08f);
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(end, 0.25f);
        }

        if (exitPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, exitPoint.position);
            Gizmos.DrawWireSphere(exitPoint.position, 0.5f);
        }

        if (wanderPoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform point in wanderPoints)
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, 0.3f);
        }
    }
}