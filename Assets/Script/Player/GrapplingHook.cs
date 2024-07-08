using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingHook : MonoBehaviour
{
    [SerializeField] private float grappleSpeed = 10f;
    [SerializeField] private LayerMask grappleLayer;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attractionSpeed = 5f;
    [SerializeField] private float slackSpeed = 5f;
    [SerializeField] private float maxGrappleDistance = 20f;

    private Vector2 grapplePoint;
    private DistanceJoint2D joint;
    private bool isGrappling;
    private bool isAttracting;
    private bool isSlacking;

    private void Start()
    {
        joint = gameObject.AddComponent<DistanceJoint2D>();
        joint.enabled = false;
        lineRenderer.enabled = false;
    }

    private void Update()
    {
        if (isGrappling)
        {
            lineRenderer.SetPosition(0, firePoint.position);
            lineRenderer.SetPosition(1, grapplePoint);

            if (isAttracting)
            {
                joint.distance = Mathf.Max(joint.distance - attractionSpeed * Time.deltaTime, 0f);
            }

            if (isSlacking)
            {
                joint.distance = Mathf.Min(joint.distance + slackSpeed * Time.deltaTime, maxGrappleDistance);
            }
        }
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
        if (context.started)
        {
            isAttracting = true;
        }
        else if (context.canceled)
        {
            isAttracting = false;
        }
    }

    public void OnGrappleDown(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isSlacking = true;
        }
        else if (context.canceled)
        {
            isSlacking = false;
        }
    }

    private void StartGrapple()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = mousePosition - (Vector2)firePoint.position;
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, maxGrappleDistance, grappleLayer);

        if (hit.collider != null)
        {
            isGrappling = true;
            grapplePoint = hit.point;
            joint.connectedAnchor = grapplePoint;
            joint.enabled = true;
            joint.distance = Vector2.Distance(firePoint.position, grapplePoint);
            joint.autoConfigureDistance = false;
            joint.autoConfigureConnectedAnchor = false;

            lineRenderer.enabled = true;
        }
    }

    private void StopGrapple()
    {
        isGrappling = false;
        isAttracting = false;
        isSlacking = false;
        joint.enabled = false;
        lineRenderer.enabled = false;
    }
}
