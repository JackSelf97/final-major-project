using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SpawnPointSO spawnPointSO = null;

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
}
