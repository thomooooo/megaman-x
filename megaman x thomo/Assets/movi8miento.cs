using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MegaManX2Simple : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 8f;            // Velocidad máxima
    public float acceleration = 20f;        // Qué tan rápido acelera
    public float deceleration = 25f;        // Qué tan rápido se detiene
    private float velocityX;
    private float inputX;

    [Header("Salto")]
    public float jumpForce = 14f;
    private bool isGrounded;

    [Header("Suelo")]
    public Transform groundCheck;
    public float groundRadius = 0.1f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // --- INPUT ---
        inputX = 0f;
        if (Input.GetKey(KeyCode.A))
            inputX = -1f;
        else if (Input.GetKey(KeyCode.D))
            inputX = 1f;

        // --- SALTO ---
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // --- FLIP ---
        if (inputX != 0)
            sr.flipX = inputX < 0;
    }

    void FixedUpdate()
    {
        // --- MOVIMIENTO CON SUAVIDAD ---
        if (Mathf.Abs(inputX) > 0.1f)
            velocityX = Mathf.MoveTowards(velocityX, inputX * moveSpeed, acceleration * Time.fixedDeltaTime);
        else
            velocityX = Mathf.MoveTowards(velocityX, 0, deceleration * Time.fixedDeltaTime);

        rb.velocity = new Vector2(velocityX, rb.velocity.y);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}
