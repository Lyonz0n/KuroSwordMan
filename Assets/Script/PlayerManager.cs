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

    public Rigidbody2D rb;

    private Vector2 movementInput;
    private Vector2 smoothedMovementInput;
    private Vector2 movementInputSmoothVelocity;
    private float horizontalInput;
    private bool isJumping = false;
    private bool isGrounded = true; // Assurez-vous de mettre à jour cette variable en fonction de votre détection de sol

    private bool isDashing;
    private Transform playerTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerTransform = transform; // Assurez-vous que le script est attaché au joueur
    }

    private void Update()
    {
        horizontalInput = movementInput.x;

        if (horizontalInput != 0)
        {
           // FlipSprite(horizontalInput);
            animator.SetBool("isRunning", true); // Activer l'animation de course
        }
        else
        {
            animator.SetBool("isRunning", false); // Désactiver l'animation de course
        }

        // Mettre à jour l'état de saut dans l'Animator
        animator.SetBool("isJumping", !isGrounded);
    }

    private void FixedUpdate()
    {
        if (!isDashing) // Empêcher le mouvement normal pendant le dash
        {
            smoothedMovementInput = Vector2.SmoothDamp(smoothedMovementInput, movementInput, ref movementInputSmoothVelocity, 0.1f);
            rb.velocity = new Vector2(smoothedMovementInput.x * speed, rb.velocity.y);
        }
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
        if (context.started && isGrounded)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            isGrounded = false; // Le personnage n'est plus au sol après avoir sauté
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        Vector2 dashDirection = movementInput.normalized;

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Vérifiez si le personnage touche le sol
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}
