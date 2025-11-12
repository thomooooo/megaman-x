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

    [Header("Dash")]
    public float dashSpeed = 20f;           // velocidad del dash
    public float dashDuration = 0.2f;       // duración total del dash
    public float dashCooldown = 0.5f;       // tiempo antes de poder volver a hacer dash
    public KeyCode dashKey = KeyCode.LeftShift;

    private bool isDashing = false;
    private bool canDash = true;
    private float dashTimer;

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
        // Detecta paredes a izquierda y derecha por separado
        bool wallOnRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckRadius + 0.1f, wallLayer);
        bool wallOnLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckRadius + 0.1f, wallLayer);


        // --- dirección ---
        if (moveInput > 0.05f) facingDirection = 1;
        else if (moveInput < -0.05f) facingDirection = -1;

        // --- movimiento horizontal ---
        if (!isWallSliding && !isWallJumping && !isDashing)
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

        // --- DASH ---
        if (Input.GetKeyDown(dashKey) && canDash && !isWallSliding)
        {
            StartDash();
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                EndDash();
            }
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

        // --- giro visual mejorado ---
        Vector3 s = transform.localScale;

        // Si está en wall slide, mirar hacia la pared
        if (isWallSliding)
        {
            if (wallOnRight)
            {
                facingDirection = 1;
                transform.localScale = new Vector3(Mathf.Abs(s.x), s.y, s.z);
            }
            else if (wallOnLeft)
            {
                facingDirection = -1;
                transform.localScale = new Vector3(-Mathf.Abs(s.x), s.y, s.z);
            }
        }
        // Si no está en wall slide, girar según el input
        else if (!isWallJumping)
        {
            if (moveInput > 0.05f)
            {
                facingDirection = 1;
                transform.localScale = new Vector3(Mathf.Abs(s.x), s.y, s.z);
                lastNonZeroInput = 1;
            }
            else if (moveInput < -0.05f)
            {
                facingDirection = -1;
                transform.localScale = new Vector3(-Mathf.Abs(s.x), s.y, s.z);
                lastNonZeroInput = -1;
            }
        }

    }


    private void StopWallJump()
    {
        isWallJumping = false;
    }

    private void StartDash()
    {
        isDashing = true;
        canDash = false;
        dashTimer = dashDuration;

        // cancela la velocidad vertical para que no frene el dash
        rb.linearVelocity = new Vector2(facingDirection * dashSpeed, 0f);

        // activa animación del dash (debes crear el trigger en el Animator)
        ani.SetTrigger("dashTrigger");

        // puede opcionalmente desactivar la gravedad mientras dura
        rb.gravityScale = 0;

        // termina el dash automáticamente luego del tiempo indicado
        Invoke(nameof(EndDash), dashDuration);
        Invoke(nameof(ResetDash), dashCooldown);
    }

    private void EndDash()
    {
        if (!isDashing) return;
        isDashing = false;
        rb.gravityScale = 1;
    }

    private void ResetDash()
    {
        canDash = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (wallCheck != null)
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        // Raycasts para pared izquierda y derecha
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * (wallCheckRadius + 0.1f));
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * (wallCheckRadius + 0.1f));
    }
}
