using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public class ChildAI : MonoBehaviour
{
    public Transform target;
    public float speed = 200f;
    public float nextWaypointDistance = 3f;

    [Header("Movement Points")]
    public Transform[] toiletStalls;
    public Transform[] basinPoints;
    public Transform[] lightPoints;

    [Header("Idle Behavior")]
    public float idleTimeAtPoints = 2f;

    [Header("Exit Behaviour")]
    public Transform bathroomExitPoint;

    [Header("State-Based Pattern Breaking")]
    [Range(0f, 1f)] public float calmBreakChance = 0.1f;
    [Range(0f, 1f)] public float confusedBreakChance = 0.2f;
    [Range(0f, 1f)] public float agitatedBreakChance = 0.4f;
    [Range(0f, 1f)] public float panickedBreakChance = 0.7f;

    [Header("Teacher Activation")]
    public GameObject teacherObject; // Now referencing the GameObject, not the script
    private bool hasActivatedTeacher = false; // Add this flag to prevent multiple activations

    private float currentBreakChance = 0.1f;

    private Path path;
    private int currentWaypoint = 0;

    private Seeker seeker;
    private Rigidbody2D rb;

    private float originalSpeed;

    private bool isIdle = false;
    private float idleTimer = 0f;
    private bool isEscaping = false;
    private bool isPaused = false;

    // Pattern: Stall -> Basin -> Stall -> Light
    private int patternIndex = 0;

    private enum PatrolType
    {
        Stall,
        Basin,
        StallAgain,
        Light
    }

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        originalSpeed = speed;

        // Make sure teacher starts inactive
        if (teacherObject != null)
        {
            teacherObject.SetActive(false);
            Debug.Log("Teacher starts inactive (disabled)");
        }
        else
        {
            Debug.LogError("Teacher Object is not assigned in ChildAI Inspector!");
        }

        hasActivatedTeacher = false; // Initialize the flag

        //ValidatePatrolPoints();
        SetNextPatternTarget();
        InvokeRepeating(nameof(UpdatePath), 0f, 0.5f);
        InvokeRepeating(nameof(UpdatePath), 0f, 0.5f);
    }

    void Update()
    {
        if (isIdle)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0)
            {
                StopIdle();
            }
        }
    }

    void SetNextPatternTarget()
    {
        bool breakPattern = Random.value < currentBreakChance;

        if (breakPattern)
        {
            int randomCategory = Random.Range(0, 3); // 0 = Stall, 1 = Basin, 2 = Light

            switch (randomCategory)
            {
                case 0:
                    target = GetRandomPoint(toiletStalls);
                    Debug.Log("Pattern broken -> random Stall");
                    break;

                case 1:
                    target = GetRandomPoint(basinPoints);
                    Debug.Log("Pattern broken -> random Basin");
                    break;

                case 2:
                    target = GetRandomPoint(lightPoints);
                    Debug.Log("Pattern broken -> random Light");
                    break;
            }
        }
        else
        {
            PatrolType nextType = (PatrolType)patternIndex;

            switch (nextType)
            {
                case PatrolType.Stall:
                case PatrolType.StallAgain:
                    target = GetRandomPoint(toiletStalls);
                    Debug.Log("Following pattern -> Stall");
                    break;

                case PatrolType.Basin:
                    target = GetRandomPoint(basinPoints);
                    Debug.Log("Following pattern -> Basin");
                    break;

                case PatrolType.Light:
                    target = GetRandomPoint(lightPoints);
                    Debug.Log("Following pattern -> Light");
                    break;
            }

            patternIndex = (patternIndex + 1) % 4;
        }
    }

    Transform GetRandomPoint(Transform[] points)
    {
        if (points == null || points.Length == 0)
        {
            Debug.LogWarning("Point array is empty!");
            return null;
        }

        int randomIndex = Random.Range(0, points.Length);
        return points[randomIndex];
    }

    void StartIdle()
    {
        isIdle = true;
        idleTimer = idleTimeAtPoints;
        rb.linearVelocity = Vector2.zero;
    }

    void StopIdle()
    {
        isIdle = false;
        SetNextPatternTarget();
    }

    void UpdatePath()
    {
        if (target == null) return;
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
            return;
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

        if (currentWaypoint >= path.vectorPath.Count)
        {
            if (!isEscaping)
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

        if (!isEscaping)
        {
            SetNextPatternTarget();
        }
    }

    public void OnAwarenessFull()
    {
        // Only activate teacher once
        if (!hasActivatedTeacher && teacherObject != null)
        {
            hasActivatedTeacher = true;
            teacherObject.SetActive(true);
            Debug.Log("Child is fully aware! Teacher activated (GameObject enabled)!");
        }
        else if (teacherObject == null)
        {
            Debug.LogError("Teacher GameObject reference is NULL! Make sure to assign the Teacher GameObject in the Inspector.");
        }
        else if (hasActivatedTeacher)
        {
            Debug.Log("Teacher already activated, ignoring duplicate call.");
        }
    }

    public void SetBreakChanceByState(string stateName)
    {
        switch (stateName)
        {
            case "Calm":
                currentBreakChance = calmBreakChance;
                break;

            case "Confused":
                currentBreakChance = confusedBreakChance;
                break;

            case "Agitated":
                currentBreakChance = agitatedBreakChance;
                break;

            case "Panicked":
                currentBreakChance = panickedBreakChance;
                OnAwarenessFull();
                break;

            default:
                currentBreakChance = calmBreakChance;
                break;
        }

        Debug.Log("Current break chance set to: " + currentBreakChance + " for state " + stateName);
    }
}
