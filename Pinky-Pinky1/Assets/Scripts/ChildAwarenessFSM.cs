using UnityEngine;

public class ChildAwarenessFSM : MonoBehaviour
{
    public enum ChildState
    {
        Calm,
        Confused,
        Agitated,
        Panicked
    }

    [Header("State")]
    public ChildState currentState = ChildState.Calm;

    [Header("Awareness")]
    public AwarenessScript awarenessScript;
    public float confusedThreshold = 1f;
    public float agitatedThreshold = 75f;
    public float panickedThreshold = 150f;

    [Header("References")]
    public ChildAI childAI;
    public GirlSounds girlSounds;

    [Header("Reaction Icons")]
    public GameObject questionMarkIcon;
    public GameObject agitatedIcon;
    public GameObject exclamationIcon;

    [Header("Movement Speeds")]
    public float calmSpeed = 200f;
    public float agitatedSpeed = 260f;
    public float panicSpeedMultiplier = 2f;

    [Header("Reaction Timing")]
    public float confusedPauseDuration = 0.8f;

    private bool confusedTriggered = false;
    private bool agitatedTriggered = false;
    private bool panickedTriggered = false;

    void Start()
    {
        HideAllIcons();

        if (childAI != null)
        {
            childAI.speed = calmSpeed;
        }
    }

    void Update()
    {
        if (awarenessScript == null || awarenessScript.slider == null) return;

        float awareness = awarenessScript.slider.value;

        if (!confusedTriggered && awareness >= confusedThreshold)
        {
            EnterConfusedState();
        }

        if (!agitatedTriggered && awareness >= agitatedThreshold)
        {
            EnterAgitatedState();
        }

        if (!panickedTriggered && awareness >= panickedThreshold)
        {
            EnterPanickedState();
        }
    }

    void EnterConfusedState()
    {
        currentState = ChildState.Confused;
        confusedTriggered = true;

        HideAllIcons();
        if (questionMarkIcon != null)
            questionMarkIcon.SetActive(true);

        if (girlSounds != null)
            girlSounds.PlayDeepBreath();

        if (childAI != null)
            childAI.TriggerConfusedPause(confusedPauseDuration);

        Debug.Log("Child entered CONFUSED state");
    }

    void EnterAgitatedState()
    {
        currentState = ChildState.Agitated;
        agitatedTriggered = true;

        HideAllIcons();
        if (agitatedIcon != null)
            agitatedIcon.SetActive(true);

        if (girlSounds != null)
            girlSounds.PlayMultipleBreath();

        if (childAI != null)
            childAI.speed = agitatedSpeed;

        Debug.Log("Child entered AGITATED state");
    }

    void EnterPanickedState()
    {
        currentState = ChildState.Panicked;
        panickedTriggered = true;

        HideAllIcons();
        if (exclamationIcon != null)
            exclamationIcon.SetActive(true);

        if (girlSounds != null)
            girlSounds.PlayScream();

        if (childAI != null)
        {
            childAI.speed = agitatedSpeed;
            childAI.TriggerEscape(panicSpeedMultiplier);
        }

        Debug.Log("Child entered PANICKED state");
    }

    void HideAllIcons()
    {
        if (questionMarkIcon != null) questionMarkIcon.SetActive(false);
        if (agitatedIcon != null) agitatedIcon.SetActive(false);
        if (exclamationIcon != null) exclamationIcon.SetActive(false);
    }
}
