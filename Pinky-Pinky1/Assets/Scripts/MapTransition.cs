using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class MapTransition : MonoBehaviour
{
    [SerializeField] PolygonCollider2D mapBoundary;
    CinemachineConfiner2D confiner;
    [SerializeField] Direction transitionDirection;

    enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    private void Awake()
    {
        confiner = FindFirstObjectByType<CinemachineConfiner2D>();
        if (confiner == null)
        {
            Debug.LogError("CinemachineConfiner2D not found in the scene.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(UpdateCameraAndPlayer(collision.gameObject));
        }
    }

    private IEnumerator UpdateCameraAndPlayer(GameObject player)
    {
        // Update the camera bounds first
        confiner.BoundingShape2D = mapBoundary;

        // Force the camera to invalidate its cache and recalculate bounds
        confiner.InvalidateBoundingShapeCache();

        // Wait one frame to allow the camera to update
        yield return null;

        // Now teleport the player
        UpdatePlayerPosition(player);

        // Wait another frame to ensure camera catches up
        yield return null;

        // Force camera to reevaluate bounds again after player moved
        confiner.InvalidateBoundingShapeCache();

        Debug.Log("Player entered new map area. Camera confiner updated.");
    }

    private void UpdatePlayerPosition(GameObject player)
    {
        Vector3 newPosition = player.transform.position;

        switch (transitionDirection)
        {
            case Direction.Up:
                newPosition.y += 3f;
                break;
            case Direction.Down:
                newPosition.y -= 3f;
                break;
            case Direction.Left:
                newPosition.x -= 2f;
                break;
            case Direction.Right:
                newPosition.x += 2f;
                break;
        }
        player.transform.position = newPosition;
    }
}

