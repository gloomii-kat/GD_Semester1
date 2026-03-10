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
