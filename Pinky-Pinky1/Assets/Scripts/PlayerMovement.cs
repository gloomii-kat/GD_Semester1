using UnityEngine;
using UnityEngine.UI;


public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public KeyCode sprintKey = KeyCode.LeftShift;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float currentSpeed;

    public float stamina = 100f;
    public float staminaDrain = 25f;
    public float staminaRegen = 20f;

    [HideInInspector] public bool canMove = true; // Can be accessed by other scripts

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        // In sprint logic:
        if (Input.GetKey(sprintKey) && stamina > 0)
        {
            currentSpeed = sprintSpeed;
            stamina -= staminaDrain * Time.deltaTime;
        }
        else
        {
            currentSpeed = moveSpeed;
            stamina += staminaRegen * Time.deltaTime;
        }
        stamina = Mathf.Clamp(stamina, 0f, 100f);
    }

    void FixedUpdate()
    {
        if (canMove)
            rb.linearVelocity = moveInput * currentSpeed;
        else
            rb.linearVelocity = Vector2.zero;
    }
}