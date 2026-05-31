using UnityEngine;
using System.Collections.Generic;

public class RoomChildManager : MonoBehaviour
{
    [Header("Room References")]
    public Transform[] roomWanderPoints;  // Shared wander points for all children
    public Transform roomExitPoint;       // Shared exit point

    [Header("Child Settings")]
    public GameObject childPrefab;
    public int numberOfChildren = 2;
    public Vector2 spawnAreaMin = new Vector2(-2, -2);
    public Vector2 spawnAreaMax = new Vector2(2, 2);

    [Header("Individual Child Settings")]
    public float minIdleTime = 2f;
    public float maxIdleTime = 5f;
    public float minWanderSpeed = 120f;
    public float maxWanderSpeed = 180f;

    private List<ChildAI> children = new List<ChildAI>();
    private int scaredChildrenCount = 0;

    void Start()
    {
        SpawnAllChildren();
    }

    void SpawnAllChildren()
    {
        for (int i = 0; i < numberOfChildren; i++)
        {
            // Calculate spawn position within room area
            Vector2 spawnPos = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );

            // Instantiate child as child of this room
            GameObject childObj = Instantiate(childPrefab, spawnPos, Quaternion.identity, transform);
            ChildAI childAI = childObj.GetComponent<ChildAI>();

            // Customize each child with slightly different behavior
            childAI.wanderPoints = roomWanderPoints;
            childAI.exitPoint = roomExitPoint;
            childAI.wanderSpeed = Random.Range(minWanderSpeed, maxWanderSpeed);
            childAI.idleTimeAtPoints = Random.Range(minIdleTime, maxIdleTime);

            // Optional: Give each child a unique name
            childObj.name = $"Child_{i + 1}";

            children.Add(childAI);

            // Subscribe to each child's scared event
            ChildAI.OnChildScared += OnChildScaredHandler;
        }
    }

    void OnChildScaredHandler()
    {
        scaredChildrenCount++;
        Debug.Log($"Child scared! {scaredChildrenCount}/{numberOfChildren} children scared in room {gameObject.name}");

        // Optional: Do something when all children are scared
        if (scaredChildrenCount >= numberOfChildren)
        {
            OnAllChildrenScared();
        }
    }

    void OnAllChildrenScared()
    {
        Debug.Log($"All {numberOfChildren} children have escaped from {gameObject.name}!");
        // You could unlock a door, trigger an event, etc.
    }

    // Method to scare all children in room at once
    public void ScareAllChildren()
    {
        foreach (ChildAI child in children)
        {
            if (child != null && !child.IsScared())
            {
                child.Scare();
            }
        }
    }

    // Method to lure all children
    public void LureAllChildren(Transform lureTarget)
    {
        foreach (ChildAI child in children)
        {
            if (child != null && !child.IsScared())
            {
                child.Lure(lureTarget);
            }
        }
    }

    // Get remaining children count
    public int GetRemainingChildren()
    {
        int remaining = 0;
        foreach (ChildAI child in children)
        {
            if (child != null && !child.IsScared())
                remaining++;
        }
        return remaining;
    }

    void OnDestroy()
    {
        // Clean up event subscriptions
        ChildAI.OnChildScared -= OnChildScaredHandler;
    }
}
