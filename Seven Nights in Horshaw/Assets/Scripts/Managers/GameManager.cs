using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SpawnPointSO spawnPointSO = null;
    [SerializeField] private UIManager UIMan = null;
    [SerializeField] private GameObject player = null;

    [Header("Respite Mechanics")]
    public bool cutscenes = true;
    public bool healthRecovery = true;
    public bool hints = true;
    public bool interactiveDialogue = true;
    public bool loadingScreens = true;
    public bool pauseScreen = true;
    public bool playerDeath = true;
    public bool quickTimeEvents = true;
    public bool safeRooms = true;
    public bool savePoints = true;
    public bool staticInventory = true;
    public bool tutorialSections = true;

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
        player = FindObjectOfType<PlayerController>().gameObject;
    }

    public Vector3 GetPlayerSpawnPoint()
    {
        int ranNo = Random.Range(0, spawnPointSO.playerSpawnPoint.Length);
        Debug.Log("Spawn point: " + ranNo);
        return spawnPointSO.playerSpawnPoint[ranNo];
    }
}