using UnityEngine;

public class GravityZone : MonoBehaviour
{
    [SerializeField] private float gravityScale = 1f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Entered GravityZone");
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player detected in GravityZone");
            PlayerManager player = collision.GetComponent<PlayerManager>();
            if (player != null)
            {
                player.SetGravityScale(gravityScale);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Exited GravityZone");
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player exited GravityZone");
            PlayerManager player = collision.GetComponent<PlayerManager>();
            if (player != null)
            {
                player.ResetGravityScale(); // Reset to default gravity scale when exiting the zone
            }
        }
    }
}
