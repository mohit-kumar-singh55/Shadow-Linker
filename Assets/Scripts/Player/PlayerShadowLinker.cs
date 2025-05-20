using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShadowLinker : MonoBehaviour
{
    [Header("Teleport Settings")]
    public LayerMask obstacleMask;
    public float maxLinkDistance = 20f;
    public KeyCode linkKey = KeyCode.E;
    public GameObject orbPrefab; // assign in Inspector

    [Header("Cooldown Settings")]
    public float cooldownDuration = 5f;
    public Image cooldownUI;

    [Header("Teleport FX")]
    public AudioSource audioSource; // reference to AudioSource on the player
    public CanvasGroup fadeOverlay; // black screen
    public float suckDuration = 0.5f;
    public Transform playerModel; // assign actual player visuals (not root transform)
    public float suckScale = 0.1f;
    public Material distortionMaterial;     // shader for distortion effect

    private Transform cam;
    private GameObject currentOrb;
    private ShadowZone currentTarget;
    private bool isHolding;
    private bool isCooldown;
    private float cooldownTimer;

    void Start()
    {
        cam = Camera.main.transform;
        cooldownTimer = cooldownDuration;
        if (cooldownUI != null) cooldownUI.fillAmount = 1f; // full at start
    }

    /// <summary>
    /// When the key is hold, it will start checking for the nearest
    /// shadow zone to teleport to.
    /// When the key is released, it will stop checking and teleport the
    /// player to the nearest target if found.
    /// </summary>
    void Update()
    {
        // Handle cooldown
        if (isCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownUI != null) cooldownUI.fillAmount = Mathf.Clamp01(1f - (cooldownTimer / cooldownDuration));

            if (cooldownTimer <= 0f)
            {
                isCooldown = false;
                cooldownTimer = 0f;
                if (cooldownUI != null) cooldownUI.fillAmount = 1f;
            }
            return; // block any shadow linking during cooldown
        }

        // Safety check
        if (PlayerShadowState.Instance == null || !PlayerShadowState.Instance.isInShadow)
        {
            isHolding = false;
            DestroyOrb();
            return;
        }

        // When player starts holding the key
        if (Input.GetKeyDown(linkKey)) isHolding = true;

        // When player stops holding the key
        if (Input.GetKeyUp(linkKey))
        {
            if (currentOrb != null && currentTarget != null) TeleportTo(currentTarget);
            else Debug.Log("Teleport canceled");

            isHolding = false;
            DestroyOrb();
            currentTarget = null;
        }

        if (isHolding) GetNearestShadowTarget();
    }

    /// <summary>
    /// Finds the closest visible ShadowZone from the given camera position and direction.
    /// The direction is used to filter out zones that are outside of a 45° cone in front.
    /// If the player is already in a zone, that one is excluded from the search.
    /// </summary>
    /// <remarks>
    /// This function updates the currentOrb and currentTarget fields accordingly.
    /// </remarks>
    void GetNearestShadowTarget()
    {
        ShadowZone currentZone = PlayerShadowState.Instance.CurrentZone;
        ShadowZone target = ShadowZoneManager.GetClosestVisibleShadow(cam.position, cam.forward, maxLinkDistance, obstacleMask, currentZone);

        if (target != null && (currentZone == null || target != currentZone))
        {
            Vector3 destination = target.GetSurfacePoint();

            if (currentOrb == null) currentOrb = Instantiate(orbPrefab, destination, Quaternion.identity);
            else currentOrb.transform.position = destination;

            // setting target
            currentTarget = target;
        }
    }

    void TeleportTo(ShadowZone zone)
    {
        Vector3 destination = zone.GetCenter() + Vector3.up * 1.5f;

        // Play sfx
        if (audioSource != null) audioSource.Play();

        DestroyOrb();
        StartCoroutine(PortalTeleportVFX(destination));
    }

    void StartCooldown()
    {
        isCooldown = true;
        cooldownTimer = cooldownDuration;
        if (cooldownUI != null) cooldownUI.fillAmount = 0f;
    }

    void DestroyOrb()
    {
        if (currentOrb != null)
        {
            Destroy(currentOrb);
            currentOrb = null;
        }
    }

    /// <summary>
    /// Handles the visual effects for teleporting the player to a new destination.
    /// This includes fading the screen to black, shrinking the player model, applying
    /// a distortion effect, performing an instant teleport, and then reversing the effects.
    /// </summary>
    IEnumerator PortalTeleportVFX(Vector3 destination)
    {
        float t = 0f;
        Vector3 originalScale = playerModel.localScale;
        Vector3 targetScale = originalScale * suckScale;

        // Fade to black & shrink
        while (t < suckDuration)
        {
            t += Time.deltaTime;
            float normalized = t / suckDuration;

            playerModel.localScale = Vector3.Lerp(originalScale, targetScale, normalized);
            fadeOverlay.alpha = normalized;

            // Apply distortion effect
            distortionMaterial.SetFloat("_Strength", 10f * normalized);

            yield return null;
        }

        // Instant teleport
        transform.position = destination;

        // Fade back and scale up
        t = 0f;
        while (t < suckDuration)
        {
            t += Time.deltaTime;
            float normalized = t / suckDuration;

            playerModel.localScale = Vector3.Lerp(targetScale, originalScale, normalized);
            fadeOverlay.alpha = 1f - normalized;

            // resetting distortion effect
            distortionMaterial.SetFloat("_Strength", .001f * normalized);

            yield return null;
        }

        fadeOverlay.alpha = 0f;
        playerModel.localScale = originalScale;
        distortionMaterial.SetFloat("_Strength", 0f);
        StartCooldown();
    }
}
