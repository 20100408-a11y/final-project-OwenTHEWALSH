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
                him.MoveAwayFrom(origin, distance);
                Debug.Log($"Move: HIM moved away by {distance:F1} from {origin}.");
            }
            else
            {
                // Fallback: move transform directly away from origin
                Vector3 dir = transform.position - origin;
                if (dir.sqrMagnitude < 0.0001f) dir = Vector3.up;
                dir.Normalize();
                transform.position += dir * distance;
                Debug.Log($"Move: (fallback) moved transform by {distance:F1}.");
            }
        }
    }
}
