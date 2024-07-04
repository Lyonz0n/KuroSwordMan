using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animator animator; // Référence à l'Animator

    [Header("Movement Setting")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 10f; // Force du saut
    [SerializeField] private float dashSpeed = 20f; // Vitesse du dash
    [SerializeField] private float dashDuration = 0.2f; // Durée du dash
    [SerializeField] private float groundDeceleration = 10f; // Décélération au sol
    [SerializeField] private float airDeceleration = 5f; // Décélération en l'air
    [SerializeField] private float acceleration = 10f; // Accélération
    [SerializeField] private float wallJumpForce = 10f; // Force du wall jump
    [SerializeField] private Vector2 wallJumpDirection = new Vector2(1, 1); // Direction du wall jump

    [Header("Gravity Settings")]
    [SerializeField] private float defaultGravityScale = 1f;
    [SerializeField] private float fallAcceleration = 20f; // Accélération de la chute
    [SerializeField] private float maxFallSpeed = 40f; // Vitesse de chute maximale
    [SerializeField] private float jumpEndEarlyGravityModifier = 2f; // Modificateur de gravité lorsque le saut est terminé prématurément

    [Header("Assists")]
    [SerializeField] private float coyoteTime = 0.2f; // Période de grâce après avoir quitté le sol
    [SerializeField] private float jumpBufferTime = 0.2f; // Période de buffer pour les inputs de saut

    public Rigidbody2D rb;
    public LayerMask wallLayer; // Masque de couche pour détecter les murs

    private Vector2 movementInput;
    private Vector2 frameVelocity;
    private bool isJumping = false;
    private bool isGrounded = true; // Assurez-vous de mettre à jour cette variable en fonction de votre détection de sol
    private bool isTouchingWall = false; // Indique si le joueur touche un mur
    private bool isDashing;
    private bool endedJumpEarly = false;
    private Transform playerTransform;

    private float lastGroundedTime; // Temps écoulé depuis que le joueur a quitté le sol
    private float lastJumpTime; // Temps écoulé depuis que le joueur a appuyé sur le bouton de saut
    private bool gravityInverted = false; // État de la gravité

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerTransform = transform; // Assurez-vous que le script est attaché au joueur
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
            animator.SetBool("isRunning", false); // Désactiver l'animation de course
        }

        // Mettre à jour l'état de saut dans l'Animator
        animator.SetBool("isJumping", !isGrounded);

        // Décrémenter les timers de coyote time et de jump buffer
        lastGroundedTime -= Time.deltaTime;
        lastJumpTime -= Time.deltaTime;

        // Vérifier les conditions pour sauter
        if (CanJump() && lastJumpTime > 0)
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        if (!isDashing) // Empêcher le mouvement normal pendant le dash
        {
            rb.velocity = new Vector2(frameVelocity.x, rb.velocity.y);
        }

        HandleGravity();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            animator.SetTrigger("isAttacking"); // Activer l'animation d'attaque
            // Ajouter ici le code pour gérer l'attaque
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && !isDashing)
        {
            StartCoroutine(Dash());
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
            else if (isTouchingWall)
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
        isGrounded = false; // Le personnage n'est plus au sol après avoir sauté
        endedJumpEarly = false;
        lastJumpTime = 0;
        lastGroundedTime = 0;
        isJumping = true; // Mettez à jour l'état de saut
    }

    private void WallJump()
    {
        // Appliquer une force pour le wall jump
        Vector2 wallJumpVelocity = new Vector2(wallJumpDirection.x * wallJumpForce * -Mathf.Sign(movementInput.x), wallJumpDirection.y * wallJumpForce * (gravityInverted ? -1 : 1));
        rb.velocity = new Vector2(wallJumpVelocity.x, 0); // Annuler la vélocité verticale actuelle
        rb.AddForce(wallJumpVelocity, ForceMode2D.Impulse);
        endedJumpEarly = false;
        lastJumpTime = 0;
        lastGroundedTime = 0;
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        Vector2 dashDirection = movementInput.normalized;

        // Activer l'animation de dash
        animator.SetTrigger("Dash");

        // Appliquer une vélocité constante pour le dash
        rb.velocity = dashDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.velocity = Vector2.zero; // Arrêter le joueur après la durée du dash
        isDashing = false;
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
        if (movementInput.x == 0)
        {
            var deceleration = isGrounded ? groundDeceleration : airDeceleration;
            frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, movementInput.x * speed, acceleration * Time.fixedDeltaTime);
        }
    }

    private void CheckWallContact()
    {
        // Raycast pour détecter les murs à gauche et à droite
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, 0.5f, wallLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, 0.5f, wallLayer);

        isTouchingWall = hitLeft.collider != null || hitRight.collider != null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Vérifiez si le personnage touche le sol
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            lastGroundedTime = coyoteTime; // Réinitialiser le coyote time lorsqu'on touche le sol
            isJumping = false; // Réinitialiser l'état de saut lorsqu'on touche le sol
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Vérifiez si le personnage quitte le sol
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
}
