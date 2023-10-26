using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SpawnPointSO playerSpawnPointSO = null;
    [SerializeField] private GameObject player = null;
    private PlayerController playerController = null;
    public Transform startPos = null;
    public bool mainMenu = true;

    [Header("Proof of Concept")]
    [SerializeField] private List<GameObject> skulls = new List<GameObject>();
    [SerializeField] private SpawnPointSO skullSpawnPointSO = null;
    [SerializeField] private GameObject skullPrefab = null;
    [SerializeField] private int totalSkulls = 8;
    public int collectedSkulls = 0;
    public bool gameWon = false;

    [Header("Respite Mechanics")]
    public bool cutscenes = true; // rename to 'CHECK'
    public bool health = true;
    public bool hints = true;
    public bool dialogue = true;
    public bool load = true;
    public bool pause = true;
    public bool death = true;
    public bool quickTimeEvents = true;
    public bool safeRooms = true;
    public bool savePoints = true;
    public bool staticInventory = true;
    public bool tutorial = true;

    #region Singleton & Awake
    public static GameManager gMan = null; // should always initilize

    private void Awake()
    {
        if (gMan == null)
        {
            DontDestroyOnLoad(gameObject);
            gMan = this;
        }
        else if (gMan != null)
        {
            Destroy(gameObject); // if its already there destroy it
        }
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        startPos = GameObject.Find("StartPos").gameObject.transform;
        playerController = player.GetComponent<PlayerController>();

        // Proof of Concept
        InstantiateSkulls();
    }

    #region Player Functions

    public Vector3 GetPlayerSpawnPoint()
    {
        int ranNo = Random.Range(0, playerSpawnPointSO.spawnPoint.Length);
        Debug.Log("Spawn point: " + ranNo);
        return playerSpawnPointSO.spawnPoint[ranNo];
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

    #endregion

    public void InstantiateSkulls()
    {
        if (skullSpawnPointSO == null)
        {
            Debug.LogError("SkullSpawnPointSO is not assigned.");
            return;
        }

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
            GameObject newSkull = Instantiate(skullPrefab, spawnPosition, Quaternion.identity);

            occupiedSpawnIndices.Add(randomSpawnIndex);
            skulls.Add(newSkull);
        }
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

    public bool CheckWinState()
    {
        if (collectedSkulls == totalSkulls)
        {
            Debug.Log("You Win!");
            return gameWon = true;
        }
        else
        {
            return gameWon = false;
        }
    }

    public void CheckLoseState()
    {
        if (!gameWon)
        {
            Debug.Log("You Lose!");
        }
    }
}