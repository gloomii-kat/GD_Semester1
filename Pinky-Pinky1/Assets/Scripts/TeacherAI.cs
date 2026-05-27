
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TeacherAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] hallwayPoints;     // Points along hallway
    public Transform[] roomCheckPoints;   // Points inside rooms to check
    public float patrolSpeed = 2f;
    public float roomCheckSpeed = 1.5f;

    [Header("Room Check Timing")]
    public float minTimeBetweenRoomChecks = 5f;
    public float maxTimeBetweenRoomChecks = 10f;
    public float timeSpentInRoom = 3f;

    [Header("Chase Settings")]
    public Transform playerTarget;
    public float chaseSpeed = 4f;
    public float chaseRange = 8f;
    public float loseSightRange = 12f;

    [Header("Search Settings")]
    public float searchSpeed = 2.5f;
    public float searchDuration = 5f;
    private Vector3 lastKnownPlayerPosition;

    [Header("Exit")]
    public Transform bathroomExit;
    private bool leavingBathroom = false;

    [Header("References")]
    public NightManager nightManager;
    public GameObject gotchaText;

    private AudioManager audioManager;
    private Rigidbody2D rb;
    private float currentSpeed;

    // Patrol state tracking
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    private float waitTimer;
    private bool isDoingRoomCheck = false;
    private int currentRoomPointIndex = 0;
    private Vector3 originalHallwayPosition;

    // FSM States
    private enum TeacherState
    {
        PatrolHallway,    // Walking the hallway
        EnteringRoom,     // Going into a room to check
        CheckingRoom,     // Moving around inside the room
        LeavingRoom,      // Going back to hallway
        Chase,            // Chasing player
        Search,           // Searching for player
        Exit              // Leaving with caught player
    }

    private TeacherState currentState;
    private float stateTimer;

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
        currentState = TeacherState.PatrolHallway;
        currentSpeed = patrolSpeed;

        // Start hallway patrol
        if (hallwayPoints != null && hallwayPoints.Length > 0)
        {
            currentPatrolIndex = 0;
        }

        // Schedule first room check
        ScheduleNextRoomCheck();

        Debug.Log("Teacher activated - PATROLLING HALLWAY");
    }

    void OnDisable()
    {
        rb.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        switch (currentState)
        {
            case TeacherState.PatrolHallway:
                UpdatePatrolHallwayState();
                break;
            case TeacherState.EnteringRoom:
                UpdateEnteringRoomState();
                break;
            case TeacherState.CheckingRoom:
                UpdateCheckingRoomState();
                break;
            case TeacherState.LeavingRoom:
                UpdateLeavingRoomState();
                break;
            case TeacherState.Chase:
                UpdateChaseState();
                break;
            case TeacherState.Search:
                UpdateSearchState();
                break;
            case TeacherState.Exit:
                UpdateExitState();
                break;
        }
    }

    void UpdatePatrolHallwayState()
    {
        // Check for player first
        if (IsPlayerInChaseRange())
        {
            TransitionToChase();
            return;
        }

        // Handle waiting between room checks
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                StartRoomCheck();
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }
        }

        // Patrol hallway points
        if (hallwayPoints != null && hallwayPoints.Length > 0)
        {
            MoveTowardsTarget(hallwayPoints[currentPatrolIndex].position, patrolSpeed);

            float distanceToPoint = Vector2.Distance(transform.position, hallwayPoints[currentPatrolIndex].position);
            if (distanceToPoint < 0.5f)
            {
                // Move to next hallway point
                currentPatrolIndex = (currentPatrolIndex + 1) % hallwayPoints.Length;
            }
        }
    }

    void UpdateEnteringRoomState()
    {
        // Check for player during room entry
        if (IsPlayerInChaseRange())
        {
            TransitionToChase();
            return;
        }

        if (roomCheckPoints != null && roomCheckPoints.Length > 0)
        {
            // Move to first room check point
            MoveTowardsTarget(roomCheckPoints[0].position, roomCheckSpeed);

            float distanceToRoom = Vector2.Distance(transform.position, roomCheckPoints[0].position);
            if (distanceToRoom < 0.5f)
            {
                // Entered the room, start checking around
                currentState = TeacherState.CheckingRoom;
                currentRoomPointIndex = 0;
                Debug.Log("Teacher entered room, now CHECKING inside");
            }
        }
    }

    void UpdateCheckingRoomState()
    {
        // Check for player during room check
        if (IsPlayerInChaseRange())
        {
            TransitionToChase();
            return;
        }

        if (roomCheckPoints != null && roomCheckPoints.Length > 0)
        {
            // Move to each check point inside the room
            MoveTowardsTarget(roomCheckPoints[currentRoomPointIndex].position, roomCheckSpeed);

            float distanceToPoint = Vector2.Distance(transform.position, roomCheckPoints[currentRoomPointIndex].position);
            if (distanceToPoint < 0.5f)
            {
                // Move to next room point
                currentRoomPointIndex++;

                if (currentRoomPointIndex >= roomCheckPoints.Length)
                {
                    // Finished checking all points in room
                    currentState = TeacherState.LeavingRoom;
                    Debug.Log("Teacher finished checking room, now LEAVING");
                }
            }
        }
        else
        {
            // No room points, just wait then leave
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                currentState = TeacherState.LeavingRoom;
            }
        }
    }

    void UpdateLeavingRoomState()
    {
        // Check for player while leaving
        if (IsPlayerInChaseRange())
        {
            TransitionToChase();
            return;
        }

        // Move back to hallway position (original patrol point)
        if (hallwayPoints != null && hallwayPoints.Length > 0)
        {
            MoveTowardsTarget(hallwayPoints[currentPatrolIndex].position, roomCheckSpeed);

            float distanceToHallway = Vector2.Distance(transform.position, hallwayPoints[currentPatrolIndex].position);
            if (distanceToHallway < 0.5f)
            {
                // Back in hallway, resume patrol
                currentState = TeacherState.PatrolHallway;
                ScheduleNextRoomCheck();
                Debug.Log("Teacher back in hallway, resuming PATROL");
            }
        }
    }

    void UpdateChaseState()
    {
        if (IsPlayerCaught())
        {
            CaughtPlayer();
            return;
        }

        if (IsPlayerInChaseRange())
        {
            lastKnownPlayerPosition = playerTarget.position;
            MoveTowardsTarget(playerTarget.position, chaseSpeed);
        }
        else if (IsPlayerInLoseRange())
        {
            TransitionToSearch();
        }
        else
        {
            TransitionToPatrol();
        }
    }

    void UpdateSearchState()
    {
        stateTimer -= Time.deltaTime;
        MoveTowardsTarget(lastKnownPlayerPosition, searchSpeed);

        if (stateTimer <= 0f)
        {
            TransitionToPatrol();
        }

        if (IsPlayerInChaseRange())
        {
            TransitionToChase();
        }
    }

    void UpdateExitState()
    {
        if (bathroomExit != null)
        {
            MoveTowardsTarget(bathroomExit.position, patrolSpeed);
        }
    }

    void MoveTowardsTarget(Vector3 target, float speed)
    {
        Vector2 direction = (target - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    void ScheduleNextRoomCheck()
    {
        float delay = Random.Range(minTimeBetweenRoomChecks, maxTimeBetweenRoomChecks);
        isWaiting = true;
        waitTimer = delay;
        Debug.Log($"Teacher will check a room in {delay} seconds");
    }

    void StartRoomCheck()
    {
        if (roomCheckPoints != null && roomCheckPoints.Length > 0)
        {
            currentState = TeacherState.EnteringRoom;
            Debug.Log("Teacher decided to CHECK a room");
        }
        else
        {
            // No rooms to check, just keep patrolling
            ScheduleNextRoomCheck();
        }
    }

    void TransitionToPatrol()
    {
        currentState = TeacherState.PatrolHallway;
        currentSpeed = patrolSpeed;

        Debug.Log("Teacher: Returned to PATROL");

        if (audioManager != null)
        {
            audioManager.RestoreBackgroundMusic();
        }

        ScheduleNextRoomCheck();
    }

    void TransitionToChase()
    {
        if (currentState == TeacherState.Exit) return;

        currentState = TeacherState.Chase;
        currentSpeed = chaseSpeed;

        Debug.Log("Teacher: CHASE - Spotted Pinky!");

        if (audioManager != null)
        {
            audioManager.PlayChaseMusic();
        }
    }

    void TransitionToSearch()
    {
        currentState = TeacherState.Search;
        currentSpeed = searchSpeed;
        stateTimer = searchDuration;
        lastKnownPlayerPosition = playerTarget.position;

        Debug.Log("Teacher: SEARCH - Lost Pinky");
    }

    void TransitionToExit()
    {
        currentState = TeacherState.Exit;
        leavingBathroom = true;
        Debug.Log("Teacher: EXITING with Pinky");
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

    bool IsPlayerCaught()
    {
        if (playerTarget == null) return false;
        return Vector2.Distance(transform.position, playerTarget.position) < 1.5f;
    }

    void CaughtPlayer()
    {
        Debug.Log("TEACHER CAUGHT PINKY!");
        rb.linearVelocity = Vector2.zero;

        if (gotchaText != null)
        {
            gotchaText.SetActive(true);
            StartCoroutine(HideGotchaTextAfterDelay(2f));
        }

        TransitionToExit();
    }

    IEnumerator HideGotchaTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (gotchaText != null)
            gotchaText.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        // Hallway points
        if (hallwayPoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform point in hallwayPoints)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, 0.3f);
            }
        }

        // Room check points
        if (roomCheckPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform point in roomCheckPoints)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, 0.3f);
            }
        }

        // Chase ranges
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, loseSightRange);
    }
}