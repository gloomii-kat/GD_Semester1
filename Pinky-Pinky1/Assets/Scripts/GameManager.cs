using UnityEngine;
using UnityEngine.Events;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Night Cycle Settings")]
    [Tooltip("How long one night lasts in seconds (default 240 = 4 minutes)")]
    public float nightDuration = 240f;

    [Header("Win / Lose Thresholds")]
    [Tooltip("Fear value needed to win the night")]
    [Range(0f, 100f)] public float fearWinThreshold = 80f;
    [Tooltip("Awareness value that causes the player to lose")]
    [Range(0f, 100f)] public float awarenessLoseThreshold = 100f;

    [Header("Events — wire these up in the Inspector")]
    public UnityEvent onNightBegin;
    public UnityEvent onPlayerWin;
    public UnityEvent onPlayerLose;

    // ?? Runtime state ??????????????????????????????????????????????
    public float NightTimeRemaining { get; private set; }
    public bool NightActive { get; private set; }

    void Awake()
    {
        // Singleton pattern — only one GameManager may exist
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() => BeginNight();

    void Update()
    {
        if (!NightActive) return;

        NightTimeRemaining -= Time.deltaTime;

        // Time ran out without winning — check current fear level
        if (NightTimeRemaining <= 0f)
        {
            NightTimeRemaining = 0f;
            EvaluateEndOfNight();
        }
    }

    // ?? Public API ?????????????????????????????????????????????????

    public void BeginNight()
    {
        NightTimeRemaining = nightDuration;
        NightActive = true;
        onNightBegin?.Invoke();
        Debug.Log("[GameManager] Night has begun.");
    }

    /// <summary>Called by BeliefSystem when fear crosses the win threshold.</summary>
    public void TriggerWin()
    {
        if (!NightActive) return;
        NightActive = false;
        onPlayerWin?.Invoke();
        Debug.Log("[GameManager] WIN — fear threshold reached!");
    }

    /// <summary>Called by AwarenessSystem when awareness crosses the lose threshold.</summary>
    public void TriggerLose()
    {
        if (!NightActive) return;
        NightActive = false;
        onPlayerLose?.Invoke();
        Debug.Log("[GameManager] LOSE — too many adults alerted!");
    }

    // ?? Private helpers

    void EvaluateEndOfNight()
    {
        // If time is up and fear hasn't hit the win threshold ? lose
        if (BeliefSystem.Instance != null && BeliefSystem.Instance.Fear >= fearWinThreshold)
            TriggerWin();
        else
            TriggerLose();
    }
}