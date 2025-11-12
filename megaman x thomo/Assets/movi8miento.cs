using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
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
    private Animator ani;

    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isWallJumping;
    private bool isJumping;
    private bool isRunning;

    private float moveInput;
    private float lastNonZeroInput;   // guarda la última dirección válida
    private float idleBufferTimer;    // pequeño buffer para evitar micro-idle
    private int facingDirection = 1;

    [Header("Buffer de idle (segundos)")]
    public float idleBuffer = 0.1f;   // cuanto tiempo espera antes de poner idle

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
        wallJumpDirection.Normalize();
    }

    private void Update()
    {
        // --- entrada y detección ---
        moveInput = Input.GetAxis("Horizontal");
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);

        // --- dirección ---
        if (moveInput > 0.05f) facingDirection = 1;
        else if (moveInput < -0.05f) facingDirection = -1;

        // --- movimiento horizontal ---
        if (!isWallSliding && !isWallJumping)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }

        // --- salto normal ---
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = true;
            ani.SetTrigger("jumpTrigger");
        }

        if (rb.linearVelocity.y < -0.1f && !isGrounded)
            isJumping = false;

        // --- pared y salto en pared ---
        if (isTouchingWall && !isGrounded && Mathf.Sign(moveInput) == facingDirection)
            isWallSliding = true;
        else
            isWallSliding = false;

        if (isWallSliding)
            rb.linearVelocity = new Vector2(0, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));

        if (Input.GetKeyDown(KeyCode.Space) && isWallSliding)
        {
            isWallSliding = false;
            isWallJumping = true;
            isJumping = true;

            Vector2 force = new Vector2(-facingDirection * wallJumpDirection.x, wallJumpDirection.y) * wallJumpForce;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(force, ForceMode2D.Impulse);

            ani.SetTrigger("wallJumpTrigger");
            Invoke(nameof(StopWallJump), 0.25f);
        }

        // --- manejo del buffer para correr ---
        if (Mathf.Abs(moveInput) > 0.05f && isGrounded)
        {
            isRunning = true;
            idleBufferTimer = idleBuffer;         // resetea el tiempo de espera
            lastNonZeroInput = moveInput;         // guarda última dirección
        }
        else
        {
            if (idleBufferTimer > 0)
                idleBufferTimer -= Time.deltaTime; // sigue corriendo un poco
            else
                isRunning = false;                 // recién ahora pasa a idle
        }

        // --- animaciones ---
        ani.SetBool("isRunning", isRunning);
        ani.SetBool("isGrounded", isGrounded);
        ani.SetBool("isJumping", isJumping);
        ani.SetBool("isFalling", rb.linearVelocity.y < -0.1f && !isGrounded);
        ani.SetBool("isWallSliding", isWallSliding);
        ani.SetBool("isWallJumping", isWallJumping);

        // --- giro visual ---
        if (!isWallJumping)
        {
            Vector3 s = transform.localScale;
            if (lastNonZeroInput > 0.05f)
                transform.localScale = new Vector3(Mathf.Abs(s.x), s.y, s.z);
            else if (lastNonZeroInput < -0.05f)
                transform.localScale = new Vector3(-Mathf.Abs(s.x), s.y, s.z);
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
