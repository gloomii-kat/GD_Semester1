using UnityEngine;
using System.Collections.Generic;

public class WaypointFlipper : MonoBehaviour
{
    [Header("Flip Points")]
    public Transform[] flipPoints; // Assign points where teacher should flip

    [Header("Settings")]
    public float arrivalDistance = 0.5f; // How close to consider "arrived"
    public bool flipOnlyOnce = true; // Flip only the first time reaching each point
    public bool debugMode = true;

    private Dictionary<Transform, bool> pointsFlipped = new Dictionary<Transform, bool>();
    private Transform teacherTransform;
    private TeacherChase teacherChase;
    private TeacherPatrol teacherPatrol;
    private Transform currentTarget;

    void Start()
    {
        teacherTransform = transform;
        teacherChase = GetComponent<TeacherChase>();
        teacherPatrol = GetComponent<TeacherPatrol>();

        // Initialize flip tracking
        if (flipPoints != null)
        {
            foreach (Transform point in flipPoints)
            {
                if (point != null)
                    pointsFlipped[point] = false;
            }
        }

        if (teacherPatrol != null)
        {
            // Subscribe to patrol target changes if possible
            // This assumes TeacherPatrol has an event for target changes
            // If not, we'll check in Update
        }
    }

    void Update()
    {
        // Get current target from patrol or chase
        Transform target = GetCurrentTarget();

        if (target != null && currentTarget != target)
        {
            currentTarget = target;
        }

        // Check if we've reached any flip point
        CheckFlipPoints();
    }

    Transform GetCurrentTarget()
    {
        // If teacher is chasing, check chase target
        if (teacherChase != null && teacherChase.IsChasing)
        {
            return teacherChase.playerTarget;
        }

        // If patrolling, check patrol target
        if (teacherPatrol != null)
        {
            // This assumes TeacherPatrol has a public waypoint property
            // You might need to add this to TeacherPatrol
            return teacherPatrol.GetCurrentWaypoint();
        }

        return null;
    }

    void CheckFlipPoints()
    {
        if (flipPoints == null) return;

        foreach (Transform point in flipPoints)
        {
            if (point == null) continue;

            // Skip if already flipped and flipOnlyOnce is true
            if (flipOnlyOnce && pointsFlipped.ContainsKey(point) && pointsFlipped[point])
                continue;

            // Check distance to flip point
            float distance = Vector2.Distance(teacherTransform.position, point.position);

            if (distance <= arrivalDistance)
            {
                FlipTowardsPoint(point);

                if (flipOnlyOnce)
                    pointsFlipped[point] = true;

                if (debugMode)
                    Debug.Log($"Teacher flipped at point: {point.name}");
            }
        }
    }

    void FlipTowardsPoint(Transform point)
    {
        if (point == null) return;

        // Determine which way to flip based on point position relative to teacher
        float direction = Mathf.Sign(point.position.x - teacherTransform.position.x);

        if (direction != 0)
        {
            Flip(direction > 0); // true = face right, false = face left
        }
    }

    void Flip(bool faceRight)
    {
        Vector3 localScale = teacherTransform.localScale;

        if (faceRight)
        {
            // Face right (positive X scale)
            localScale.x = Mathf.Abs(localScale.x);
        }
        else
        {
            // Face left (negative X scale)
            localScale.x = -Mathf.Abs(localScale.x);
        }

        teacherTransform.localScale = localScale;

        if (debugMode)
            Debug.Log($"Teacher flipped to face {(faceRight ? "RIGHT" : "LEFT")}");
    }

    // Public method to manually flip
    public void FlipNow(bool faceRight)
    {
        Flip(faceRight);
    }

    // Reset all flip points (useful for scene restarts)
    public void ResetFlips()
    {
        foreach (Transform point in flipPoints)
        {
            if (point != null)
                pointsFlipped[point] = false;
        }
    }

    // Draw gizmos for visualization
    void OnDrawGizmosSelected()
    {
        if (flipPoints == null) return;

        Gizmos.color = Color.cyan;
        foreach (Transform point in flipPoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, arrivalDistance);

                // Draw line from teacher to point for direction
                if (Application.isPlaying && teacherTransform != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(teacherTransform.position, point.position);
                    Gizmos.color = Color.cyan;
                }
            }
        }
    }
}
