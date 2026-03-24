using UnityEngine;
using UnityEngine.AI;

public class ChildAI_Movement : MonoBehaviour
{
    public Transform[] waypoints; // Array of waypoints for the child to follow
    private int currentWaypointIndex = 0; // Index of the current waypoint
    private NavMeshAgent agent; // Reference to the NavMeshAgent component

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>(); // Get the NavMeshAgent component
        agent.updateRotation = false; // Allow the NavMeshAgent to update the rotation of the child
        agent.updateUpAxis = false; // Allow the NavMeshAgent to update the position of the child

        agent.SetDestination(waypoints[currentWaypointIndex].position); // Set the initial destination to the first waypoint
        

    }

    // Update is called once per frame
    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f) // Check if the agent has reached the current waypoint
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length; // Move to the next waypoint, looping back to the start if necessary
            agent.SetDestination(waypoints[currentWaypointIndex].position); // Set the new destination
        }
    }
}

/*using System.Collections;

public class ChildAI_Movement : MonoBehaviour
{
    public Transform monster;      // Drag the Monster object here
    public float fleeDistance = 5f; // How far to run away
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;
    [Range(0, 100)]
    public float stopChance = 25f;  // % chance to stop when reaching a point

    private NavMeshAgent agent;
    private bool isWaiting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    void Update()
    {
        if (isWaiting) return;

        // 1. If the monster is close, calculate a fleeing destination
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            // Decide whether to stop and hide/freeze or keep running
            if (Random.Range(0, 100) < stopChance)
            {
                StartCoroutine(StopAndCower());
            }
            else
            {
                FleeFromMonster();
            }
        }
    }

    void FleeFromMonster()
    {
        // Calculate direction away from monster
        Vector3 runDirection = transform.position - monster.position;

        // Find a target position in that direction
        Vector3 fleeTarget = transform.position + runDirection.normalized * fleeDistance;

        // Find the nearest valid point on the NavMesh so the agent doesn't run into a void
        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleeTarget, out hit, fleeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    IEnumerator StopAndCower()
    {
        isWaiting = true;
        agent.isStopped = true;

        // Randomly "freeze" in fear
        yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));

        agent.isStopped = false;
        isWaiting = false;
        FleeFromMonster(); // Resume fleeing after the pause
    }
}*/
