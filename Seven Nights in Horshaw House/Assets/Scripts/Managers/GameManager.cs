using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject player = null;
    [SerializeField] private GameObject enemy = null;
    private PlayerController playerController = null;
    private TimeManager timeManager = null;
    public SpawnPointSO playerSpawnPointSO = null;
    public SpawnPointSO enemySpawnPointSO = null;
    public Transform startPos = null;
    public bool mainMenu = true;

    [Header("Alpha")]
    [SerializeField] private List<GameObject> skulls = new List<GameObject>();
    [SerializeField] private SpawnPointSO skullSpawnPointSO = null;
    [SerializeField] private GameObject skullPrefab = null;
    public int totalSkulls = 8;
    public int collectedSkulls = 0;
    public KingOfTheHill kingOfTheHill = null;

    [Header("Jump Scare")]
    [SerializeField] private GameObject jumpScareMonster = null;
    [SerializeField] private float rotationSpeed = 20f;
    private GameObject playerCamPos = null;
    private Vector3 lastMonsterPos = Vector3.zero;
    private float headHeightOffset = 1.4f;
    [HideInInspector] public bool isJumpScaring = false;

    [Header("Game Over")]
    [SerializeField] private GameObject endGameScreen = null;
    [SerializeField] private Text endGameSkullCount = null;
    [SerializeField] private Text endGameHillCheck = null;
    [SerializeField] private GameObject winPrompt = null;
    [SerializeField] private GameObject defeatPromptNoSkulls = null;
    [SerializeField] private GameObject defeatPromptNoHill = null;
    [SerializeField] private GameObject defeatPromptBoth = null;
    private bool gameWon = false;

    [Header("Respite Mechanics")]
    [SerializeField] private GameObject HUDPanel = null;
    [SerializeField] private Button loadLastCheckpointButton = null;
    [SerializeField] private EnemyMelee enemyMelee = null;

    [Header("RM/Checks")]
    public bool cutsceneCheck = false;
    public bool extraHealthCheck = false;
    public bool HUDCheck = false;
    public bool dialogueCheck = false;
    public bool loadingScreenCheck = false;
    public bool staticPauseCheck = false;
    public bool deathCheck = false;
    public bool QTECheck = false;
    public bool safeRoomCheck = false;
    public bool checkpointCheck = false;
    public bool staticInventoryCheck = false;
    public bool tutorialCheck = false;

    [Header("Loading Screens")]
    public GameObject disclaimerPanel;
    public  float disclaimerDuration = 10f;
    public bool isDisclaimerOn = false;

    #region Singleton & Awake
    public static GameManager gMan = null;

    private void Awake()
    {
        if (gMan == null)
        {
            DontDestroyOnLoad(gameObject);
            gMan = this;
        }
        else if (gMan != null)
        {
            Destroy(gameObject);
        }
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        InitialiseGameManager();
        SetDefaultRespiteMechanics();
        SetSoundtracks();
    }

    private void InitialiseGameManager()
    {
        player = GameObject.FindWithTag("Player");
        enemy = GameObject.FindWithTag("Enemy");
        timeManager = FindObjectOfType<TimeManager>();
        startPos = GameObject.Find("StartPos").gameObject.transform;
        playerController = player.GetComponent<PlayerController>();
        playerCamPos = playerController.camPos;

        // Alpha
        if (endGameScreen != null)
            endGameScreen.SetActive(false);
    }

    private void SetDefaultRespiteMechanics()
    {
        HUDPanel.SetActive(false);
    }

    private void SetSoundtracks()
    {
        FindObjectOfType<AudioManager>().Play("House");
    }

    void Update()
    {
        if (isJumpScaring)
            RotateCameraTowardsMonster();
    }

    #region Player Functions

    public void GetSpawnPoint(SpawnPointSO spawnPointSO, out Vector3 position, out Vector3 rotation)
    {
        int ranNo = Random.Range(0, spawnPointSO.spawnPoint.Length);

        position = spawnPointSO.spawnPoint[ranNo];
        rotation = spawnPointSO.spawnRotation[ranNo];
    }

    private int GetRandomUnoccupiedSpawnIndex(List<int> occupiedIndices)
    {
        List<int> availableIndices = new List<int>();

        for (int i = 0; i < skullSpawnPointSO.spawnPoint.Length; i++)
        {
            if (!occupiedIndices.Contains(i))
            {
                availableIndices.Add(i);
            }
        }

        if (availableIndices.Count > 0)
        {
            int randomIndex = Random.Range(0, availableIndices.Count);
            return availableIndices[randomIndex];
        }
        else
        {
            return -1; // All spawn points are occupied
        }
    }

    public void PlayerActionMap(bool active)
    {
        if (active)
        {
            playerController.userInterfaceMap.Disable();
            playerController.playerMap.Enable();
        }
        else
        {
            playerController.playerMap.Disable();
            playerController.userInterfaceMap.Enable();
        }
    }

    private void RotateCameraTowardsMonster()
    {
        // Offset the target position to the head height
        Vector3 targetPosition = lastMonsterPos + Vector3.up * headHeightOffset;

        // Calculate the rotation needed to look at the target position
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - playerCamPos.transform.position);

        // Apply the rotation
        playerCamPos.transform.rotation = Quaternion.Slerp(playerCamPos.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    #endregion

    public void InstantiateSkulls()
    {
        if (skullSpawnPointSO == null)
        {
            Debug.LogError("SkullSpawnPointSO is not assigned.");
            return;
        }

        // Clear existing skulls and remove them from the scene
        foreach (GameObject skull in skulls)
        {
            Destroy(skull);
        }
        skulls.Clear();
        collectedSkulls = 0;

        if (totalSkulls > skullSpawnPointSO.spawnPoint.Length)
        {
            Debug.LogWarning("Number of totalSkulls: " + totalSkulls + " exceeds the total number of spawn points: " + skullSpawnPointSO.spawnPoint.Length);
            return;
        }

        List<int> occupiedSpawnIndices = new List<int>();

        for (int i = 0; i < totalSkulls; i++)
        {
            int randomSpawnIndex = GetRandomUnoccupiedSpawnIndex(occupiedSpawnIndices);

            if (randomSpawnIndex == -1)
            {
                Debug.LogWarning("All spawn points are occupied. Stopping the spawning process.");
                return;
            }

            Vector3 spawnPosition = skullSpawnPointSO.spawnPoint[randomSpawnIndex];
            Vector3 spawnRotation = skullSpawnPointSO.spawnRotation[randomSpawnIndex];

            GameObject newSkull = Instantiate(skullPrefab, spawnPosition, Quaternion.Euler(spawnRotation.x, spawnRotation.y, spawnRotation.z));
            newSkull.name = "Skull"; 

            occupiedSpawnIndices.Add(randomSpawnIndex);
            skulls.Add(newSkull);
        }
    }

    public void JumpScare(bool state)
    {
        // Rotate the camera
        lastMonsterPos = enemy.transform.position;

        // Activate the 'Jump Scare Monster'
        isJumpScaring = state;
        enemy.SetActive(!state);
        jumpScareMonster.SetActive(state);

        // Lock the player
        playerController.LockUser(state);

        // Shake the player's Vcamera
        if (isJumpScaring)
        {
            jumpScareMonster.GetComponent<JumpScare>().PlayMonsterScream();
            CameraShake.instance.ShakeCamera(2.5f, 3.5f); // Animation time
        }
    }


    public bool EnableEndGameState()
    {
        // Check if skulls are collected
        bool skullsCollected = collectedSkulls == totalSkulls;

        // Check if the slider is at max value
        bool sliderMaxed = kingOfTheHill.controlPointSlider.value == 1;

        // Check if both conditions are met
        gameWon = skullsCollected && sliderMaxed;

        Debug.Log(gameWon ? "You Win!" : "You Lose!");

        if (gameWon)
        {
            winPrompt.SetActive(true);
        }
        else
        {
            if (skullsCollected && !sliderMaxed)
                defeatPromptNoHill.SetActive(true);
            else if (!skullsCollected && sliderMaxed)
                defeatPromptNoSkulls.SetActive(true);
            else if (!skullsCollected && !sliderMaxed)
                defeatPromptBoth.SetActive(true);
        }

        endGameSkullCount.text = "Skulls Collected: " + collectedSkulls + "/" + totalSkulls;
        endGameHillCheck.text = "House Occupied: " + sliderMaxed.ToString();
        endGameScreen.SetActive(true);

        PlayerActionMap(false);
        playerController.LockUser(true);

        Time.timeScale = 0f;
        timeManager.ResetTimeOfDay();

        return gameWon;
    }

    public void ResetEndGame()
    {
        if (!endGameScreen.activeSelf) return;

        if (gMan.gameWon)
        {
            gameWon = false;
            winPrompt.SetActive(false);
        }
        else
        {
            defeatPromptNoSkulls.SetActive(false);
            defeatPromptNoHill.SetActive(false);
            defeatPromptBoth.SetActive(false);
        }

        gMan.endGameScreen.SetActive(false);
    }

    #region Respite Mechanic Functions

    public void ExtraHealth(bool value)
    {
        extraHealthCheck = value;
        enemyMelee.damage = value ? 25 : 100;
    }

    public void HUD(bool value)
    {
        HUDCheck = value;
        HUDPanel.SetActive(value);
    }

    public void LoadingScreen(bool value)
    {
        loadingScreenCheck = value;
        isDisclaimerOn = value;
    }

    public void StaticPause(bool value)
    {
        staticPauseCheck = value;
    }

    public void StaticInventory(bool value)
    {
        staticInventoryCheck = value;
    }

    public void Checkpoint(bool value)
    {
        checkpointCheck = value;
        loadLastCheckpointButton.interactable = value;
    }

    #endregion
}