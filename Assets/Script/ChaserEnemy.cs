
using UnityEngine;

public class ChaserEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 3f; // Vitesse de d�placement de l'ennemi
    [SerializeField] private float detectionRange = 10f; // Port�e de d�tection du joueur

    [Header("Player Reference")]
    [SerializeField] private Transform player; // R�f�rence au joueur

    private Rigidbody2D rb;
    private bool isPlayerInRange = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (player != null)
        {
            // V�rifier si le joueur est � port�e
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            isPlayerInRange = distanceToPlayer <= detectionRange;
        }
    }

    private void FixedUpdate()
    {
        if (isPlayerInRange && player != null)
        {
            // D�placement vers le joueur
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
        }
        else
        {
            // Arr�ter l'ennemi lorsqu'il n'est pas � port�e
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Dessiner une sph�re pour visualiser la port�e de d�tection dans l'�diteur
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
