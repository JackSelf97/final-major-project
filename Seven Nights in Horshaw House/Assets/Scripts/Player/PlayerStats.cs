using UnityEngine;
using Cinemachine;
using Cinemachine.PostFX;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private GameObject playerCorpse = null;
    private PlayerController playerController = null;

    // Time Control
    private TimeManager timeManager = null;
    private float originalTimeMultiplier;

    // Camera Properties
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private CinemachineVirtualCamera virtualCamera = null;

    [Header("Player State")]
    private int maxHP = 100;
    public int currHP = 0;
    public bool isDead = false;
    public bool spiritRealm = false;

    // Start is called before the first frame update
    void Start()
    {
        InitialisePlayerStats(); 
    }

    private void InitialisePlayerStats()
    {
        playerController = GetComponent<PlayerController>();
        timeManager = FindObjectOfType<TimeManager>();
        currHP = maxHP;

        virtualCamera = GameObject.Find("CM vcam [Player]")?.GetComponent<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            var postProcessing = virtualCamera.GetComponent<CinemachinePostProcessing>();
            if (postProcessing != null)
            {
                postProcessing.enabled = false;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currHP -= damage;
        if (currHP <= 0)
        {
            StartCoroutine(PlayerDeath());
        }
    }

    #region Spirit Realm

    private IEnumerator PlayerDeath()
    {
        // Begin 'Jump Scare'
        StoreOriginalCameraTransform();
        GameManager.gMan.JumpScare(true);

        yield return new WaitForSeconds(3.5f);

        // Check to see if the player has restarted or exited the game during the jump scare
        if (GameManager.gMan.hasGameRestarted)
        {
            GameManager.gMan.hasGameRestarted = false;
            yield break;
        }

        GameManager.gMan.JumpScare(false);
        ResetCameraTransform();

        if (GameObject.Find("Player's Corpse"))
        {
            GameObject instance = GameObject.Find("Player's Corpse");
            timeManager.timeMultiplier = timeManager.timeScale;
            Destroy(instance);
        }
        var corpsePos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        GameObject corpseObj = Instantiate(playerCorpse, corpsePos, Quaternion.identity);
        corpseObj.name = "Player's Corpse";

        // Reset the player's position and toggle the spirit realm
        SetRandomPositionAndRotation();
        ToggleSpiritRealm(true, 20);
    }

    public void ToggleSpiritRealm(bool state, float percentageIncrease)
    {
        spiritRealm = state;
        isDead = state;
        currHP = maxHP;

        Debug.Log("Time was: " + timeManager.timeMultiplier);

        if (timeManager != null)
        {
            if (state) // Entering spirit realm
            {
                // Store the original timeMultiplier
                originalTimeMultiplier = timeManager.timeMultiplier;

                // Calculate the increase based on the original timeMultiplier and the specified percentage increase
                float increase = originalTimeMultiplier * (percentageIncrease / 100f);

                // Apply the increase
                timeManager.timeMultiplier += increase;
            }
            else // Exiting spirit realm
            {
                // Decrease back to the original timeMultiplier
                timeManager.timeMultiplier = originalTimeMultiplier;
            }
        }

        Debug.Log("Time is: " + timeManager.timeMultiplier);

        // Toggle the post-processing layer on the player's VCam
        if (virtualCamera != null)
        {
            CinemachinePostProcessing postProcessing = virtualCamera.GetComponent<CinemachinePostProcessing>();
            postProcessing.enabled = state;
        }

        GameManager.gMan.UpdateEmissionMaterial(state);
    }

    #endregion

    public void SetRandomPositionAndRotation()
    {
        Vector3 playerSpawnPosition;
        Vector3 playerSpawnRotation;
        GameManager.gMan.GetSpawnPoint(GameManager.gMan.playerSpawnPointSO, out playerSpawnPosition, out playerSpawnRotation);
        transform.position = playerSpawnPosition;
        transform.rotation = Quaternion.Euler(playerSpawnRotation.x, playerSpawnRotation.y, playerSpawnRotation.z);
    }

    public void StoreOriginalCameraTransform()
    {
        originalCameraPosition = playerController.camPos.transform.position;
        originalCameraRotation = playerController.camPos.transform.rotation;
    }

    public void ResetCameraTransform()
    {
        playerController.camPos.transform.position = originalCameraPosition;
        playerController.camPos.transform.rotation = originalCameraRotation;
    }
}