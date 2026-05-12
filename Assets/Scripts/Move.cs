using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple helper to move the HIM GameObject away from the player/camera when a key is pressed.
/// Attach this to the same GameObject as the `HIM` component.
/// </summary>
public class Move : MonoBehaviour
{
    [Tooltip("Key that triggers HIM to move.")]
    [SerializeField] private KeyCode moveKey = KeyCode.Space;

    [Tooltip("If true, HIM moves by a random distance between min and max each press.")]
    [SerializeField] private bool useRandomDistance = true;

    [Tooltip("Fixed move distance when not using random.")]
    [SerializeField] private float fixedMoveDistance = 10f;

    [Tooltip("Minimum random move distance.")]
    [SerializeField] private float minMoveDistance = 5f;

    [Tooltip("Maximum random move distance.")]
    [SerializeField] private float maxMoveDistance = 15f;

    // Optional: require HIM to be visible before allowing manual move
    [SerializeField] private bool requireVisible = false;

    [Header("Visibility Safety (ensures player can see HIM after move)")]
    [Tooltip("If true, adjust HIM position after moving so it's inside the player's camera view.")]
    [SerializeField] private bool keepVisibleInView = true;

    [Tooltip("Margin from viewport edges (0..0.5). HIM will be kept inside viewport with this margin.")]
    [SerializeField] [Range(0f, 0.5f)] private float viewportMargin = 0.05f;

    [Tooltip("Minimum distance in front of the camera when an adjusted position would otherwise be behind the camera.")]
    [SerializeField] private float minDistanceFromCamera = 2f;

    private HIM him;
    private Player player;

    private void Awake()
    {
        him = GetComponent<HIM>();
        player = FindObjectOfType<Player>();
    }

    private void Update()
    {
        // Don't allow manual HIM moves while player movement/input is paused
        if (player != null && player.IsMovementPaused)
            return;

        if (Input.GetKeyDown(moveKey))
        {
            if (requireVisible && (him == null || !him.IsVisible()))
            {
                Debug.Log("Move: HIM not visible, ignoring manual move.");
                return;
            }

            float distance = useRandomDistance
                ? Random.Range(minMoveDistance, maxMoveDistance)
                : fixedMoveDistance;

            Vector3 origin = Vector3.zero;
            if (player != null)
            {
                // Prefer player position if available
                origin = player.transform.position;
            }
            else if (Camera.main != null)
            {
                origin = Camera.main.transform.position;
            }

            if (him != null)
            {
                // Compute desired position explicitly so we can ensure it's visible to the camera
                Vector3 dir = him.transform.position - origin;
                if (dir.sqrMagnitude < 0.0001f) dir = Vector3.up;
                dir.Normalize();

                Vector3 desiredPos = him.transform.position + dir * distance;

                // If requested, adjust desiredPos so it's inside the main camera viewport
                if (keepVisibleInView && Camera.main != null)
                {
                    desiredPos = AdjustPositionToCameraView(desiredPos, Camera.main, viewportMargin, minDistanceFromCamera);
                }

                him.transform.position = desiredPos;

                // Ensure HIM is shown (alpha / active) so the player can actually see him
                him.ShowHIM();

                Debug.Log($"Move: HIM moved to {desiredPos} (requested distance {distance:F1} from {origin}).");
            }
            else
            {
                // Fallback: move transform directly away from origin then ensure it's visible
                Vector3 dir = transform.position - origin;
                if (dir.sqrMagnitude < 0.0001f) dir = Vector3.up;
                dir.Normalize();

                Vector3 desiredPos = transform.position + dir * distance;

                if (keepVisibleInView && Camera.main != null)
                {
                    desiredPos = AdjustPositionToCameraView(desiredPos, Camera.main, viewportMargin, minDistanceFromCamera);
                }

                transform.position = desiredPos;

                // If there's a HIM component later added, showing it would be done via its ShowHIM.
                Debug.Log($"Move: (fallback) moved transform by {distance:F1} to {desiredPos}.");
            }
        }
    }

    // Move/position adjustment so the point is inside the camera viewport with a margin.
    // Uses WorldToViewportPoint and clamps X/Y to [margin, 1-margin]. Preserves depth when in front of camera.
    private Vector3 AdjustPositionToCameraView(Vector3 desiredWorldPos, Camera cam, float margin, float minZ)
    {
        Vector3 viewport = cam.WorldToViewportPoint(desiredWorldPos);

        // If behind the camera, force it in front at minimum distance
        if (viewport.z <= 0f)
        {
            viewport.z = Mathf.Max(minZ, 1f);
        }

        // Clamp XY to viewport with margin so HIM is not near screen edge or off-screen
        viewport.x = Mathf.Clamp(viewport.x, margin, 1f - margin);
        viewport.y = Mathf.Clamp(viewport.y, margin, 1f - margin);

        Vector3 adjusted = cam.ViewportToWorldPoint(viewport);
        return adjusted;
    }
}
