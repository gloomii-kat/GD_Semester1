using UnityEngine;


/*[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    [Tooltip("Speed multiplier when player is 'manifesting' (visible form)")]
    public float manifestedSpeedPenalty = 0.5f;

    [Header("Visibility")]
    [Tooltip("Is Pinky Pinky currently in visible / manifested form?")]
    public bool IsManifested { get; private set; } = false;

    // -- Internal refs ----------------------------------------------
    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private ScareAbilityManager abilityManager;

    // Animator parameter hashes (avoids string lookups every frame)
    private static readonly int HashMoveX = Animator.StringToHash("MoveX");
    private static readonly int HashMoveY = Animator.StringToHash("MoveY");
    private static readonly int HashMoving = Animator.StringToHash("IsMoving");
    private static readonly int HashManifest = Animator.StringToHash("IsManifested");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        abilityManager = GetComponent<ScareAbilityManager>();
    }

    void Update()
    {
        if (!GameManager.Instance.NightActive) return;

        HandleMovementInput();
        HandleAbilityInput();
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.NightActive) return;

        float speed = moveSpeed * (IsManifested ? manifestedSpeedPenalty : 1f);
        rb.linearVelocity = moveInput * speed;
    }

    // -- Input handlers ---------------------------------------------

    void HandleMovementInput()
    {
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        // Drive animations
        if (moveInput != Vector2.zero)
        {
            anim.SetFloat(HashMoveX, moveInput.x);
            anim.SetFloat(HashMoveY, moveInput.y);
        }
        anim.SetBool(HashMoving, moveInput != Vector2.zero);
    }

    void HandleAbilityInput()
    {
        if (abilityManager == null) return;

        // Ability slot 1 -- e.g. Flicker Lights
        if (Input.GetKeyDown(KeyCode.Alpha1))
            abilityManager.UseAbility(0);

        // Ability slot 2 -- e.g. Whisper / Shadow
        if (Input.GetKeyDown(KeyCode.Alpha2))
            abilityManager.UseAbility(1);

        // Ability slot 3 -- Full Apparition (Tier 3, unlocked later)
        if (Input.GetKeyDown(KeyCode.Alpha3))
            abilityManager.UseAbility(2);
    }

    // -- Public helpers ---------------------------------------------

    /// <summary>
    /// Toggle manifested (visible) state.
    /// Called by abilities that make Pinky Pinky physically appear.
    /// </summary>
    public void SetManifested(bool manifested)
    {
        IsManifested = manifested;
        anim.SetBool(HashManifest, manifested);
    }
}*/

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;

    [Tooltip("Speed multiplier when player is manifested (visible form)")]
    public float manifestedSpeedPenalty = 0.5f;

    public bool IsManifested { get; private set; } = false;

    // -- Internal refs ---------------------------------------------
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private ScareAbilityManager abilityManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        abilityManager = GetComponent<ScareAbilityManager>();
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.NightActive) return;

        HandleMovementInput();
        HandleAbilityInput();
    }

    void FixedUpdate()
    {
        if (GameManager.Instance == null || !GameManager.Instance.NightActive) return;

        float speed = moveSpeed * (IsManifested ? manifestedSpeedPenalty : 1f);
        rb.linearVelocity = moveInput * speed;
    }

    // -- Input handlers --------------------------------------------

    void HandleMovementInput()
    {
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    void HandleAbilityInput()
    {
        if (abilityManager == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            abilityManager.UseAbility(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            abilityManager.UseAbility(1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            abilityManager.UseAbility(2);
    }

    // -- Public helpers --------------------------------------------

    public void SetManifested(bool manifested)
    {
        IsManifested = manifested;
    }
}
