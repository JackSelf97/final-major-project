using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private PlayerController playerController = null;
    private CharacterController characterController = null;
    [SerializeField] private GameObject playerCorpse = null;
    [SerializeField] private int currHP = 0, maxHP = 100;

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
                var corpsePos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                Instantiate(playerCorpse, corpsePos, Quaternion.identity);
                transform.position = GameManager.gMan.GetPlayerSpawnPoint();
            }
        }
    }
}
