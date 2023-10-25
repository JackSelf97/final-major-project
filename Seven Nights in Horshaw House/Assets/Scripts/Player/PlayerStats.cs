using UnityEngine;
using Cinemachine;
using Cinemachine.PostFX;

public class PlayerStats : MonoBehaviour
{
    private PlayerController playerController = null;
    private TimeManager timeManager = null;
    [SerializeField] private CinemachineVirtualCamera virtualCamera = null;
    [SerializeField] private GameObject playerCorpse = null;
    public int currHP = 0, maxHP = 100;
    public bool spiritRealm = false;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        timeManager = FindObjectOfType<TimeManager>();
        currHP = maxHP;

        virtualCamera = GameObject.Find("CM vcam [Player]").GetComponent<CinemachineVirtualCamera>();
        if (virtualCamera != null)
            virtualCamera.GetComponent<CinemachinePostProcessing>().enabled = false;
    }

    public void TakeDamage(int damage)
    {
        if (!playerController.locked)
        {
            currHP -= damage;
            if (currHP <= 0)
            {
                if (GameObject.Find("Player's Corpse"))
                {
                    GameObject instance = GameObject.Find("Player's Corpse");
                    timeManager.timeMultiplier = timeManager.timeScale;
                    Destroy(instance);
                }
                var corpsePos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                GameObject corpseObj = Instantiate(playerCorpse, corpsePos, Quaternion.identity);
                corpseObj.name = "Player's Corpse";
                transform.position = GameManager.gMan.GetPlayerSpawnPoint();
                ToggleSpiritRealm(true, 1);
            }
        }
    }

    public void ToggleSpiritRealm(bool state, float percentage) // not sure if I should do it this way because of the percentage change
    {
        spiritRealm = state;
        timeManager.timeMultiplier += (timeManager.timeScale / percentage); // speed up time

        // toggle the post processing layer on the player's VCam
        if (virtualCamera != null)
        {
            CinemachinePostProcessing postProcessing = virtualCamera.GetComponent<CinemachinePostProcessing>();
            postProcessing.enabled = state;
        }
    }
}