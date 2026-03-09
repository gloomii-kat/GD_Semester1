using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ChildAI.cs
/// State machine for a child NPC.
///
/// States:
///   Idle      -> wandering / sleeping in dorm
///   Scared    -> panicking, fear builds for player
///   Reporting -> running to find a teacher (raises awareness)
///   Frozen    -> paralysed with fear (high-value scare state)
///
/// Requires: NavMeshAgent OR simple 2D pathfinding.
/// For 2D top-down, swap NavMeshAgent with a custom 2D mover if needed.
/// </summary>
public class ChildAI : MonoBehaviour
{
    // -- State enum -------------------------------------------------
    public enum ChildState { Idle, Scared, Reporting, Frozen }
    public ChildState CurrentState { get; private set; } = ChildState.Idle;

    [Header("Behaviour Thresholds")]
    [Tooltip("Fear amount that makes this child run to report")]
    public float reportThreshold = 25f;
    [Tooltip("Fear amount that freezes this child in place")]
    public float freezeThreshold = 50f;
    [Tooltip("Awareness added to the system when this child successfully reports")]
    public float reportAwarenessValue = 15f;

    [Header("Recovery")]
    [Tooltip("Seconds in Scared state before child calms down if nothing else happens")]
    public float scaredDuration = 8f;

    [Header("Witnessed Kill Multiplier")]
    [Tooltip("If this child witnesses another child being scared, multiply fear gain")]
    public float witnessMultiplier = 2f;

    [Header("Teacher Reference")]
    [Tooltip("Drag a TeacherPatrol GameObject here, or leave null to find one at runtime")]
    public TeacherPatrol nearestTeacher;

    // -- Private ----------------------------------------------------
    private float accumulatedFear = 0f;  // this child's personal fear level
    private Coroutine scaredRoutine;
    private Animator anim;

    private static readonly int HashState = Animator.StringToHash("ChildState");

    void Awake()
    {
        anim = GetComponent<Animator>();
        if (nearestTeacher == null)
            nearestTeacher = FindFirstObjectByType<TeacherPatrol>();
    }

    // -- Public API -- called by ScareAbilityManager -----------------

    /// <summary>
    /// Pinky Pinky performed a scare near this child.
    /// fearAmount: base fear from the ability.
    /// wasManifested: if Pinky was visible, bonus fear applies.
    /// </summary>
    public void OnScared(float fearAmount, bool wasManifested)
    {
        if (CurrentState == ChildState.Frozen) return; // already max-scared

        float bonus = wasManifested ? 1.5f : 1f;
        accumulatedFear += fearAmount * bonus;

        // Transition logic
        if (accumulatedFear >= freezeThreshold)
            EnterFrozen();
        else if (accumulatedFear >= reportThreshold)
            EnterReporting();
        else
            EnterScared();
    }

    /// <summary>
    /// Another child nearby was scared. Witnessing it adds bonus fear.
    /// Called by ChildAI on its neighbours via OverlapCircle.
    /// </summary>
    public void OnWitnessedScare(float fearAmount)
    {
        OnScared(fearAmount * witnessMultiplier, false);
    }

    // -- State transitions ------------------------------------------

    void EnterScared()
    {
        CurrentState = ChildState.Scared;
        SetAnimState(1);
        Debug.Log($"[ChildAI] {name} is scared. Accumulated fear: {accumulatedFear:F1}");

        if (scaredRoutine != null) StopCoroutine(scaredRoutine);
        scaredRoutine = StartCoroutine(ScaredRecovery());

        // Notify witnesses within a small radius
        NotifyWitnesses();
    }

    void EnterReporting()
    {
        CurrentState = ChildState.Reporting;
        SetAnimState(2);
        Debug.Log($"[ChildAI] {name} is running to report!");

        StartCoroutine(RunToTeacher());
    }

    void EnterFrozen()
    {
        CurrentState = ChildState.Frozen;
        SetAnimState(3);
        Debug.Log($"[ChildAI] {name} is frozen with fear!");

        // Frozen child gives a big one-time fear boost and doesn't report
        BeliefSystem.Instance.AddFear(20f);

        // Notify witnesses -- seeing someone frozen is terrifying
        NotifyWitnesses();
    }

    void ReturnToIdle()
    {
        CurrentState = ChildState.Idle;
        accumulatedFear = Mathf.Max(0f, accumulatedFear - 10f); // partial recovery
        SetAnimState(0);
    }

    // -- Coroutines -------------------------------------------------

    IEnumerator ScaredRecovery()
    {
        yield return new WaitForSeconds(scaredDuration);
        if (CurrentState == ChildState.Scared)
            ReturnToIdle();
    }

    IEnumerator RunToTeacher()
    {
        if (nearestTeacher == null)
        {
            Debug.LogWarning("[ChildAI] No teacher found to report to.");
            yield break;
        }

        // Simple approach: move toward teacher position each frame
        float speed = 2.5f;
        float reportDistance = 1.2f;

        while (CurrentState == ChildState.Reporting)
        {
            Vector3 dir = (nearestTeacher.transform.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;

            if (Vector3.Distance(transform.position, nearestTeacher.transform.position) <= reportDistance)
            {
                // Reached the teacher -- raise awareness
                BeliefSystem.Instance.AddAwareness(reportAwarenessValue);
                nearestTeacher.OnChildReported();
                Debug.Log($"[ChildAI] {name} reported to teacher! +{reportAwarenessValue} Awareness");
                ReturnToIdle();
                yield break;
            }

            yield return null;
        }
    }

    // -- Helpers ----------------------------------------------------

    void NotifyWitnesses()
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, 3f);
        foreach (var col in nearby)
        {
            if (col.gameObject == gameObject) continue;
            ChildAI witness = col.GetComponent<ChildAI>();
            if (witness != null && witness.CurrentState == ChildState.Idle)
                witness.OnWitnessedScare(accumulatedFear * 0.5f);
        }
    }

    void SetAnimState(int stateIndex)
    {
        if (anim != null)
            anim.SetInteger(HashState, stateIndex);
    }

    // Draw detection gizmo in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 3f);
    }
}