using UnityEngine;
using UnityEngine.Events;

public class BeliefSystem : MonoBehaviour
{
    public static BeliefSystem Instance { get; private set; }

    [Header("Starting Values")]
    [Range(0f, 100f)] public float startingFear = 0f;
    [Range(0f, 100f)] public float startingAwareness = 0f;

    [Header("Decay — fear slowly fades if player is idle")]
    [Tooltip("Fear points lost per second when no scare is active")]
    public float fearDecayRate = 0.5f;
    [Tooltip("Awareness slowly drops when the player hides and is quiet")]
    public float awarenessDecayRate = 0.2f;

    [Header("Events")]
    public UnityEvent<float> onFearChanged;       // passes new Fear value
    public UnityEvent<float> onAwarenessChanged;  // passes new Awareness value

    // Runtime 
    public float Fear { get; private set; }
    public float Awareness { get; private set; }

    // Track whether a scare is currently active (suppresses fear decay)
    private bool scareActive;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        Fear = startingFear;
        Awareness = startingAwareness;
    }

    void Update()
    {
        if (!GameManager.Instance.NightActive) return;

        // Passive decay
        if (!scareActive)
            SetFear(Fear - fearDecayRate * Time.deltaTime);

        SetAwareness(Awareness - awarenessDecayRate * Time.deltaTime);
    }

    // Public API 

    /// <summary>
    /// Add fear points. Pass a negative value to reduce.
    /// multiplier lets scare combos (e.g. witnessed kill) stack.
    /// </summary>
    public void AddFear(float amount, float multiplier = 1f)
    {
        SetFear(Fear + amount * multiplier);
    }

    /// <summary>Add awareness points. Pass negative to reduce.</summary>
    public void AddAwareness(float amount)
    {
        SetAwareness(Awareness + amount);
    }

    /// <summary>Call when a scare animation starts so fear doesn't decay mid-scare.</summary>
    public void SetScareActive(bool active) => scareActive = active;

    //  Private 

    void SetFear(float value)
    {
        Fear = Mathf.Clamp(value, 0f, 100f);
        onFearChanged?.Invoke(Fear);

        if (Fear >= GameManager.Instance.fearWinThreshold)
            GameManager.Instance.TriggerWin();
    }

    void SetAwareness(float value)
    {
        Awareness = Mathf.Clamp(value, 0f, 100f);
        onAwarenessChanged?.Invoke(Awareness);

        if (Awareness >= GameManager.Instance.awarenessLoseThreshold)
            GameManager.Instance.TriggerLose();
    }
}