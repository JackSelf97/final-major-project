using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController characterController = null;
    private PlayerControls playerControls = null;
    private PlayerInput playerInput = null;

    [Header("Cinemachine")]
    public float currRotationSpeed = 2.0f;
    public float rotationSpeed = 0.0f;
    public GameObject cinemachineCameraTarget;
    public Transform cam = null;
    private float rotationVelocity;
    private float verticalVelocity;
    private float topClamp = 90.0f;
    private float bottomClamp = -90.0f;
    private float cinemachineTargetPitch;
    private const float threshold = 0.01f;

    [Header("Game Properties")]
    public LayerMask interactableLayer;
    public bool lockInput = false;
    public bool analogMovement;
    public bool isPaused = false;
    public float gravityValue = -15.0f;

    [Header("Player Properties")]
    public float speedChangeRate = 10.0f;
    public bool grounded = true;
    public float groundedOffset = -0.14f;
    public float groundedRadius = 0.5f;
    public LayerMask groundLayers;
    public float fallTimeout = 0.15f;
    public bool jump;
    public float jumpTimeout = 0.1f;
    public float jumpHeight = 1.2f;
    private float pushPower = 2.0f;
    private float moveSpeed = 9f;
    private float slopeForce = 40;
    private float slopeForceRayLength = 5;
    private float fallMultiplier = 2.5f;
    private float fallTimeoutDelta;
    private float jumpTimeoutDelta;
    private Vector3 playerVelocity = Vector3.zero;
    private float terminalVelocity = 53.0f;

    [Header("Inventory Settings")]
    [SerializeField] private InventoryPage inventoryPage = null;
    public int inventorySize = 10;

    private bool isCurrentDeviceMouse
    {
        get
        {
        #if ENABLE_INPUT_SYSTEM
            return playerInput.currentControlScheme == "M&K";
        #else
				return false;
        #endif
        }
    }

    private void Awake()
    {
        Application.targetFrameRate = 120;
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        cam = Camera.main.transform;

        // lock state
        //Cursor.lockState = CursorLockMode.Locked;

        // set the size of the player's inventory
        inventoryPage.InitialiseInventoryUI(inventorySize);
    }

    // Update is called once per frame
    void Update()
    {
        // Run-time Checks
        GroundCheck();

        // Player Inputs
        Jump();
        Interaction();
        Inventory();
    }

    void FixedUpdate()
    {
        Move();
    }

    void LateUpdate()
    {
        CameraRotation();
    }

    private void Move()
    {
        if (!lockInput)
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = moveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (GetPlayerMovement() == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(characterController.velocity.x, 0.0f, characterController.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = analogMovement ? GetPlayerMovement().magnitude : 1f;

            float tempSpeed;
            float thousand = 1000;
            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                tempSpeed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * speedChangeRate);

                // round speed to 3 decimal places
                tempSpeed = Mathf.Round(tempSpeed * thousand) / thousand;
            }
            else
            {
                tempSpeed = targetSpeed;
            }

            // normalise input direction
            Vector3 inputDirection = new Vector3(GetPlayerMovement().x, 0.0f, GetPlayerMovement().y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (GetPlayerMovement() != Vector2.zero)
            {
                // move
                inputDirection = transform.right * GetPlayerMovement().x + transform.forward * GetPlayerMovement().y;
            }

            // Move the player
            characterController.Move(inputDirection.normalized * (moveSpeed * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

            #region Slope & Jumping

            // Slope movement
            if (GetPlayerMovement() != Vector2.zero && SlopeCheck())
            {
                characterController.Move(Vector3.down * characterController.height / 2 * slopeForce * Time.fixedDeltaTime);
            }

            // Jump effect
            if (playerVelocity.y < 0f) // if player is falling
            {
                playerVelocity += Vector3.up * gravityValue * (fallMultiplier - 1) * Time.fixedDeltaTime; // for a 'non-floaty' jump
            }

            #endregion
        }
    }

    private void CameraRotation()
    {
        if (!lockInput)
        {
            // if there is an input
            if (GetMouseDelta().sqrMagnitude >= threshold)
            {
                //Don't multiply mouse input by Time.deltaTime
                float deltaTimeMultiplier = isCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                cinemachineTargetPitch += GetMouseDelta().y * currRotationSpeed * deltaTimeMultiplier;
                rotationVelocity = GetMouseDelta().x * currRotationSpeed * deltaTimeMultiplier;

                // clamp our pitch rotation
                cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

                // Update Cinemachine camera target pitch
                cinemachineCameraTarget.transform.localRotation = Quaternion.Euler(cinemachineTargetPitch, 0.0f, 0.0f);

                // rotate the player left and right
                transform.Rotate(Vector3.up * rotationVelocity);
            }
        }
    }

    public void GroundCheck()
    {
        grounded = characterController.isGrounded;
        if (grounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
            jump = false;
        }
    }

    public bool SlopeCheck()
    {
        if (jump) { return false; }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, characterController.height / 2 * slopeForceRayLength))
        {
            if (hit.normal != Vector3.up)
            {
                return true;
            }
        }
        return false;
    }

    public void Jump()
    {
        if (grounded && !lockInput && !isPaused)
        {
            jump = true;

            // reset the fall timeout timer
            fallTimeoutDelta = fallTimeout;

            // stop our velocity dropping infinitely when grounded
            if (verticalVelocity < 0.0f)
            {
                verticalVelocity = -2f;
            }

            // Jump
            if (JumpInput() && jumpTimeoutDelta <= 0.0f && jump)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
            }

            // jump timeout
            if (jumpTimeoutDelta >= 0.0f)
            {
                jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            jumpTimeoutDelta = jumpTimeout;

            // fall timeout
            if (fallTimeoutDelta >= 0.0f)
            {
                fallTimeoutDelta -= Time.deltaTime;
            }
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (verticalVelocity < terminalVelocity)
        {
            verticalVelocity += gravityValue * Time.deltaTime;
        }
    }

    public void Interaction()
    {
        RaycastHit hit;
        const float rayLength = 3;

        Debug.DrawRay(cam.position, cam.forward.normalized * rayLength, Color.cyan);
        if (Physics.Raycast(cam.position, cam.forward.normalized, out hit, rayLength, interactableLayer))
        {
            if (hit.transform.CompareTag("AccessPoint") && hit.transform.name == "Grandfather Clock")
            {
                //InteractionUI(true, "TOUCH");
                if (InteractionInput())
                {
                    AccessPoint accessPoint = hit.transform.GetComponent<AccessPoint>();
                    accessPoint.isGamePaused = !accessPoint.isGamePaused;
                }
            }
        }
        else
        {
            //InteractionUI(false);
        }
    }

    //public void InteractionUI(bool state, string text = null)
    //{
    //    interactionBox.SetActive(state);
    //    interactionText.text = text;
    //}

    public void Inventory()
    {
        if (InventoryInput())
        {
            if (inventoryPage.isActiveAndEnabled == false)
            {
                inventoryPage.Show();
            }
            else
            {
                inventoryPage.Hide();
            }
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // no rigidbody
        if (body == null || body.isKinematic)
        {
            return;
        }

        // We dont want to push objects below us
        if (hit.moveDirection.y < -0.3)
        {
            return;
        }

        // Calculate push direction from move direction,
        // we only push objects to the sides never up and down
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // If you know how fast your character is trying to move,
        // then you can also multiply the push velocity by that.

        // Apply the push
        body.velocity = pushDir * pushPower;
    }

    #region Gamepad Logic (Xbox & PS4)

    public IEnumerator PlayHaptics(float seconds, float leftMotorSpeed = 0.25f, float rightMotorSpeed = 0.25f)
    {
        Gamepad.current.SetMotorSpeeds(leftMotorSpeed, rightMotorSpeed);
        yield return new WaitForSeconds(seconds);
        InputSystem.ResetHaptics();
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    #endregion

    #region Player Inputs

    public Vector2 GetPlayerMovement()
    {
        return playerControls.Player.Move.ReadValue<Vector2>();
    }
    public Vector2 GetMouseDelta()
    {
        return playerControls.Player.Look.ReadValue<Vector2>();
    }
    public bool JumpInput()
    {
        return playerControls.Player.Jump.triggered;
    }
    public bool InteractionInput()
    {
        return playerControls.Player.Interaction.triggered;
    }
    public bool InventoryInput()
    {
        return playerControls.Player.Inventory.triggered;
    }

    #endregion
}
