using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingHook : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public LayerMask grappleLayer;
    public Transform firePoint;
    public GameObject grappleAnchorPrefab;

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
            distanceJoint.distance -= 1f;
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
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, mousePosition - (Vector2)firePoint.position, Mathf.Infinity, grappleLayer);

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
