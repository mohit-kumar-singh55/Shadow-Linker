using UnityEngine;

// Singleton class for keeping track of whether the player is in shadow
public class PlayerShadowState : MonoBehaviour
{
    public static PlayerShadowState Instance;
    public bool isInShadow { get; private set; }
    public ShadowZone CurrentZone { get; private set; }

    void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    public void SetInShadow(bool inShadow, ShadowZone zone = null)
    {
        isInShadow = inShadow;

        if (inShadow) CurrentZone = zone;
        else if (CurrentZone == zone) CurrentZone = null;

        Debug.Log("Player in shadow: " + isInShadow);
    }
}
