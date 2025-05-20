using UnityEngine;

// Detects when player enters and exits shadow zone (or any zone where this collider is placed)
public class ShadowZone : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";

    void OnEnable()
    {
        ShadowZoneManager.Register(this);
    }

    void OnDisable()
    {
        ShadowZoneManager.Unregister(this);
    }

    public Vector3 GetCenter() => GetComponent<Collider>().bounds.center;

    /// <summary>
    /// Returns the surface point of this shadow zone, which is the point closest to the center
    /// of the zone where the player can stand without clipping through the zone.
    /// </summary>
    /// <returns>The surface point of this shadow zone.</returns>
    public Vector3 GetSurfacePoint()
    {
        // Try a raycast from above directly downward to find the surface
        Vector3 fromAbove = GetCenter() + Vector3.up * 2f;

        if (Physics.Raycast(fromAbove, Vector3.down, out RaycastHit hit, 10f))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                return hit.point + Vector3.up * -1.2f + new Vector3(3, 0, 2f); // making it center
            }
        }

        // fallback: center of bounds slightly above ground
        return GetCenter() + Vector3.up * 0.3f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(PLAYER_TAG))
        {
            PlayerShadowState.Instance?.SetInShadow(true, this);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(PLAYER_TAG))
        {
            PlayerShadowState.Instance?.SetInShadow(false, this);
        }
    }
}
