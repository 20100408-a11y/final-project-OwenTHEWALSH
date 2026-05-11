using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Camera Move Settings")]
    [Tooltip("Camera to move. If null, Camera.main will be used.")]
    [SerializeField]
    private Camera targetCamera;

    [Header("Light Settings")]
    [Tooltip("Light that is a child of the camera. Moves with the camera.")]
    [SerializeField]
    private Light cameraLight;

    [Header("Repel Settings")]
    [Tooltip("Radius to detect objects for repelling.")]
    [SerializeField]
    private float repelRadius = 10f;

    [Tooltip("Force applied to repel objects.")]
    [SerializeField]
    private float repelForce = 20f;

    [Tooltip("Tag to identify objects to repel (e.g., 'HIM').")]
    [SerializeField]
    private string repelTag = "HIM";

    // Prevent starting multiple concurrent moves       
    private Coroutine moveCoroutine;

    private const float moveUpAmount = 10f; // Always move up by 10 units

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            Debug.LogWarning("Player: No camera found to move. Assign a Camera in the Inspector or ensure there is a Camera tagged MainCamera.");
        }

        if (cameraLight == null)
        {
            Debug.LogWarning("Player: No light assigned. Assign a Light child of the camera in the Inspector for best results.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (targetCamera == null)
                return;

            // Instantly move the camera up by 10 units
            targetCamera.transform.position += Vector3.up * moveUpAmount;

            RepelObjects();
        }
    }

    private void RepelObjects()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, repelRadius);

        foreach (Collider collider in hitColliders)
        {
            if (collider.CompareTag(repelTag))
            {
                Rigidbody rb = collider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 repelDirection = (collider.transform.position - transform.position).normalized;
                    rb.velocity = Vector3.zero;
                    rb.AddForce(repelDirection * repelForce, ForceMode.Impulse);
                }
            }
        }
    }

    public Camera GetCamera()
    {
        return targetCamera;
    }
}
