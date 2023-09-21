using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SpawnPointSO spawnPointSO = null;
    [SerializeField] private UIManager UIMan = null;
    [SerializeField] private GameObject player = null;
    public bool mainMenu = true;

    [Header("Respite Mechanics")]
    public bool cutscenes = true;
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
        player = FindObjectOfType<PlayerController>().gameObject;
    }

    public Vector3 GetPlayerSpawnPoint()
    {
        int ranNo = Random.Range(0, spawnPointSO.playerSpawnPoint.Length);
        Debug.Log("Spawn point: " + ranNo);
        return spawnPointSO.playerSpawnPoint[ranNo];
    }
}