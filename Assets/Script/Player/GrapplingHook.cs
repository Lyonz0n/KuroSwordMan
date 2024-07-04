using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    [SerializeField] private float grappleLength = 10f;
    [SerializeField] private float attractionSpeed = 5f; // Vitesse d'attraction vers l'ancre
    [SerializeField] private LayerMask grappleLayer;
    private Vector3 grapplePoint;
    private DistanceJoint2D joint;
    private LineRenderer lineRenderer;
    private GameObject grappleAnchor;

    // Start is called before the first frame update
    void Start()
    {
        joint = gameObject.GetComponent<DistanceJoint2D>();
        joint.enabled = false;

        // Ajout d'un LineRenderer pour visualiser le grappin
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
                RaycastHit2D hit = Physics2D.Raycast(
                origin: Camera.main.ScreenToWorldPoint(Input.mousePosition),
                direction: Vector2.zero,
                distance: Mathf.Infinity,
                layerMask: grappleLayer);

            if (hit.collider != null)
            {
                grapplePoint = hit.point;
                grapplePoint.z = 0;

                // Créer un point d'ancrage pour le grappin
                CreateGrappleAnchor(grapplePoint);

                joint.connectedAnchor = grapplePoint;
                joint.enabled = true;
                joint.distance = grappleLength;

                // Activer et définir les positions du LineRenderer
                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, grapplePoint);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            joint.enabled = false;
            lineRenderer.enabled = false;

            // Détruire le point d'ancrage du grappin
            if (grappleAnchor != null)
            {
                Destroy(grappleAnchor);
            }
        }

        // Mettre à jour la position de départ du LineRenderer
        if (joint.enabled)
        {
            lineRenderer.SetPosition(0, transform.position);

            // Vérifier si la touche d'attraction est enfoncée
            if (Input.GetKey(KeyCode.E))
            {
                AttractToAnchor();
            }
        }
    }

    private void CreateGrappleAnchor(Vector3 anchorPosition)
    {
        if (grappleAnchor != null)
        {
            Destroy(grappleAnchor);
        }

        grappleAnchor = new GameObject("GrappleAnchor");
        grappleAnchor.transform.position = anchorPosition;
        Rigidbody2D anchorRb = grappleAnchor.AddComponent<Rigidbody2D>();
        anchorRb.bodyType = RigidbodyType2D.Dynamic;
        anchorRb.gravityScale = 0f; // Vous pouvez ajuster la gravité si nécessaire

        joint.connectedBody = anchorRb;
    }

    private void AttractToAnchor()
    {
        Vector2 direction = (grapplePoint - transform.position).normalized;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.AddForce(direction * attractionSpeed, ForceMode2D.Force);
    }
}
