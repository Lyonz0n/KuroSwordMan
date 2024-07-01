using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
/*
public class PlayerManager : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animator animator; // R�f�rence � l'Animator

    [Header("Movement Setting")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 10f; // Force du saut
    [SerializeField] private float dashSpeed = 20f; // Vitesse du dash
    [SerializeField] private float dashDuration = 0.2f; // Dur�e du dash
    [SerializeField] private float groundDeceleration = 10f; // D�c�l�ration au sol
    [SerializeField] private float airDeceleration = 5f; // D�c�l�ration en l'air
    [SerializeField] private float acceleration = 10f; // Acc�l�ration

    [Header("Gravity Settings")]
    [SerializeField] private float groundingForce = -20f; // Force appliqu�e lorsque le personnage est au sol
    [SerializeField] private float fallAcceleration = 20f; // Acc�l�ration de la chute
    [SerializeField] private float maxFallSpeed = 40f; // Vitesse de chute maximale
    [SerializeField] private float jumpEndEarlyGravityModifier = 2f; // Modificateur de gravit� lorsque le saut est termin� pr�matur�ment

    public Rigidbody2D rb;

    private Vector2 movementInput;
    private Vector2 frameVelocity;
    private bool isJumping = false;
    private bool isGrounded = true; // Assurez-vous de mettre � jour cette variable en fonction de votre d�tection de sol

    private bool isDashing;
    private bool endedJumpEarly = false;
    private Transform playerTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerTransform = transform; // Assurez-vous que le script est attach� au joueur
    }

    private void Update()
    {
        HandleDirection();

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
    }

    private void FixedUpdate()
    {
        if (!isDashing) // Emp�cher le mouvement normal pendant le dash
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
            // Ajouter ici le code pour g�rer l'attaque
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
        if (context.started && isGrounded)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            isGrounded = false; // Le personnage n'est plus au sol apr�s avoir saut�
            endedJumpEarly = false;
        }
        else if (context.canceled && rb.velocity.y > 0)
        {
            endedJumpEarly = true;
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        Vector2 dashDirection = movementInput.normalized;

        // Appliquer une v�locit� constante pour le dash
        rb.velocity = dashDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.velocity = Vector2.zero; // Arr�ter le joueur apr�s la dur�e du dash
        isDashing = false;
    }

    private void FlipSprite(float horizontalMovement)
    {
        Vector3 localScale = playerTransform.localScale;

        if (horizontalMovement < 0)
        {
            localScale.x = Mathf.Abs(localScale.x); // Flip gauche
        }
        else if (horizontalMovement > 0)
        {
            localScale.x = -Mathf.Abs(localScale.x); // Flip droite
        }

        playerTransform.localScale = localScale;
    }

    private void HandleGravity()
    {
        if (isGrounded && rb.velocity.y <= 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, groundingForce);
        }
        else
        {
            var inAirGravity = fallAcceleration;
            if (endedJumpEarly && rb.velocity.y > 0)
            {
                inAirGravity *= jumpEndEarlyGravityModifier;
            }
            rb.velocity = new Vector2(rb.velocity.x, Mathf.MoveTowards(rb.velocity.y, -maxFallSpeed, inAirGravity * Time.fixedDeltaTime));
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // V�rifiez si le personnage touche le sol
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}*/
