using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TeacherPatrol.cs
/// Teacher NPC -- the main threat to the player.
///
/// States:
///   Patrolling   -> routine path through dorm/corridor
///   Investigating -> moves toward last known disturbance
///   Alerted       -> can see Pinky Pinky if manifested; slows her progression
///
/// The teacher cannot directly harm the player, but being seen while in
/// Alerted state drains the Fear bar and blocks ability use.
///
/// Awareness threshold determines which state the teacher is in:
///   0-33   -> Patrolling  (slow, predictable)
///   34-66  -> Investigating (faster, follows reports)
///   67-100 -> Alerted (can detect manifested player)
/// </summary>
public class TeacherPatrol : MonoBehaviour
{
    public enum TeacherState { Patrolling, Investigating, Alerted }
    public TeacherState CurrentState { get; private set; } = TeacherState.Patrolling;

    [Header("Patrol Waypoints")]
    [Tooltip("Place empty GameObjects in the scene and drag them here")]
    public List<Transform> patrolWaypoints = new List<Transform>();

    [Header("Movement Speeds")]
    public float patrolSpeed = 1.5f;
    public float investigateSpeed = 2.5f;
    public float alertedSpeed = 3f;

    [Header("Detection")]
    [Tooltip("How far the teacher can see the manifested player")]
    public float visionRange = 5f;
    [Tooltip("Field of view angle in degrees")]
    public float visionAngle = 90f;
    [Tooltip("Layer mask for the player")]
    public LayerMask playerLayer;

    [Header("Awareness Thresholds (mirrors BeliefSystem)")]
    public float investigateThreshold = 34f;
    public float alertedThreshold = 67f;

    // -- Private ----------------------------------------------------
    private int currentWaypointIndex = 0;
    private Vector3 investigateTarget;
    private float stateUpdateInterval = 0.5f; // check awareness every 0.5s
    private float stateTimer = 0f;

    private Transform playerTransform;

    void Start()
    {
        // Cache player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (patrolWaypoints.Count == 0)
            Debug.LogWarning("[TeacherPatrol] No patrol waypoints set!");
    }

    void Update()
    {
        if (!GameManager.Instance.NightActive) return;

        // Periodically re-evaluate state from global awareness
        stateTimer += Time.deltaTime;
        if (stateTimer >= stateUpdateInterval)
        {
            stateTimer = 0f;
            UpdateStateFromAwareness();
        }

        // Behave according to state
        switch (CurrentState)
        {
            case TeacherState.Patrolling: DoPatrol(); break;
            case TeacherState.Investigating: DoInvestigate(); break;
            case TeacherState.Alerted: DoAlerted(); break;
        }
    }

    // -- State logic ------------------------------------------------

    void DoPatrol()
    {
        if (patrolWaypoints.Count == 0) return;

        MoveToward(patrolWaypoints[currentWaypointIndex].position, patrolSpeed);

        if (Vector3.Distance(transform.position, patrolWaypoints[currentWaypointIndex].position) < 0.2f)
            currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Count;
    }

    void DoInvestigate()
    {
        MoveToward(investigateTarget, investigateSpeed);
        TryDetectPlayer();
    }

    void DoAlerted()
    {
        // In alerted state, move toward player if visible
        if (playerTransform != null && CanSeePlayer())
        {
            MoveToward(playerTransform.position, alertedSpeed);
            // Seeing the manifested player drains fear (disbelief effect)
            BeliefSystem.Instance.AddFear(-10f * Time.deltaTime);
            Debug.Log("[TeacherPatrol] Teacher sees Pinky Pinky! Fear draining...");
        }
        else
        {
            // Otherwise head toward last report
            MoveToward(investigateTarget, alertedSpeed * 0.7f);
        }
    }

    // -- Detection --------------------------------------------------

    bool CanSeePlayer()
    {
        if (playerTransform == null) return false;

        PlayerController pc = playerTransform.GetComponent<PlayerController>();
        if (pc == null || !pc.IsManifested) return false; // can only see manifested player

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist > visionRange) return false;

        Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.up, dirToPlayer); // assuming teacher faces "up"
        if (angle > visionAngle * 0.5f) return false;

        // Optionally add a Raycast here for line-of-sight vs walls
        return true;
    }

    void TryDetectPlayer()
    {
        if (CanSeePlayer())
            BeliefSystem.Instance.AddAwareness(20f * Time.deltaTime);
    }

    // -- Public API -- called by ChildAI -----------------------------

    /// <summary>A child has reached this teacher and reported a sighting.</summary>
    public void OnChildReported()
    {
        // Move investigation target to the child's last known position
        // (In a fuller game you'd pass the position -- for now investigate last known general area)
        investigateTarget = transform.position + (Vector3)(Random.insideUnitCircle * 5f);
        Debug.Log("[TeacherPatrol] Teacher received a report -- investigating.");
    }

    /// <summary>Direct external trigger to send teacher to a position (e.g. from a sound event).</summary>
    public void InvestigatePosition(Vector3 worldPos)
    {
        investigateTarget = worldPos;
    }

    // -- Helpers ----------------------------------------------------

    void UpdateStateFromAwareness()
    {
        if (BeliefSystem.Instance == null) return;

        float awareness = BeliefSystem.Instance.Awareness;

        if (awareness >= alertedThreshold)
            SetState(TeacherState.Alerted);
        else if (awareness >= investigateThreshold)
            SetState(TeacherState.Investigating);
        else
            SetState(TeacherState.Patrolling);
    }

    void SetState(TeacherState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
        Debug.Log($"[TeacherPatrol] Teacher is now: {newState}");
    }

    void MoveToward(Vector3 target, float speed)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    // Gizmos
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        if (patrolWaypoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (var wp in patrolWaypoints)
                if (wp != null) Gizmos.DrawSphere(wp.position, 0.2f);
        }
    }
}