using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Tooltip("The downward velocity threshold at which fall damage begins.")]
    public float fallDamageThreshold = -10f;
    [Tooltip("Damage applied per unit of downward velocity beyond the threshold.")]
    public float fallDamageMultiplier = 2f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Player player;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Tooltip("The camera to use for movement direction. If left null, Camera.main will be used.")]
    public Transform cameraTransform;

    [Tooltip("The animator controller for the player character.")]
    public Animator animator;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        player = GetComponent<Player>() ?? Player.Instance;

        if (animator == null)
        {
            // Tries to find an Animator on this object or its children if one isn't assigned
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        bool wasGroundedLastFrame = isGrounded;

        // Check if grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded)
        {
            if (!wasGroundedLastFrame && velocity.y < 0f)
            {
                TryApplyFallDamage(velocity.y);
            }

            if (velocity.y < 0)
            {
                velocity.y = -2f; // small downward force to keep grounded
            }
        }

        // Movement input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move;
        Transform cam = cameraTransform;
        if (cam == null && Camera.main != null)
        {
            cam = Camera.main.transform;
        }

        if (cam != null)
        {
            Vector3 camForward = cam.forward;
            Vector3 camRight = cam.right;

            // Keep movement horizontal
            camForward.y = 0f;
            camRight.y = 0f;

            camForward.Normalize();
            camRight.Normalize();

            move = camRight * x + camForward * z;
        }
        else
        {
            // Fallback to local transform if no camera is found
            move = transform.right * x + transform.forward * z;
        }

        bool isMoving = move.magnitude > 0.1f;

        if (isMoving)
        {
            if (cam != null)
            {
                Vector3 camForward = cam.forward;
                camForward.y = 0f;
                camForward.Normalize();
                
                // Rotates the player to face the same direction as the camera
                transform.forward = move.normalized; 
                
                // If you instead wanted the player to face the direction they are walking, 
                // you would use: transform.forward = move.normalized;
            }
            else
            {
                transform.forward = move.normalized;
            }
        }

        // Update the animator
        if (animator != null)
        {
            animator.SetBool("IsRunning", isMoving);
        }

        controller.Move(move * speed * Time.deltaTime);

        // Jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void TryApplyFallDamage(float impactVelocity)
    {
        if (impactVelocity >= fallDamageThreshold)
            return;

        float damage = (Mathf.Abs(impactVelocity) - Mathf.Abs(fallDamageThreshold)) * fallDamageMultiplier;
        if (damage > 0f && player != null)
        {
            player.TakeDamage(damage);
        }
    }
}
