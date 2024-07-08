using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingHook : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public LayerMask grappleLayer;
    public Transform firePoint;
    public GameObject grappleAnchorPrefab;
    public float maxGrappleDistance = 10f; // Augmenter la portée du grappin

    private DistanceJoint2D distanceJoint;
    private Rigidbody2D rb;
    private Vector2 grapplePoint;
    private GameObject grappleAnchor;
    private bool isGrappling;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        distanceJoint = gameObject.AddComponent<DistanceJoint2D>();
        distanceJoint.enabled = false;
        distanceJoint.autoConfigureDistance = false;
        distanceJoint.enableCollision = true; // Activer les collisions pour le DistanceJoint2D
    }

    public void OnGrapple(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            StartGrapple();
        }
        else if (context.canceled)
        {
            StopGrapple();
        }
    }

    public void OnGrappleUp(InputAction.CallbackContext context)
    {
        if (context.started && isGrappling)
        {
            distanceJoint.distance = Mathf.Max(distanceJoint.distance - 1f, 1f); // Empêcher la distance d'être négative
        }
    }

    public void OnGrappleDown(InputAction.CallbackContext context)
    {
        if (context.started && isGrappling)
        {
            distanceJoint.distance += 1f;
        }
    }

    private void StartGrapple()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (mousePosition - (Vector2)firePoint.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, maxGrappleDistance, grappleLayer);

        if (hit.collider != null)
        {
            isGrappling = true;
            grapplePoint = hit.point;
            distanceJoint.enabled = true;
            distanceJoint.connectedAnchor = grapplePoint;
            distanceJoint.distance = Vector2.Distance(firePoint.position, grapplePoint);
            distanceJoint.connectedBody = hit.collider.GetComponent<Rigidbody2D>();

            lineRenderer.positionCount = 2;

            if (grappleAnchor != null)
            {
                Destroy(grappleAnchor);
            }

            grappleAnchor = Instantiate(grappleAnchorPrefab, grapplePoint, Quaternion.identity);
        }
    }

    private void StopGrapple()
    {
        isGrappling = false;
        distanceJoint.enabled = false;
        lineRenderer.positionCount = 0;

        if (grappleAnchor != null)
        {
            Destroy(grappleAnchor);
        }
    }

    private void Update()
    {
        if (isGrappling)
        {
            lineRenderer.SetPosition(0, firePoint.position);
            lineRenderer.SetPosition(1, grapplePoint);
        }
    }
}
