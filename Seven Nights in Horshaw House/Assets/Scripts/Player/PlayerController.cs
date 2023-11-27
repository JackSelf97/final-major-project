using Inventory;
using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour, IEntityController
{
    // Player Components
    private CharacterController characterController = null;
    private PlayerInventory playerInventory = null;
    private PlayerStats playerStats = null;
    private PlayerControls playerControls = null;

    [Header("Cinemachine")]
    [SerializeField] private Transform cam = null;
    public GameObject camPos = null;
    [SerializeField] private float rotationSpeed = 1.0f;
    [SerializeField] private float topClamp = 90.0f;
    [SerializeField] private float bottomClamp = -90.0f;
    private float verticalVelocity = 0f;
    private float cinemachineTargetPitch = 0f;
    private float rotationVelocity = 0f;
    private const float threshold = 0.01f;

    [Header("Game Properties")]
    public GameObject pauseScreen = null;
    public bool isLocked = false;
    public bool isPaused = false;
    public bool isInventoryOpen = false;
    private bool analogMovement = false;
    private float gravityValue = -9.81f;

    [Header("Player Properties")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float speedChangeRate = 10.0f;
    [SerializeField] private float fallTimeout = 0.15f;
    [SerializeField] private float jumpTimeout = 0.1f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private Vector3 direction = Vector3.zero;
    [SerializeField] private GameObject candle = null;
    [SerializeField] private bool candleLit = false;
    private bool jump;
    private bool grounded = true;
    private float pushPower = 2.0f;
    private float moveSpeed = 5f;
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
    [SerializeField] private bool interact = false;
    [SerializeField] private bool grabbing = false;
    private Rigidbody objectRb = null;
    private float pickUpRange = 5f;
    private float pickUpForce = 500f;

    [Header("Inventory")]
    [SerializeField] private InventorySO inventorySO = null;

    // Actions
    private PlayerInput playerInput = null;
    private InputAction moveAction = null;
    private InputAction jumpAction = null;
    private InputAction interactAction = null;
    private InputAction inventoryAction = null;
    private InputAction pauseAction = null;
    private InputAction flashlightAction = null;

    // Actions/UI
    private InputAction inventoryUIAction = null;
    private InputAction pauseUIAction = null;

    // Action Maps
    [HideInInspector] public InputActionMap playerMap = null;
    [HideInInspector] public InputActionMap userInterfaceMap = null;

    [Header("Hints")]
    [SerializeField] private Text interactionText = null;

    [Header("Character Sound")]
    [SerializeField] private AudioMixerGroup audioMixerGroup = null;
    [SerializeField] private AudioSource audioSource = null;
    [SerializeField] private List<AudioClip> footstepSounds = new List<AudioClip>();
    [SerializeField] private AudioClip jumpSound = null;
    [SerializeField] private AudioClip landSound = null;
    private FootstepSwapper footstepSwapper = null;
    private float lastFootstepTime;
    private float footstepDelay = 0.3f;
    private Queue<int> lastSoundsQueue = new Queue<int>();

    private void Awake()
    {
        Application.targetFrameRate = 60;
        FindInputActionAndMaps();
    }

    private void FindInputActionAndMaps()
    {
        playerInput = GetComponent<PlayerInput>();
        playerControls = new PlayerControls();
        playerMap = playerInput.actions.FindActionMap("Player");
        userInterfaceMap = playerInput.actions.FindActionMap("UI");
    }

    private void OnEnable()
    {
        playerControls.Enable();

        // String assignment
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        interactAction = playerInput.actions["Interact"];
        inventoryAction = playerInput.actions["Inventory"];
        pauseAction = playerInput.actions["Pause"];
        flashlightAction = playerInput.actions["Flashlight"];

        inventoryUIAction = playerInput.actions["InventoryUI"];
        pauseUIAction = playerInput.actions["PauseUI"];

        // Subscribing to functions
        moveAction.performed += context => Move(context.ReadValue<Vector2>());
        jumpAction.performed += Jump;

        interactAction.performed += Interact;
        interactAction.performed += context => grabbing = true;
        interactAction.canceled += context => grabbing = false;

        inventoryAction.performed += Inventory;
        pauseAction.performed += Pause;
        flashlightAction.performed += Flashlight;

        // UI
        inventoryUIAction.performed += Inventory;
        pauseUIAction.performed += Pause;
    }

    private void OnDisable()
    {
        playerControls.Disable();

        // Unsubscribing to functions
        moveAction.performed -= context => Move(context.ReadValue<Vector2>());
        jumpAction.performed -= Jump;
        interactAction.performed -= Interact;
        interactAction.performed -= context => grabbing = true;
        interactAction.canceled -= context => grabbing = false;

        inventoryAction.performed -= Inventory;
        pauseAction.performed -= Pause;
        flashlightAction.performed -= Flashlight;

        // UI
        inventoryUIAction.performed -= Inventory;
        pauseUIAction.performed -= Pause;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitialisePlayer();
    }

    private void InitialisePlayer()
    {
        characterController = GetComponent<CharacterController>();
        playerInventory = GetComponent<PlayerInventory>();
        playerStats = GetComponent<PlayerStats>();
        audioSource = GetComponent<AudioSource>();
        footstepSwapper = GetComponent<FootstepSwapper>();

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

        // Should be in FixedUpdate, however the 'StarterAsset' has this in Update.
        Movement();
    }

    void FixedUpdate()
    {
        // Picking up objects
        if (objectTarget != null)
            MoveObject();
    }

    private void LateUpdate() // CameraRotation back in LateUpdate() & Cinemachine camera changed to Smart/Late from Fixed
    {
        CameraRotation();
    }

    private void Move(Vector2 input)
    {
        direction = new Vector3(input.x, 0, input.y);
    }

    private void Movement()
    {
        if (isLocked) { return; }
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = moveSpeed;

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if (direction == Vector3.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(characterController.velocity.x, 0.0f, characterController.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = analogMovement ? direction.magnitude : 1f;

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
        Vector3 inputDirection = new Vector3(direction.x, 0.0f, direction.z).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (direction != Vector3.zero)
        {
            // move
            inputDirection = transform.right * direction.x + transform.forward * direction.z;
        }

        // Move the player
        characterController.Move(inputDirection.normalized * (moveSpeed * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

        #region Slope & Jumping

        // Slope movement
        if (direction != Vector3.zero && SlopeCheck())
        {
            characterController.Move(Vector3.down * characterController.height / 2 * slopeForce * Time.fixedDeltaTime);
        }

        // Jump effect
        if (playerVelocity.y < 0f) // if player is falling
        {
            playerVelocity += Vector3.up * gravityValue * (fallMultiplier - 1) * Time.fixedDeltaTime; // for a 'non-floaty' jump
        }

        #endregion

        Footsteps();
    }

    private void CameraRotation()
    {
        if (isLocked) { return; }

        // if there is an input
        if (GetMouseDelta().sqrMagnitude >= threshold)
        {
            //Don't multiply mouse input by Time.deltaTime
            //float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            cinemachineTargetPitch += GetMouseDelta().y * rotationSpeed * 1;
            rotationVelocity = GetMouseDelta().x * rotationSpeed * 1;

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
        if (isPaused) { return; }
        
        if (grounded && !isLocked)
        {
            jump = true;
            fallTimeoutDelta = fallTimeout;

            // Stop our velocity dropping infinitely when grounded
            verticalVelocity = Mathf.Max(-2f, verticalVelocity);

            // Jump timeout
            jumpTimeoutDelta -= Time.deltaTime;
        }
        else
        {
            // Reset the jump timeout timer
            jumpTimeoutDelta = jumpTimeout;

            // Fall timeout
            fallTimeoutDelta -= Time.deltaTime;
        }

        // Apply gravity over time if under terminal
        verticalVelocity = Mathf.Min(verticalVelocity + gravityValue * Time.deltaTime, terminalVelocity);
    }

    #region Audio

    private void Footsteps()
    {
        if (characterController.velocity.sqrMagnitude > 0 && (direction.x != 0 || direction.z != 0))
        {
            // Check if enough time has passed since the last footstep
            if (Time.time - lastFootstepTime > footstepDelay)
            {
                PlayFootstepAudio();
                lastFootstepTime = Time.time;
            }
        }
    }

    private void PlayFootstepAudio()
    {
        if (!grounded) { return; }

        footstepSwapper.CheckLayers();

        int ranNo;
        do
        {
            ranNo = Random.Range(0, footstepSounds.Count);
        } while (lastSoundsQueue.Contains(ranNo));

        audioSource.clip = footstepSounds[ranNo];

        // Set a random pitch between 1 and 2 with increments of 0.1
        float randomPitch = Mathf.Round(Random.Range(10f, 20f)) * 0.1f;
        audioSource.pitch = randomPitch;

        audioSource.PlayOneShot(audioSource.clip);
        audioSource.outputAudioMixerGroup = audioMixerGroup;

        // Enqueue the recently played sound
        lastSoundsQueue.Enqueue(ranNo);

        // Keep the queue size at 2, removing the oldest sound
        if (lastSoundsQueue.Count > 2)
        {
            lastSoundsQueue.Dequeue();
        }
    }

    public void SwapFootsteps(FootstepCollection collection)
    {
        footstepSounds.Clear();
        for (int i = 0; i < collection.footstepSpunds.Count; i++)
        {
            footstepSounds.Add(collection.footstepSpunds[i]); 
        }
        jumpSound = collection.jumpSound;
        landSound = collection.landSound;
    }


    #endregion

    private void Flashlight(InputAction.CallbackContext callbackContext)
    {
        if (candle != null)
        {
            candleLit = !candleLit;
            candle.SetActive(candleLit);
        }
    }

    public void Inventory(InputAction.CallbackContext callbackContext)
    {
        if (isPaused || GameManager.gMan.mainMenu) { return; }

        isInventoryOpen = !isInventoryOpen;
        LockUser(isInventoryOpen);

        if (isInventoryOpen)
        {
            playerInventory.inventoryUI.Show();
            foreach (var item in inventorySO.GetCurrInventoryState()) // Returns a dictionary
            {
                playerInventory.inventoryUI.UpdateData(item.Key, item.Value.itemSO.ItemImage, item.Value.count);
            }
            GameManager.gMan.PlayerActionMap(false);
        }
        else
        {
            playerInventory.inventoryUI.Hide();
            GameManager.gMan.PlayerActionMap(true);
        }

        Time.timeScale = (isInventoryOpen && GameManager.gMan.staticInventoryCheck) ? 0f : 1f;
    }

    #region Item & Object Interaction

    private void ObjectCheck()
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
        if (objectTarget && objectRb)
        {
            // Calculate the offset to move the object's origin point lower
            Vector3 offset = Vector3.up * 0.1f; // Adjust the 0.5f to your desired height

            if (Vector3.Distance(objectTarget.transform.position + offset, objectDestination.position) > 0.1f)
            {
                Vector3 moveDirection = (objectDestination.position - (objectTarget.transform.position + offset));
                objectRb.AddForce(moveDirection * pickUpForce);
            }
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
            //objectRb.transform.parent = objectDestination; // Enabling this line will cause the object to pass through other colliders. (1/2)
            objectTarget = obj;
        }
    }

    private void DropObject()
    {
        objectRb.useGravity = true;
        objectRb.drag = 1;
        objectRb.constraints = RigidbodyConstraints.None;
        //objectTarget.transform.parent = null; // The character controller could be causing this issue. (2/2)
        objectTarget = null;
    }

    private void Interact(InputAction.CallbackContext callbackContext)
    {
        RaycastHit hit;
        const float rayLength = 3;

        Debug.DrawRay(cam.position, cam.forward.normalized * rayLength, Color.cyan);
        if (!Physics.Raycast(cam.position, cam.forward.normalized, out hit, rayLength, interactableLayer))
        {
            interact = false; // No interaction if not hitting an interactable object
            return;
        }

        interact = true; // Assume interaction by default

        switch (hit.transform.tag)
        {
            case "AccessPoint":
                hit.transform.GetComponent<AccessPoint>()?.Interact();
                break;
            case "Corpse":
                gameObject.transform.position = hit.transform.position;
                Destroy(hit.transform.gameObject);
                playerStats.ToggleSpiritRealm(false, -1);
                break;
            case "Door":
                hit.transform.GetComponent<Door>()?.Interact();
                break;
            case "Item":
                Item item = hit.transform.GetComponent<Item>();
                if (item != null && !playerStats.spiritRealm)
                {
                    int remainder = inventorySO.AddItem(item.InventoryItem, item.Count);
                    if (item.GetComponent<Skull>() != null)
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
            default:
                interact = false; // Reset to false for other cases
                break;
        }
    }

    private void InteractionUI()
    {
        RaycastHit hit;
        const float rayLength = 3;
        const int touchSpriteIndex = 0, grabSpriteIndex = 1;

        Debug.DrawRay(cam.position, cam.forward.normalized * rayLength, Color.yellow);

        if (Physics.Raycast(cam.position, cam.forward.normalized, out hit, rayLength, interactableLayer))
        {
            var target = hit.transform;
            bool shouldChangeSprite = !interact;
            Sprite sprite = null;
            string prompt = "";

            switch (target.tag)
            {
                case "AccessPoint":
                    sprite = interactionSprite[touchSpriteIndex];
                    if (GameManager.gMan.HUDCheck) prompt = "Stop Time [E]";
                    break;
                case "Corpse":
                    sprite = interactionSprite[grabSpriteIndex];
                    if (GameManager.gMan.HUDCheck) prompt = "Exit Spirit Realm [E]";
                    break;
                case "Door":
                    sprite = interactionSprite[touchSpriteIndex];
                    if (GameManager.gMan.HUDCheck)
                    {
                        bool isOpen = target.GetComponent<Door>().isOpen;
                        prompt = isOpen ? "Close Door [E]" : "Open Door [E]";
                    }
                    break;
                case "Item":
                    if (playerStats.spiritRealm) return;
                    sprite = interactionSprite[grabSpriteIndex];
                    if (GameManager.gMan.HUDCheck) prompt = "Take [E]";
                    break;
                case "Object":
                    if (!grabbing)
                    {
                        sprite = interactionSprite[touchSpriteIndex];
                        if (GameManager.gMan.HUDCheck) prompt = "Pick Up [E]";
                    }
                    else sprite = interactionSprite[grabSpriteIndex];
                    break;
                default:
                    shouldChangeSprite = false;
                    break;
            }

            interactionText.text = prompt;
            SpriteChange(shouldChangeSprite, sprite);
        }
        else
        {
            interactionText.text = null;
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
        if (isInventoryOpen) { return; }
        isPaused = !isPaused;
        if (isPaused)
        {
            // Handle pause
            HandlePause(true);
        }
        else
        {
            // Handle unpause
            HandlePause(false);
        }
    }

    private void HandlePause(bool isPaused)
    {
        pauseScreen.SetActive(isPaused);
        LockUser(isPaused);
        GameManager.gMan.PlayerActionMap(!isPaused);
        Time.timeScale = isPaused && GameManager.gMan.staticPauseCheck ? 0f : 1f;
    }

    #region Other

    public void LockUser(bool state)
    {
        isLocked = state;
        if (state)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;
    }

    public void ResetPlayer()
    {
        // Reset Character Position
        if (GameManager.gMan.startPos != null)
            transform.position = GameManager.gMan.startPos.position;

        // Reset Inventory
        playerInventory.PrepareInventoryData();

        // Check for the spirit realm
        if (!playerStats.spiritRealm)
            return;
        else
        {
            Destroy(GameObject.Find("Player's Corpse"));
            playerStats.ToggleSpiritRealm(false, -1);
        }
    }

    #endregion

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
    public Vector2 GetMouseDelta()
    {
        return playerControls.Player.Look.ReadValue<Vector2>();
    }
    #endregion
}