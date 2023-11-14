using UnityEngine;
using Cinemachine;
using Cinemachine.PostFX;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera = null;
    [SerializeField] private GameObject playerCorpse = null;
    private PlayerController playerController = null;
    private TimeManager timeManager = null;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
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
            isDead = true;
            StartCoroutine(PlayerDeath());
        }
    }

    #region Player Death & Spirit Realm

    private IEnumerator PlayerDeath()
    {
        // Begin 'Jump Scare'
        StoreOriginalCameraTransform();
        GameManager.gMan.JumpScare(true);

        yield return new WaitForSeconds(3.5f);
        
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
        transform.position = GameManager.gMan.GetPlayerSpawnPoint();
        ToggleSpiritRealm(true, 1);
    }

    public void Permadeath()
    {
        transform.position = GameManager.gMan.GetPlayerSpawnPoint();
    }

    public void ToggleSpiritRealm(bool state, float percentage)
    {
        spiritRealm = state;
        isDead = false;
        currHP = maxHP;

        if (timeManager != null)
            timeManager.timeMultiplier += (timeManager.timeScale / percentage); // Speed up time

        // Toggle the post processing layer on the player's VCam
        if (virtualCamera != null)
        {
            CinemachinePostProcessing postProcessing = virtualCamera.GetComponent<CinemachinePostProcessing>();
            postProcessing.enabled = state;
        }
    }

    #endregion

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