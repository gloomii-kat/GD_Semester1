using UnityEngine;
using UnityEngine.UI;

public class Teacher : MonoBehaviour
{
    private float dirX;
    private Rigidbody2D rb;
    private float moveSpeed = 1f;

    [SerializeField] private GameObject gotchaTxt;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gotchaTxt.SetActive(false);
        rb = GetComponent<Rigidbody2D>();
        dirX = -1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x < -8f)
        {
            dirX = 1f;
        }
        else if (transform.position.x > 8f)
        {
            dirX = -1f;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(dirX * moveSpeed, rb.linearVelocityY);
    }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                gotchaTxt.SetActive(true);
            }
        }
    
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                gotchaTxt.SetActive(false);
            }
    }
}
