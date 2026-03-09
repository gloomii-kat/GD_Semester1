using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScareAbilityManager.cs
/// Manages the player's equipped scare abilities.
/// Handles cooldown tracking and delegates to BeliefSystem.
/// Attach to the same GameObject as PlayerController.
///
/// NOTE: ScareAbility ScriptableObject is in its own file: ScareAbility.cs
/// Make sure both files are in your project Assets/Scripts folder.
/// </summary>
public class ScareAbilityManager : MonoBehaviour
{
    [Header("Equipped Abilities (drag ScareAbility assets here)")]
    public List<ScareAbility> equippedAbilities = new List<ScareAbility>();

    // Cooldown tracker: index -> remaining seconds
    private float[] cooldownTimers;

    private PlayerController playerController;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        cooldownTimers = new float[3];
    }

    void Update()
    {
        for (int i = 0; i < cooldownTimers.Length; i++)
        {
            if (cooldownTimers[i] > 0f)
                cooldownTimers[i] -= Time.deltaTime;
        }
    }

    // -- Public API ------------------------------------------------

    /// <summary>Attempt to use the ability in the given slot (0-indexed).</summary>
    public void UseAbility(int slotIndex)
    {
        if (!GameManager.Instance.NightActive) return;
        if (slotIndex >= equippedAbilities.Count) return;

        ScareAbility ability = equippedAbilities[slotIndex];
        if (ability == null) return;

        if (cooldownTimers[slotIndex] > 0f)
        {
            Debug.Log("[ScareAbilityManager] " + ability.abilityName + " on cooldown: " + cooldownTimers[slotIndex].ToString("F1") + "s");
            return;
        }

        ExecuteAbility(ability, slotIndex);
    }

    public bool IsOnCooldown(int slotIndex)
    {
        if (slotIndex >= cooldownTimers.Length) return true;
        return cooldownTimers[slotIndex] > 0f;
    }

    public float GetCooldownRemaining(int slotIndex)
    {
        if (slotIndex >= cooldownTimers.Length) return 0f;
        return Mathf.Max(0f, cooldownTimers[slotIndex]);
    }

    // -- Private ---------------------------------------------------

    void ExecuteAbility(ScareAbility ability, int slotIndex)
    {
        BeliefSystem.Instance.AddFear(ability.fearValue);
        BeliefSystem.Instance.AddAwareness(ability.awarenessRisk);

        if (ability.manifestsPlayer && playerController != null)
        {
            playerController.SetManifested(true);
            StartCoroutine(UnmanifestAfterDelay(ability.cooldown * 0.4f));
        }

        if (ability.vfxPrefab != null)
            Instantiate(ability.vfxPrefab, transform.position, Quaternion.identity);

        //NotifyNearbyChildren(ability);

        cooldownTimers[slotIndex] = ability.cooldown;

        BeliefSystem.Instance.SetScareActive(true);
        StartCoroutine(EndScareActive(1.5f));

        Debug.Log("[ScareAbilityManager] Used: " + ability.abilityName + " | +" + ability.fearValue + " Fear | +" + ability.awarenessRisk + " Awareness");
    }

    /*void NotifyNearbyChildren(ScareAbility ability)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, ability.detectionRadius);
        foreach (var hit in hits)
        {
            ChildAI child = hit.GetComponent<ChildAI>();
            if (child != null)
                child.OnScared(ability.fearValue, ability.manifestsPlayer);
        }
    }*/

    IEnumerator UnmanifestAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (playerController != null)
            playerController.SetManifested(false);
    }

    IEnumerator EndScareActive(float delay)
    {
        yield return new WaitForSeconds(delay);
        BeliefSystem.Instance.SetScareActive(false);
    }
}