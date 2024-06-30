
using UnityEngine;

public class ChaserEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 3f; // Vitesse de déplacement de l'ennemi
    [SerializeField] private float detectionRange = 10f; // Portée de détection du joueur

    [Header("Player Reference")]
    [SerializeField] private Transform player; // Référence au joueur

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
            // Vérifier si le joueur est à portée
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            isPlayerInRange = distanceToPlayer <= detectionRange;
        }
    }

    private void FixedUpdate()
    {
        if (isPlayerInRange && player != null)
        {
            // Déplacement vers le joueur
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
        }
        else
        {
            // Arrêter l'ennemi lorsqu'il n'est pas à portée
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Dessiner une sphère pour visualiser la portée de détection dans l'éditeur
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
