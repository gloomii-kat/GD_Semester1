using UnityEngine;

/// <summary>
/// ScareAbility.cs
/// ScriptableObject data container for a single scare ability.
/// Create via: Assets -> Create -> PinkyPinky -> ScareAbility
/// </summary>
[CreateAssetMenu(menuName = "PinkyPinky/ScareAbility", fileName = "NewScareAbility")]
public class ScareAbility : ScriptableObject
{
    [Header("Identity")]
    public string abilityName = "New Ability";
    [TextArea] public string description;

    [Header("Costs and Values")]
    [Tooltip("Fear points granted when this scare lands")]
    public float fearValue = 10f;
    [Tooltip("Awareness raised when this scare is used (risk)")]
    public float awarenessRisk = 5f;
    [Tooltip("Seconds before this ability can be used again")]
    public float cooldown = 5f;

    [Header("Visibility")]
    [Tooltip("Does using this ability manifest Pinky Pinky (make her visible)?")]
    public bool manifestsPlayer = false;
    [Tooltip("Radius in world units that children can see or hear this ability")]
    public float detectionRadius = 3f;

    [Header("Tier - used by progression system")]
    [Range(1, 3)] public int tier = 1;

    [Header("Optional VFX Prefab (spawned at player position on use)")]
    public GameObject vfxPrefab;
}
