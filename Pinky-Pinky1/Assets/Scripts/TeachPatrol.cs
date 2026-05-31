using UnityEngine;
using System.Collections;

public class TeacherPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] hallwayPatrolPoints;    // Points in the hallway
    public float patrolSpeed = 2f;
    public float waitTimeAtPoints = 1f;

    [Header("Waypoint Flipping")]
    public bool flipAtWaypoints = true;        // Flip sprite at waypoints
    private bool hasFlippedAtCurrentWaypoint = false;

    [Header("References")]
    public TeacherChase chaseScript;  // Reference to the chase script

    // Public properties for other scripts to read
    public bool IsMoving { get; private set; } = false;
    public Vector3 CurrentPosition => transform.position;

    private Rigidbody2D rb;

    // Patrol state
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    // Movement control
    private Vector2 currentMoveDirection = Vector2.zero;
    private float currentSpeed = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 0f;
        }

        // Find chase script if not assigned
        if (chaseScript == null)
            chaseScript = GetComponent<TeacherChase>();
    }

    void OnEnable()
    {
        isWaiting = false;
        currentPatrolIndex = 0;
        hasFlippedAtCurrentWaypoint = false;

        Debug.Log("Teacher Patrol: Starting hallway patrol");
    }

    void OnDisable()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        // If chase script is active and chasing, don't do patrol movement
        if (chaseScript != null && chaseScript.IsChasing)
        {
            // Let chase script handle movement
            return;
        }

        UpdatePatrolHallway();

        // Apply movement
        if (rb != null)
        {
            rb.linearVelocity = currentMoveDirection * currentSpeed;
        }
    }

    void UpdatePatrolHallway()
    {
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
                currentMoveDirection = Vector2.zero;
                currentSpeed = 0f;
                return;
            }
        }

        // Move to current patrol point
        if (hallwayPatrolPoints != null && hallwayPatrolPoints.Length > 0)
        {
            Transform target = hallwayPatrolPoints[currentPatrolIndex];
            if (target != null)
            {
                Vector2 direction = (target.position - transform.position).normalized;
                currentMoveDirection = direction;
                currentSpeed = patrolSpeed;
                IsMoving = true;

                // Check if reached target
                float distance = Vector2.Distance(transform.position, target.position);
                if (distance < 0.3f)
                {
                    // Flip at waypoint if enabled
                    if (flipAtWaypoints && !hasFlippedAtCurrentWaypoint)
                    {
                        FlipTowardsNextWaypoint();
                        hasFlippedAtCurrentWaypoint = true;
                    }

                    isWaiting = true;
                    waitTimer = waitTimeAtPoints;
                    currentMoveDirection = Vector2.zero;
                    currentSpeed = 0f;
                    IsMoving = false;
                    Debug.Log($"Reached hallway point {currentPatrolIndex}");
                }
                else if (distance > 1f)
                {
                    // Reset flip flag when moving away from waypoint
                    hasFlippedAtCurrentWaypoint = false;
                }
            }
        }
    }

    void MoveToNextPatrolPoint()
    {
        if (hallwayPatrolPoints == null || hallwayPatrolPoints.Length == 0) return;
        currentPatrolIndex = (currentPatrolIndex + 1) % hallwayPatrolPoints.Length;
        Debug.Log($"Moving to hallway point {currentPatrolIndex}");
    }

    void FlipTowardsNextWaypoint()
    {
        if (hallwayPatrolPoints == null || hallwayPatrolPoints.Length == 0) return;

        // Get next waypoint index
        int nextIndex = (currentPatrolIndex + 1) % hallwayPatrolPoints.Length;

        if (nextIndex < hallwayPatrolPoints.Length && hallwayPatrolPoints[nextIndex] != null)
        {
            float direction = Mathf.Sign(hallwayPatrolPoints[nextIndex].position.x - transform.position.x);

            Vector3 localScale = transform.localScale;

            if (direction > 0)
            {
                // Face right
                localScale.x = Mathf.Abs(localScale.x);
                Debug.Log($"Flipping to face RIGHT (next waypoint is to the right)");
            }
            else if (direction < 0)
            {
                // Face left
                localScale.x = -Mathf.Abs(localScale.x);
                Debug.Log($"Flipping to face LEFT (next waypoint is to the left)");
            }

            transform.localScale = localScale;
        }
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
        hasFlippedAtCurrentWaypoint = false;
        Debug.Log($"Resuming patrol from nearest point {currentPatrolIndex}");
    }

    // Public method to stop patrol movement (called by chase script)
    public void StopPatrolMovement()
    {
        currentMoveDirection = Vector2.zero;
        currentSpeed = 0f;
        IsMoving = false;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    // Public method to resume patrol after chase
    public void ResumePatrol()
    {
        FindNearestHallwayPoint();
    }

    // Public method to get current waypoint (for WaypointFlipper if needed)
    public Transform GetCurrentWaypoint()
    {
        if (hallwayPatrolPoints != null && hallwayPatrolPoints.Length > 0 && currentPatrolIndex < hallwayPatrolPoints.Length)
        {
            return hallwayPatrolPoints[currentPatrolIndex];
        }
        return null;
    }

    void OnDrawGizmosSelected()
    {
        if (hallwayPatrolPoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform point in hallwayPatrolPoints)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, 0.3f);
            }

            // Draw arrows showing flip directions
            if (flipAtWaypoints && hallwayPatrolPoints.Length >= 2)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < hallwayPatrolPoints.Length - 1; i++)
                {
                    if (hallwayPatrolPoints[i] != null && hallwayPatrolPoints[i + 1] != null)
                    {
                        Vector3 direction = hallwayPatrolPoints[i + 1].position - hallwayPatrolPoints[i].position;
                        Vector3 center = (hallwayPatrolPoints[i].position + hallwayPatrolPoints[i + 1].position) / 2;
                        Vector3 arrowTip = center + direction.normalized * 0.5f;
                        Gizmos.DrawLine(center, arrowTip);
                    }
                }
            }
        }
    }
}