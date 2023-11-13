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
    public bool isDead = false;
    public bool spiritRealm = false;

    private float jumpScareTimer = 0f;
    private float jumpScareDuration = 3.5f;

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

    void Update()
    {
        if (isDead)
            PlayerDeath();
    }

    public void TakeDamage(int damage)
    {
        currHP -= damage;
        if (currHP <= 0)
        {
            isDead = true;
        }
    }

    private void PlayerDeath()
    {
        jumpScareTimer += Time.deltaTime;
        if (jumpScareTimer <= jumpScareDuration)
        {
            GameManager.gMan.JumpScare(true);
        }
        else
        {
            GameManager.gMan.JumpScare(false);

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
            jumpScareTimer = 0f;
        }
    }

    public void Permadeath()
    {
        transform.position = GameManager.gMan.GetPlayerSpawnPoint();
    }

    public void ToggleSpiritRealm(bool state, float percentage) // not sure if I should do it this way because of the percentage change
    {
        spiritRealm = state;
        isDead = false;
        currHP = maxHP;

        if (timeManager != null)
            timeManager.timeMultiplier += (timeManager.timeScale / percentage); // speed up time

        // toggle the post processing layer on the player's VCam
        if (virtualCamera != null)
        {
            CinemachinePostProcessing postProcessing = virtualCamera.GetComponent<CinemachinePostProcessing>();
            postProcessing.enabled = state;
        }
    }
}