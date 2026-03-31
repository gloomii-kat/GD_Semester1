using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public class ChildAI : MonoBehaviour
{
    public Transform target; // Will be set dynamically
    public float speed = 200f;
    public float nextWaypointDistance = 3f;

    [Header("Movement Points")]
    public Transform[] toiletStalls; // Assign toilet stall positions in inspector
    public Transform[] bathroomPoints; // Random points in bathroom to run to when scared

    [Header("Idle Behavior")]
    public float idleTimeAtStalls = 3f; // Time to wait at each stall

    private Path path;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;

    private Seeker seeker;
    private Rigidbody2D rb;

    private float originalSpeed;

    // New variables for idle behavior
    private bool isIdle = false;
    private float idleTimer = 0f;
    private int currentStallIndex = -1;

    [Header("Exit Behaviour")]
    public Transform bathroomExitPoint;
    private bool isEscaping = false;
    private bool isPaused = false;



    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        originalSpeed = speed;

        // Start by moving to a random toilet stall
        if (toiletStalls != null && toiletStalls.Length > 0)
        {
            SetRandomStallAsTarget();
        }

        InvokeRepeating("UpdatePath", 0f, .5f);
    }

    void Update()
    {

        // Handle idle timer
        if (isIdle)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0)
            {
                StopIdle();
            }
        }
    }

    void SetRandomStallAsTarget()
    {
        int randomIndex;

        // Make sure we don't pick the same stall twice in a row
        do
        {
            randomIndex = Random.Range(0, toiletStalls.Length);
        } while (toiletStalls.Length > 1 && randomIndex == currentStallIndex);

        currentStallIndex = randomIndex;
        target = toiletStalls[currentStallIndex];
    }

    void SetRandomBathroomPointAsTarget()
    {
        if (bathroomPoints != null && bathroomPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, bathroomPoints.Length);
            target = bathroomPoints[randomIndex];
        }
    }

    void StartIdle()
    {
        isIdle = true;
        idleTimer = idleTimeAtStalls;

        // Stop moving while idle
        rb.linearVelocity = Vector2.zero;
    }

    void StopIdle()
    {
        isIdle = false;

        // Move to the next random stall
        SetRandomStallAsTarget();
    }

    

    void UpdatePath()
    {
        if (target == null) return;

        // Don't update path if idle
        if (isIdle || isPaused) return;

        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    void FixedUpdate()
    {
        // Don't move if idle or screaming (screaming still moves, but we handle that separately)
        if (isIdle || isPaused)
        {
            return;
        }

        if (path == null || target == null)
        {
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;

        rb.AddForce(force);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        if (isEscaping && bathroomExitPoint != null)
        {
            float exitDistance = Vector2.Distance(rb.position, bathroomExitPoint.position);

            if (exitDistance < 0.5f)
            {
                rb.linearVelocity = Vector2.zero;
                enabled = false;
                Debug.Log("Child escaped the bathroom");
            }
        }

        // Check if we've reached the target (last waypoint)
        if (currentWaypoint >= path.vectorPath.Count)
        {
            // If we're not screaming and we're at a toilet stall, start idling
            if (!isEscaping && toiletStalls != null && System.Array.Exists(toiletStalls, stall => stall == target))
            {
                StartIdle();
            }
        }
    }

    public void TriggerConfusedPause(float pauseDuration)
    {
        if (!isEscaping)
        {
            StartCoroutine(ConfusedPauseRoutine(pauseDuration));
        }
    }

    public void TriggerEscape(float speedMultiplier)
    {
        if (isEscaping) return;

        isEscaping = true;
        isIdle = false;

        speed = originalSpeed * speedMultiplier;

        if (bathroomExitPoint != null)
        {
            target = bathroomExitPoint;
        }
    }

    IEnumerator ConfusedPauseRoutine(float pauseDuration)
    {
        isPaused = true;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(pauseDuration);

        isPaused = false;

        if (!isEscaping && target == null && toiletStalls != null && toiletStalls.Length > 0)
        {
            SetRandomStallAsTarget();
        }
    }
}