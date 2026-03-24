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

    [Header("Screaming Behavior")]
    public float screamDuration = 5f;
    public float runSpeedMultiplier = 2f;

    [Header("Idle Behavior")]
    public float idleTimeAtStalls = 3f; // Time to wait at each stall

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip screamSound;

    private Path path;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;

    private Seeker seeker;
    private Rigidbody2D rb;

    private bool isScreaming = false;
    private float screamTimer = 0f;
    private float originalSpeed;

    // New variables for idle behavior
    private bool isIdle = false;
    private float idleTimer = 0f;
    private int currentStallIndex = -1;

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
        // Handle screaming state timer
        if (isScreaming)
        {
            screamTimer -= Time.deltaTime;
            if (screamTimer <= 0)
            {
                StopScreaming();
            }
        }

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

    // Call this method when you want the child to start screaming (e.g., from another script)
    public void TriggerScream()
    {
        if (!isScreaming)
        {
            StartScreaming();
        }
    }

    void StartScreaming()
    {
        isScreaming = true;
        isIdle = false; // Interrupt idle if screaming
        screamTimer = screamDuration;

        // Play scream sound
        if (audioSource != null && screamSound != null)
        {
            audioSource.PlayOneShot(screamSound);
        }

        // Increase speed for running
        speed = originalSpeed * runSpeedMultiplier;

        // Run to random bathroom point
        SetRandomBathroomPointAsTarget();
    }

    void StopScreaming()
    {
        isScreaming = false;
        speed = originalSpeed;

        // Go back to a random stall
        SetRandomStallAsTarget();
    }

    void UpdatePath()
    {
        if (target == null) return;

        // Don't update path if idle
        if (isIdle) return;

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
        if (isIdle)
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

        // Check if we've reached the target (last waypoint)
        if (currentWaypoint >= path.vectorPath.Count)
        {
            // If we're not screaming and we're at a toilet stall, start idling
            if (!isScreaming && toiletStalls != null && System.Array.Exists(toiletStalls, stall => stall == target))
            {
                StartIdle();
            }
        }
    }
}