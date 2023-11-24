using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public SpawnPointSO playerSpawnPointSO = null;
    public SpawnPointSO enemySpawnPointSO = null;

    [SerializeField] private GameObject player = null;
    [SerializeField] private GameObject enemy = null;
    private PlayerController playerController = null;
    public Transform startPos = null;
    public bool mainMenu = true;

    [Header("Proof of Concept")]
    [SerializeField] private List<GameObject> skulls = new List<GameObject>();
    [SerializeField] private SpawnPointSO skullSpawnPointSO = null;
    [SerializeField] private GameObject skullPrefab = null;
    public int totalSkulls = 8;
    public int collectedSkulls = 0;

    [Header("Jump Scare")]
    [SerializeField] private GameObject jumpScareMonster = null;
    [SerializeField] private float rotationSpeed = 20f;
    private GameObject playerCamPos = null;
    private Vector3 lastMonsterPos = Vector3.zero;
    private float headHeightOffset = 1.4f;
    public bool isJumpScaring = false;

    [Header("Game State")]
    public GameObject endGamePanel = null;
    public bool gameWon = false;

    [Header("Respite Mechanics")]
    public GameObject HUDPanel = null;

    [Header("RM/Checks")]
    public bool cutscenesCheck = false; // Maybe set them to their toggle?
    public bool healthCheck = false;
    public bool HUDCheck = false;
    public bool dialogueCheck = false;
    public bool loadCheck = false;
    public bool staticPauseCheck = false;
    public bool deathCheck = false;
    public bool quickTimeEventsCheck = false;
    public bool safeRoomsCheck = false;
    public bool savePointsCheck = false;
    public bool staticInventoryCheck = false;
    public bool tutorialCheck = false;

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
        SetSoundTracks();
    }

    private void InitialiseGameManager()
    {
        player = GameObject.FindWithTag("Player");
        startPos = GameObject.Find("StartPos").gameObject.transform;
        playerController = player.GetComponent<PlayerController>();
        playerCamPos = playerController.camPos;

        // Proof of Concept
        InstantiateSkulls();
        if (endGamePanel != null)
            endGamePanel.SetActive(false);
    }

    private void SetDefaultRespiteMechanics()
    {
        HUDPanel.SetActive(false);
    }

    private void SetSoundTracks()
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

        //Debug.Log("Spawn point: " + ranNo);
        //Debug.Log("Spawn position: " + position);
        //Debug.Log("Spawn rotation: " + rotation);
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
            CameraShake.instance.ShakeCamera(2.5f, 3.5f); // Animation time
            jumpScareMonster.GetComponent<JumpScare>().PlayMonsterScream();
        }
    }

    public bool EnableEndGameState()
    {
        gameWon = collectedSkulls == totalSkulls;

        if (gameWon)
        {
            Debug.Log("You Win!");
            endGamePanel.transform.GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("You Lose!");
            endGamePanel.transform.GetChild(2).gameObject.SetActive(true);
        }

        endGamePanel.SetActive(true);
        PlayerActionMap(false);
        playerController.LockUser(true);

        return gameWon;
    }
}