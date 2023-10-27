using Inventory;
using Inventory.Model;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // Player Components
    private CharacterController characterController = null;
    private PlayerInventory playerInventory = null;
    private PlayerStats playerStats = null;
    private PlayerControls playerControls = null;

    [Header("Cinemachine")]
    [SerializeField] private GameObject camPos = null;
    private Transform cam = null;
    private float currRotationSpeed = 3.0f;
    private float rotationVelocity;
    private float verticalVelocity;
    private float topClamp = 90.0f;
    private float bottomClamp = -90.0f;
    private float cinemachineTargetPitch;
    private const float threshold = 0.01f;

    [Header("Game Properties")]
    public GameObject pauseScreen = null;
    public bool locked = false;
    public bool isPaused = false;
    private bool analogMovement;
    private float gravityValue = -9.81f;

    [Header("Player Properties")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float speedChangeRate = 10.0f;
    [SerializeField] private float groundedOffset = -0.14f;
    [SerializeField] private float groundedRadius = 0.5f;
    [SerializeField] private float fallTimeout = 0.15f;
    [SerializeField] private float jumpTimeout = 0.1f;
    [SerializeField] private float jumpHeight = 1.2f;
    private bool jump;
    private bool grounded = true;
    private float pushPower = 2.0f;
    private float moveSpeed = 9f;
    private float slopeForce = 40;
    private float slopeForceRayLength = 5;
    private float fallMultiplier = 2.5f;
    private float fallTimeoutDelta;
    private float jumpTimeoutDelta;
    private float terminalVelocity = 53.0f;
    private Vector3 playerVelocity = Vector3.zero;

    [Header("Interaction")]
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Sprite[] interactionSprite = new Sprite[0];
    [SerializeField] private Image crosshair = null;
    [SerializeField] private Image interactionImage = null;

    [SerializeField] private Transform objectDestination;
    [SerializeField] private GameObject objectTarget = null;
    private Rigidbody objectRb = null;
    private float pickUpRange = 5f;
    private float pickUpForce = 150f;

    [SerializeField] private bool interact = false;
    [SerializeField] private bool grabbing = false;

    [Header("Inventory")]
    [SerializeField] private InventorySO inventorySO = null;

    // Actions
    private PlayerInput playerInput = null;
    private InputAction moveAction = null;
    private InputAction jumpAction = null;
    private InputAction lookAction = null;
    private InputAction interactAction = null;
    private InputAction inventoryAction = null;
    private InputAction inventoryUIAction = null; // this could be done for the Pause button
    private InputAction pauseAction = null;
    private InputAction pickUpAction = null;

    // Action Maps
    public InputActionMap playerMap = null;
    public InputActionMap userInterfaceMap = null;

    private void Awake()
    {
        Application.targetFrameRate = 120;

        // Input Actions & Maps
        playerInput = GetComponent<PlayerInput>();
        playerControls = new PlayerControls();
        playerInventory = GetComponent<PlayerInventory>();
        playerStats = GetComponent<PlayerStats>();
        playerMap = playerInput.actions.FindActionMap("Player");
        userInterfaceMap = playerInput.actions.FindActionMap("UI");
    }

    private void OnEnable()
    {
        playerControls.Enable();

        // String assignment
        jumpAction = playerInput.actions["Jump"];
        interactAction = playerInput.actions["Interact"];
        inventoryAction = playerInput.actions["Inventory"];
        inventoryUIAction = playerInput.actions["InventoryUI"];
        pauseAction = playerInput.actions["Pause"];
        pickUpAction = playerInput.actions["PickUp"];

        // Subscribing to functions
        jumpAction.performed += Jump;
        interactAction.performed += Interact;
        inventoryAction.performed += Inventory;
        inventoryUIAction.performed += Inventory; // "Inventory" and "InventoryUI" both subscribe to the same function
        pauseAction.performed += Pause;
        pickUpAction.performed += context => grabbing = true;
        pickUpAction.canceled += context => grabbing = false;
    }

    private void OnDisable()
    {
        playerControls.Disable();

        // Unsubscribing to functions
        jumpAction.performed -= Jump;
        interactAction.performed -= Interact;
        inventoryAction.performed -= Inventory;
        inventoryUIAction.performed -= Inventory;
        pauseAction.performed -= Pause;
        pickUpAction.performed -= context => grabbing = true;
        pickUpAction.canceled -= context => grabbing = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        cam = Camera.main.transform;
        pauseScreen.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Player Inputs
        if (GameManager.gMan.mainMenu) { return; }
        InteractionUI();
        GroundCheck();
        JumpCheck();
        ObjectCheck();
    }

    void FixedUpdate()
    {
        Move();
        CameraRotation();

        // Picking up objects
        if (objectTarget != null)
            MoveObject();
    }

    private void Move()
    {
        if (locked) { return; }
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

    private void CameraRotation()
    {
        if (locked) { return; }
        // if there is an input
        if (GetMouseDelta().sqrMagnitude >= threshold)
        {
            //Don't multiply mouse input by Time.deltaTime
            //float deltaTimeMultiplier = isCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            cinemachineTargetPitch += GetMouseDelta().y * currRotationSpeed/* * deltaTimeMultiplier*/;
            rotationVelocity = GetMouseDelta().x * currRotationSpeed/* * deltaTimeMultiplier*/;

            // clamp our pitch rotation
            cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

            // Update Cinemachine camera target pitch
            camPos.transform.localRotation = Quaternion.Euler(cinemachineTargetPitch, 0.0f, 0.0f);

            // rotate the player left and right
            transform.Rotate(Vector3.up * rotationVelocity);
        }
    }

    private void GroundCheck()
    {
        grounded = characterController.isGrounded;
        if (grounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
            jump = false;
        }
    }

    private bool SlopeCheck()
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

    private void Jump(InputAction.CallbackContext callbackContext)
    {
        // Jump
        if (jumpTimeoutDelta <= 0.0f && jump)
        {
            // the square root of H * -2 * G = how much velocity needed to reach desired height
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
        }
    }

    private void JumpCheck()
    {
        if (grounded && !locked && !isPaused)
        {
            jump = true;

            // reset the fall timeout timer
            fallTimeoutDelta = fallTimeout;

            // stop our velocity dropping infinitely when grounded
            if (verticalVelocity < 0.0f)
            {
                verticalVelocity = -2f;
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

    public void Inventory(InputAction.CallbackContext callbackContext)
    {
        if (GameManager.gMan.mainMenu) { return; }
        if (playerInventory.inventoryUI.isActiveAndEnabled == false)
        {
            LockUser(true);
            playerInventory.inventoryUI.Show();
            foreach (var item in inventorySO.GetCurrInventoryState()) // returns a dictionary
            {
                playerInventory.inventoryUI.UpdateData(item.Key, item.Value.itemSO.ItemImage, item.Value.count);
            }
            GameManager.gMan.PlayerActionMap(false);
        }
        else
        {
            LockUser(false);
            playerInventory.inventoryUI.Hide();
            GameManager.gMan.PlayerActionMap(true);
        }
    }

    #region Item & Object Interaction

    private void ObjectCheck() // World Items
    {
        if (grabbing)
        {
            if (objectTarget == null)
            {
                RaycastHit hit;
                if (Physics.Raycast(cam.position, cam.forward.normalized, out hit, pickUpRange, interactableLayer))
                {
                    PickUpObject(hit.transform.gameObject);
                }
            }
        }
        else if (!grabbing && objectTarget != null)
        {
            DropObject();
        }
    }

    private void MoveObject()
    {
        if (Vector3.Distance(objectTarget.transform.position, objectDestination.position) > 0.1f)
        {
            Vector3 moveDirection = (objectDestination.position - objectTarget.transform.position);
            objectRb.AddForce(moveDirection * pickUpForce);
        }
    }

    private void PickUpObject(GameObject obj)
    {
        if (obj.GetComponent<Rigidbody>())
        {
            objectRb = obj.GetComponent<Rigidbody>();
            objectRb.useGravity = false;
            objectRb.drag = 10;
            objectRb.constraints = RigidbodyConstraints.FreezeRotation;

            objectRb.transform.parent = objectDestination;
            objectTarget = obj;
        }
    }

    private void DropObject()
    {
        objectRb.useGravity = true;
        objectRb.drag = 1;
        objectRb.constraints = RigidbodyConstraints.None;

        objectTarget.transform.parent = null;
        objectTarget = null;
    }

    private void Interact(InputAction.CallbackContext callbackContext) // Inventory Items
    {
        RaycastHit hit;
        const float rayLength = 5;

        Debug.DrawRay(cam.position, cam.forward.normalized * rayLength, Color.cyan);
        if (Physics.Raycast(cam.position, cam.forward.normalized, out hit, rayLength, interactableLayer))
        {
            Debug.Log("Interacting with " + hit.collider.name);
            var target = hit.transform;
            switch (target.tag)
            {
                case "AccessPoint":
                    AccessPoint accessPoint = hit.transform.GetComponent<AccessPoint>();
                    accessPoint.Interact();
                    break;
                case "Item":
                    Item item = hit.transform.GetComponent<Item>();
                    if (item != null)
                    {
                        if (playerStats.spiritRealm) { return; }
                        int remainder = inventorySO.AddItem(item.InventoryItem, item.Count);
                        if (item.GetComponent<Skull>() != null) // Use this approach to check for puzzle related 'Items'
                        {
                            item.GetComponent<Skull>().Interact();
                        }
                        if (remainder == 0)
                        {
                            item.DestroyItem();
                        }
                        else
                        {
                            item.Count = remainder;
                        }
                    }
                    break;
                case "Corpse":
                    gameObject.transform.position = hit.transform.position;
                    Destroy(target.gameObject);
                    playerStats.ToggleSpiritRealm(false, -1);
                    break;
            }
            interact = true;
        }
    }

    private void InteractionUI()
    {
        RaycastHit hit;
        const float rayLength = 5;
        const int touchSpriteIndex = 0, grabSpriteIndex = 1;

        Debug.DrawRay(cam.position, cam.forward.normalized * rayLength, Color.yellow);
        if (Physics.Raycast(cam.position, cam.forward.normalized, out hit, rayLength, interactableLayer))
        {
            Debug.Log("Examining " + hit.collider.name);
            var target = hit.transform;

            bool shouldChangeSprite = !interact; // Determine whether sprite should change
            Sprite sprite = null;

            switch (target.tag)
            {
                case "AccessPoint":
                case "Corpse":
                    sprite = interactionSprite[grabSpriteIndex];
                    break;
                case "Item":
                    if (playerStats.spiritRealm) { return; } // UI change into a grab outline?
                    sprite = interactionSprite[grabSpriteIndex];
                    break;
                case "Object":
                    if (!grabbing)
                        sprite = interactionSprite[touchSpriteIndex];
                    else
                        sprite = interactionSprite[grabSpriteIndex];
                    break;
            }
            SpriteChange(shouldChangeSprite, sprite);
        }
        else
        {
            SpriteChange(false, null);
            interact = false;
        }
    }

    private void SpriteChange(bool state, Sprite sprite)
    {
        interactionImage.sprite = sprite;
        interactionImage.enabled = state;
        crosshair.enabled = !state;
    }

    #endregion

    private void Pause(InputAction.CallbackContext callbackContext)
    {
        if (GameManager.gMan.pause)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;

        isPaused = !isPaused;
        if (isPaused)
        {
            pauseScreen.SetActive(true);
            LockUser(true);
            GameManager.gMan.PlayerActionMap(false);
        }
        else
        {
            pauseScreen.SetActive(false);
            LockUser(false);
            GameManager.gMan.PlayerActionMap(true);
        }
    }

    public void LockUser(bool state)
    {
        locked = state;
        if (state)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
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

    #endregion
}