using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animator animator; // R�f�rence � l'Animator
    [Header("Sprites")]
    [SerializeField] private SpriteRenderer spriteRenderer; // R�f�rence au SpriteRenderer
    [SerializeField] private Sprite normalSprite; // Sprite normal
    [SerializeField] private Sprite slideWallSprite; // Sprite pour le wall slide

    [Header("Movement Setting")]
    [SerializeField] private float groundSpeed = 8f;
    [SerializeField] private float airSpeed = 5f;
    [SerializeField] private float sprintSpeedMultiplier = 1.5f; // Multiplicateur de vitesse pour le sprint
    [SerializeField] private float jumpForce = 16f; // Force du saut
  
    [SerializeField] private float airDeceleration = 5f; // D�c�l�ration en l'air
    [SerializeField] private float acceleration = 10f; // Acc�l�ration
    [SerializeField] private float wallSlidingSpeed = 2f; // Vitesse de glissement sur le mur
    [SerializeField] private Vector2 wallJumpingPower = new Vector2(8f, 16f); // Puissance du saut de mur

    [Header("Gravity Settings")]
    [SerializeField] public float defaultGravityScale = 1f;
    [SerializeField] private float fallAcceleration = 20f; // Acc�l�ration de la chute
    [SerializeField] private float maxFallSpeed = 40f; // Vitesse de chute maximale
    [SerializeField] private float jumpEndEarlyGravityModifier = 2f; // Modificateur de gravit� lorsque le saut est termin� pr�matur�ment

    [Header("Assists")]
    [SerializeField] private float coyoteTime = 0.2f; // P�riode de gr�ce apr�s avoir quitt� le sol
    [SerializeField] private float jumpBufferTime = 0.2f; // P�riode de buffer pour les inputs de saut

    public Rigidbody2D rb;

    [Header("GroundCheck")]
    [SerializeField] private Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.49f, 0.03f);

    [Header("WallCheck")]
    [SerializeField] private Transform wallCheck;
    public LayerMask wallLayer; // Masque de couche pour d�tecter les murs
    public Vector2 wallCheckSize = new Vector2(0.03f, 0.49f);

    private Vector2 movementInput;
    private Vector2 frameVelocity;
    private bool isJumping = false;
    private bool isGrounded = true; // Assurez-vous de mettre � jour cette variable en fonction de votre d�tection de sol
    private bool isTouchingWall = false; // Indique si le joueur touche un mur
    private bool isSprinting = false;
    private bool endedJumpEarly = false;
    private Transform playerTransform;

    private float lastGroundedTime; // Temps �coul� depuis que le joueur a quitt� le sol
    private float lastJumpTime; // Temps �coul� depuis que le joueur a appuy� sur le bouton de saut
    private bool gravityInverted = false; // �tat de la gravit�
    private bool isWallSliding = false;
    private bool isWallJumping = false;
    private float wallJumpingDirection;
    private float wallJumpingCounter;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingDuration = 0.4f;
    private GrapplingHook grapplingHook;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerTransform = transform; // Assurez-vous que le script est attach� au joueur
        grapplingHook = GetComponent<GrapplingHook>();
    }

    private void Update()
    {
        HandleDirection();
        CheckWallContact();

        if (movementInput.x != 0)
        {
            FlipSprite(movementInput.x);
            animator.SetBool("isRunning", true); // Activer l'animation de course
        }
        else
        {
            animator.SetBool("isRunning", false); // D�sactiver l'animation de course
        }

        // Mettre � jour l'�tat de saut dans l'Animator
        animator.SetBool("isJumping", !isGrounded);

        // D�cr�menter les timers de coyote time et de jump buffer
        lastGroundedTime -= Time.deltaTime;
        lastJumpTime -= Time.deltaTime;

        if (isWallSliding)
        {
            spriteRenderer.sprite = slideWallSprite;
        }
        else
        {
            spriteRenderer.sprite = normalSprite;
        }

        // V�rifier les conditions pour sauter
        if (CanJump() && lastJumpTime > 0)
        {
            Jump();
        }

        WallSlide();
    }

    private void FixedUpdate()
    {
        if (!isWallJumping) // Emp�cher le mouvement normal pendant le dash
        {
            rb.velocity = new Vector2(frameVelocity.x, rb.velocity.y);
        }

        HandleGravity();
    }

    // M�thodes du syst�me d'input
    public void OnGrapple(InputAction.CallbackContext context)
    {
        grapplingHook.OnGrapple(context);
    }

    public void OnGrappleUp(InputAction.CallbackContext context)
    {
        grapplingHook.OnGrappleUp(context);
    }

    public void OnGrappleDown(InputAction.CallbackContext context)
    {
        grapplingHook.OnGrappleDown(context);
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            animator.SetTrigger("isAttacking"); // Activer l'animation d'attaque
            // Ajouter ici le code pour g�rer l'attaque
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started && isGrounded)
        {
            isSprinting = true;
        }
        else if (context.canceled || !isGrounded)
        {
            isSprinting = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            lastJumpTime = jumpBufferTime;

            if (CanJump())
            {
                Jump();
            }
            else if (isWallSliding)
            {
                WallJump();
            }
        }
        else if (context.canceled && rb.velocity.y > 0)
        {
            endedJumpEarly = true;
        }
    }

    public void OnInvertGravity(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            InvertGravity();
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce * (gravityInverted ? -1 : 1));
        isGrounded = false; // Le personnage n'est plus au sol apr�s avoir saut�
        endedJumpEarly = false;
        lastJumpTime = 0;
        lastGroundedTime = 0;
        isJumping = true; // Mettez � jour l'�tat de saut
    }

    private void WallJump()
    {
        if (isTouchingWall)
        {
            isWallJumping = true;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            Vector2 wallJumpVelocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y * (gravityInverted ? -1 : 1));
            rb.velocity = new Vector2(wallJumpVelocity.x, 0); // Annuler la v�locit� verticale actuelle
            rb.AddForce(wallJumpVelocity, ForceMode2D.Impulse);

            if (transform.localScale.x != wallJumpingDirection)
            {
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private void WallSlide()
    {
        if (isTouchingWall && !isGrounded && movementInput.x != 0f)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void FlipSprite(float horizontalMovement)
    {
        Vector3 localScale = playerTransform.localScale;

        if (horizontalMovement < 0)
        {
            localScale.x = -Mathf.Abs(localScale.x); // Flip gauche
        }
        else if (horizontalMovement > 0)
        {
            localScale.x = Mathf.Abs(localScale.x); // Flip droite
        }

        playerTransform.localScale = localScale;
    }

    private void HandleGravity()
    {
        if (isGrounded && rb.velocity.y <= 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }
        else
        {
            var inAirGravity = fallAcceleration;
            if (endedJumpEarly && rb.velocity.y > 0)
            {
                inAirGravity *= jumpEndEarlyGravityModifier;
            }
            rb.velocity = new Vector2(rb.velocity.x, Mathf.MoveTowards(rb.velocity.y, -maxFallSpeed * (gravityInverted ? -1 : 1), inAirGravity * Time.fixedDeltaTime));
        }
    }

    private void HandleDirection()
    {
        float currentSpeed = isGrounded ? groundSpeed : airSpeed;
        float targetSpeed = movementInput.x * currentSpeed * (isSprinting ? sprintSpeedMultiplier : 1f);
        float accelerationRate = isGrounded ? acceleration : airDeceleration;
        frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, targetSpeed, accelerationRate * Time.fixedDeltaTime);
    }

    private void CheckWallContact()
    {
        // Raycast pour d�tecter les murs � gauche et � droite
        RaycastHit2D hitLeft = Physics2D.Raycast(wallCheck.position, Vector2.left, 0.2f, wallLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(wallCheck.position, Vector2.right, 0.2f, wallLayer);

        isTouchingWall = hitLeft.collider != null || hitRight.collider != null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // V�rifiez si le personnage touche le sol
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            lastGroundedTime = coyoteTime; // R�initialiser le coyote time lorsqu'on touche le sol
            isJumping = false; // R�initialiser l'�tat de saut lorsqu'on touche le sol
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // V�rifiez si le personnage quitte le sol
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    private bool CanJump()
    {
        return (lastGroundedTime > 0 || isGrounded) && !isJumping;
    }

    private void InvertGravity()
    {
        gravityInverted = !gravityInverted;
        rb.gravityScale = gravityInverted ? -defaultGravityScale : defaultGravityScale;
        Vector3 localScale = playerTransform.localScale;
        localScale.y *= -1; // Inverse verticalement le personnage
        playerTransform.localScale = localScale;
    }

    public void SetGravityScale(float newGravityScale)
    {
        rb.gravityScale = newGravityScale * (gravityInverted ? -1 : 1);
    }

    public void ResetGravityScale()
    {
        SetGravityScale(defaultGravityScale);
    }

    private void OnDrawGizmosSelected()
    {
        //Ground check visual
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        //Wall check visual
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
    }
}
