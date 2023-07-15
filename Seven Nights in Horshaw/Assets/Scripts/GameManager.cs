using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SpawnPointSO spawnPointSO = null;
    [SerializeField] private GameObject player = null;

    [Header("UI")]
    [SerializeField] private Animator animator;
    public bool mainMenu = true;
    [SerializeField] private GameObject playerCanvas = null, menuCanvas = null;

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

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 GetPlayerSpawnPoint()
    {
        int ranNo = Random.Range(0, spawnPointSO.playerSpawnPoint.Length);
        Debug.Log("Spawn point: " + ranNo);
        return spawnPointSO.playerSpawnPoint[ranNo];
    }

    public void SwitchState()
    {
        if (mainMenu)
        {
            animator.Play("Player");
            menuCanvas.SetActive(false);
            playerCanvas.SetActive(true);
            player.GetComponent<PlayerController>().LockUser(false);
        }
        else
        {
            animator.Play("Main Menu");
            playerCanvas.SetActive(false);
            menuCanvas.SetActive(true);
            player.GetComponent<PlayerController>().LockUser(true);
        }
        mainMenu = !mainMenu;
    }
}
