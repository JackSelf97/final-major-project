using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private PlayerController playerController = null;
    private CharacterController characterController = null;
    [SerializeField] private GameObject playerCorpse = null;
    public int currHP = 0, maxHP = 100;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        characterController = GetComponent<CharacterController>();
        currHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        if (!playerController.lockInput)
        {
            currHP -= damage;
            if (currHP <= 0)
            {
                if (GameObject.Find("Player's Body"))
                {
                    GameObject instance = GameObject.Find("Player's Body");
                    Destroy(instance);
                }
                var corpsePos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                GameObject corpseObj = Instantiate(playerCorpse, corpsePos, Quaternion.identity);
                corpseObj.name = "Player's Body";
                transform.position = GameManager.gMan.GetPlayerSpawnPoint();
            }
        }
    }
}
