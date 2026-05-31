using UnityEngine;
using System.Collections.Generic;

public class RoomData : MonoBehaviour
{
    [Header("Room References")]
    public Transform[] wanderPoints;
    public Transform exitPoint;

    [Header("Children Setup")]
    public GameObject childPrefab;  // Assign your Child prefab here
    public int childCount = 3;
    public Transform[] spawnPositions;  // Optional specific spawn points

    [Header("Room Settings")]
    public string roomName;

    private List<ChildAI> childrenInRoom = new List<ChildAI>();

    void Start()
    {
        SpawnChildren();
    }

    void SpawnChildren()
    {
        // Destroy any existing children (if you placed them manually)
        ChildAI[] existingChildren = GetComponentsInChildren<ChildAI>();
        foreach (ChildAI child in existingChildren)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
        childrenInRoom.Clear();
        DoorBlocker[] allDoors = FindObjectsByType<DoorBlocker>(FindObjectsSortMode.None);
        // Spawn new children
        for (int i = 0; i < childCount; i++)
        {
            Vector3 spawnPos = GetSpawnPosition(i);
            GameObject newChild = Instantiate(childPrefab, spawnPos, Quaternion.identity, transform);
            ChildAI childAI = newChild.GetComponent<ChildAI>();
            InitializeChild(childAI);

            Collider2D childCollider = newChild.GetComponent<Collider2D>();
            foreach (DoorBlocker door in allDoors)
            {
                if (childCollider != null && door.blockingCollider != null)
                    Physics2D.IgnoreCollision(childCollider, door.blockingCollider);
            }
        }

    }

    Vector3 GetSpawnPosition(int index)
    {
        if (spawnPositions != null && index < spawnPositions.Length)
            return spawnPositions[index].position;

        // Random position within room bounds (if you have a collider)
        Collider2D roomBounds = GetComponent<Collider2D>();
        if (roomBounds != null)
        {
            Bounds bounds = roomBounds.bounds;
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                0
            );
        }

        // Fallback: near the room's position
        return transform.position + new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0);
    }

    public void InitializeChild(ChildAI child)
    {
        child.SetRoomReferences(wanderPoints, exitPoint);
        childrenInRoom.Add(child);
        Debug.Log($"Initialized {child.name} in room {roomName} with {wanderPoints.Length} wander points");
    }

    public List<ChildAI> GetChildrenInRoom()
    {
        childrenInRoom.RemoveAll(c => c == null);
        return childrenInRoom;
    }

    public List<ChildAI> GetScaredChildren()
    {
        List<ChildAI> scared = new List<ChildAI>();
        foreach (ChildAI child in GetChildrenInRoom())
        {
            if (child.IsScared())
                scared.Add(child);
        }
        return scared;
    }

    // Editor helper to preview spawn positions
    void OnDrawGizmosSelected()
    {
        if (wanderPoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform point in wanderPoints)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, 0.3f);
            }
        }

        if (exitPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(exitPoint.position, 0.5f);
        }

        if (spawnPositions != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform spawn in spawnPositions)
            {
                if (spawn != null)
                    Gizmos.DrawWireSphere(spawn.position, 0.4f);
            }
        }
    }
}