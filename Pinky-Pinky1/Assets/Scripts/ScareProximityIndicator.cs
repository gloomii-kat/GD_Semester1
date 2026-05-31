using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to each scare trigger object (toilet, light switch, basin etc).
/// It finds nearby children and shows a marker above them.
/// </summary>
public class ScareProximityIndicator : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRadius = 2f;
    public string childTag = "LittleGirl";

    [Header("Indicator Prefab")]
    [Tooltip("A World Space Canvas with an Image/icon on it. Assign your prefab here.")]
    public GameObject indicatorPrefab;

    [Header("Indicator Settings")]
    public Vector3 indicatorOffset = new Vector3(0f, 1.2f, 0f);
    public Color nearColour = new Color(1f, 0.9f, 0f, 1f);   // Yellow
    public Color closeColour = new Color(1f, 0.3f, 0f, 1f);  // Orange-red when very close
    public float closeThreshold = 1f;                          // Distance for colour change

    // Track which children currently have indicators
    private Dictionary<GameObject, GameObject> activeIndicators
        = new Dictionary<GameObject, GameObject>();

    void Update()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        // Track which children are in range this frame
        HashSet<GameObject> inRangeThisFrame = new HashSet<GameObject>();

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag(childTag)) continue;

            GameObject child = hit.gameObject;

            // Skip already-scared children
            ChildAI ai = child.GetComponent<ChildAI>();
            if (ai != null && ai.IsScared()) continue;

            inRangeThisFrame.Add(child);

            // Show indicator if not already shown
            if (!activeIndicators.ContainsKey(child))
                ShowIndicator(child);

            // Update colour based on distance
            UpdateIndicatorColour(child);
        }

        // Hide indicators for children that left range
        List<GameObject> toRemove = new List<GameObject>();
        foreach (var kvp in activeIndicators)
        {
            if (!inRangeThisFrame.Contains(kvp.Key))
            {
                Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var key in toRemove)
            activeIndicators.Remove(key);
    }

    void ShowIndicator(GameObject child)
    {
        if (indicatorPrefab == null) return;

        GameObject indicator = Instantiate(indicatorPrefab, child.transform);
        indicator.transform.localPosition = indicatorOffset;
        activeIndicators[child] = indicator;
    }

    void UpdateIndicatorColour(GameObject child)
    {
        if (!activeIndicators.ContainsKey(child)) return;

        float dist = Vector2.Distance(transform.position, child.transform.position);
        Color targetColour = dist <= closeThreshold ? closeColour : nearColour;

        // Apply to all UnityEngine.UI.Image components on the indicator
        UnityEngine.UI.Image[] images = activeIndicators[child]
            .GetComponentsInChildren<UnityEngine.UI.Image>();
        foreach (var img in images)
            img.color = targetColour;

        // Also apply to SpriteRenderer if you're using a sprite instead of UI
        SpriteRenderer[] sprites = activeIndicators[child]
            .GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in sprites)
            sr.color = targetColour;
    }

    void OnDisable()
    {
        // Clean up all indicators when trigger object is disabled
        foreach (var kvp in activeIndicators)
            if (kvp.Value != null) Destroy(kvp.Value);
        activeIndicators.Clear();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, closeThreshold);
    }
}
