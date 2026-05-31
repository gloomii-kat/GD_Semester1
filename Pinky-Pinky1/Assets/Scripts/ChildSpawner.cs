using UnityEngine;
using System.Collections.Generic;

public class ChildSpawner : MonoBehaviour
{

    //  Inspector
 
    [Header("Spawning")]
    public GameObject childPrefab;
    public Transform[] spawnPoints;            // All possible spawn points in the room
    public int numberOfChildren = 3;   // How many to spawn

    [Header("Settings")]
    public float minSpawnDistance = 1.5f; // Minimum distance between spawned children
    public Transform pinky;                   // Passed to each ChildAI
    public Transform exitPoint;               // Passed to each ChildAI
    public Transform[] wanderPoints;            // Passed to each ChildAI

    //  Internal
 
    private List<GameObject> spawnedChildren = new List<GameObject>();

    //  Unity
   
    void Start()
    {
        SpawnChildren();
    }

    //  Spawning
    
    void SpawnChildren()
    {
        if (childPrefab == null)
        {
            Debug.LogError("ChildSpawner: No prefab assigned!");
            return;
        }

        // Shuffle spawn points so we don't always use the same ones
        List<Transform> shuffled = ShufflePoints(spawnPoints);
        List<Vector2> usedPositions = new List<Vector2>();
        int spawned = 0;

        foreach (Transform point in shuffled)
        {
            if (spawned >= numberOfChildren) break;

            // Check this point isn't too close to an already-used point
            if (IsTooClose(point.position, usedPositions))
            {
                Debug.Log($"ChildSpawner: Skipping {point.name} - too close to another spawn.");
                continue;
            }

            // Spawn the child
            GameObject child = Instantiate(childPrefab, point.position, Quaternion.identity);
            child.name = $"Child_{spawned + 1}";

            // Wire up references
            ChildAI ai = child.GetComponent<ChildAI>();
            if (ai != null)
            {
                ai.pinky = pinky;
                ai.exitPoint = exitPoint;
                ai.wanderPoints = wanderPoints;
            }
            else
            {
                Debug.LogWarning($"ChildSpawner: Prefab has no ChildAI component!");
            }

            usedPositions.Add(point.position);
            spawnedChildren.Add(child);
            spawned++;
        }

        if (spawned < numberOfChildren)
            Debug.LogWarning($"ChildSpawner: Only spawned {spawned}/{numberOfChildren} children. " +
                             $"Not enough valid spawn points with minSpawnDistance {minSpawnDistance}.");
    }

    
    //  Helpers
 
    bool IsTooClose(Vector2 candidate, List<Vector2> used)
    {
        foreach (Vector2 pos in used)
        {
            if (Vector2.Distance(candidate, pos) < minSpawnDistance)
                return true;
        }
        return false;
    }

    List<Transform> ShufflePoints(Transform[] points)
    {
        List<Transform> list = new List<Transform>(points);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Transform temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
        return list;
    }

    // 
    //  Public — RoomManager can call this
    // 
    public List<GameObject> GetSpawnedChildren() => spawnedChildren;

    void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;
        Gizmos.color = Color.cyan;
        foreach (Transform p in spawnPoints)
        {
            if (p != null)
            {
                Gizmos.DrawWireSphere(p.position, minSpawnDistance * 0.5f);
                Gizmos.DrawIcon(p.position, "sv_icon_dot0_pix16_gizmo", true);
            }
        }
    }
}
