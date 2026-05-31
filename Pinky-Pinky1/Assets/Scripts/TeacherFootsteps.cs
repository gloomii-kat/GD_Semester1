using UnityEngine;

/// <summary>
/// Attach to the Teacher GameObject.
/// Plays footstep sounds that get louder and faster as the teacher approaches the player.
/// </summary>
public class TeacherFootsteps : MonoBehaviour
{
    [Header("References")]
    public Transform playerTarget;
    public AudioSource footstepAudioSource;

    [Header("Footstep Sounds")]
    [Tooltip("Add 2-3 slightly different footstep clips to avoid repetition")]
    public AudioClip[] footstepClips;

    [Header("Distance Settings")]
    public float maxHearingDistance = 15f;   // Beyond this, player hears nothing
    public float minDistance = 1.5f;         // At this distance, volume is max

    [Header("Volume")]
    public float maxVolume = 1f;
    public float minVolume = 0f;

    [Header("Step Timing")]
    [Tooltip("Seconds between steps at max distance (slow, quiet)")]
    public float slowStepInterval = 0.8f;
    [Tooltip("Seconds between steps at min distance (fast, loud)")]
    public float fastStepInterval = 0.3f;

    private float stepTimer = 0f;
    private int lastClipIndex = -1;
    private TeacherChase chaseScript;

    void Awake()
    {
        chaseScript = GetComponent<TeacherChase>();

        if (footstepAudioSource == null)
        {
            footstepAudioSource = gameObject.AddComponent<AudioSource>();
            footstepAudioSource.spatialBlend = 0f; // 2D sound — volume controlled by script
            footstepAudioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        if (playerTarget == null || footstepClips == null || footstepClips.Length == 0) return;

        float distance = Vector2.Distance(transform.position, playerTarget.position);

        // Only play if teacher is moving (patrolling or chasing)
        bool teacherIsMoving = chaseScript == null || chaseScript.IsChasing ||
                               IsPatrolling();

        if (!teacherIsMoving || distance > maxHearingDistance)
        {
            stepTimer = 0f;
            return;
        }

        // 0 = far away, 1 = right on top of player
        float proximity = 1f - Mathf.Clamp01(
            (distance - minDistance) / (maxHearingDistance - minDistance)
        );

        // Volume and interval both scale with proximity
        float volume = Mathf.Lerp(minVolume, maxVolume, proximity);
        float interval = Mathf.Lerp(slowStepInterval, fastStepInterval, proximity);

        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0f)
        {
            PlayFootstep(volume);
            stepTimer = interval;
        }
    }

    void PlayFootstep(float volume)
    {
        if (footstepClips.Length == 1)
        {
            footstepAudioSource.PlayOneShot(footstepClips[0], volume);
            return;
        }

        // Pick a clip that isn't the same as last time
        int index;
        do { index = Random.Range(0, footstepClips.Length); }
        while (index == lastClipIndex);

        lastClipIndex = index;
        footstepAudioSource.PlayOneShot(footstepClips[index], volume);
    }

    bool IsPatrolling()
    {
        // Teacher is moving if its rigidbody has velocity
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        return rb != null && rb.linearVelocity.magnitude > 0.1f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, maxHearingDistance);

        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, minDistance);
    }
}
