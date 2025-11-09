using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MegaManXWallSlide : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 8f;
    public float jumpForce = 14f;

    [Header("Paredes")]
    public float wallSlideSpeed = 2f;
    public float wallJumpForce = 14f;
    public Vector2 wallJumpDirection = new Vector2(1, 1.2f);
    public Transform wallCheck;
    public float wallCheckRadius = 0.2f;
    public LayerMask wallLayer;

    [Header("Suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isWallJumping;
    private float moveInput;
    private int facingDirection = 1; // 1 = derecha, -1 = izquierda

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        wallJumpDirection.Normalize();
    }

    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);

        // --- Dirección actual del personaje ---
        if (moveInput > 0) facingDirection = 1;
        else if (moveInput < 0) facingDirection = -1;

        // --- Movimiento normal ---
        if (!isWallSliding && !isWallJumping)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }

        // --- Salto normal ---
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // --- Detección de deslizamiento por pared ---
        if (isTouchingWall && !isGrounded && moveInput == facingDirection)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }

        // 💡 NUEVO: si presionás en dirección contraria, se despega al instante
        if (isWallSliding && moveInput == -facingDirection)
        {
            isWallSliding = false;
        }

        // --- Deslizamiento ---
        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(0, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
        }

        // --- Salto desde la pared ---
        if (Input.GetKeyDown(KeyCode.Space) && isWallSliding)
        {
            isWallSliding = false;
            isWallJumping = true;

            Vector2 force = new Vector2(-facingDirection * wallJumpDirection.x, wallJumpDirection.y) * wallJumpForce;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(force, ForceMode2D.Impulse);

            Invoke(nameof(StopWallJump), 0.2f);
        }
    }

    private void StopWallJump()
    {
        isWallJumping = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (wallCheck != null)
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);

        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
