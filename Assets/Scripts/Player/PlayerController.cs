using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float crouchSpeed = 2.5f;
    public float jumpForce = 5f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    public float standingHeight = 1f;
    public float crouchingHeight = .5f;

    [Header("Camera")]
    public Transform cameraRoot;
    public float mouseSensitivity = 2f;
    public float maxPitch = 80f;
    public float crouchCamOffset = -0.5f;
    public float transitionSpeed = 5f;

    private Rigidbody rb;
    private bool isCrouching = false;
    private float currentSpeed;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isGrounded;
    private float pitch = 0f;

    // for smooth camera look
    private float targetPitch;
    private float targetYaw;
    private float currentYaw;

    public static PlayerController Instance { get; private set; }
    public bool IsCrouching => isCrouching;

    private void Awake()
    {
        // singleton class
        if (Instance != null) Destroy(gameObject);
        Instance = this;

        // rigidbody
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevent default physics rotation

        // crouch
        currentSpeed = walkSpeed;

        // cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // smooth crouch
        ToggleCrouch();

        HandleLook();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        CheckGrounded();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void OnCrouch(InputValue value)
    {
        isCrouching = !isCrouching;
        currentSpeed = isCrouching ? crouchSpeed : walkSpeed;
    }

    /// <summary>
    /// Handles movement input from the player.
    /// </summary>
    private void HandleMovement()
    {
        Vector3 move = transform.forward * moveInput.y + transform.right * moveInput.x;
        Vector3 targetVelocity = move * currentSpeed;
        Vector3 velocityChange = targetVelocity - rb.linearVelocity;
        velocityChange.y = 0f; // Don't interfere with vertical velocity (gravity/jump)
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    /// <summary>
    /// Handles the camera look functionality by processing the player's mouse input.
    /// Adjusts the pitch and yaw of the camera smoothly to ensure a fluid motion.
    /// Clamps the pitch to prevent excessive rotation and updates the camera's and player's rotation.
    /// </summary>
    private void HandleLook()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        // calculate target pitch and yaw
        targetPitch -= mouseY;
        targetPitch = Mathf.Clamp(targetPitch, -maxPitch, maxPitch);
        targetYaw += mouseX;

        // set to current pitch and yaw
        pitch = Mathf.Lerp(pitch, targetPitch, Time.deltaTime * 10f); // Smooth pitch
        currentYaw = Mathf.Lerp(currentYaw, targetYaw, Time.deltaTime * 10f); // Smooth yaw

        // apply rotation
        cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        transform.rotation = Quaternion.Euler(0f, currentYaw, 0f);
    }

    /// <summary>
    /// Checks if the player is grounded by performing a raycast from its
    /// position downwards with a slight offset to ensure it is not colliding
    /// with itself.
    /// </summary>
    private void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + 0.1f, groundLayer.value);
    }

    /// <summary>
    /// Smoothly crouches the player by adjusting its scale to the crouching height.
    /// </summary>
    private void ToggleCrouch()
    {
        // Smoothly crouch
        Vector3 targetScale = new Vector3(1, isCrouching ? crouchingHeight : standingHeight, 1);
        if (targetScale == transform.localScale) return;

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * transitionSpeed);
    }
}
