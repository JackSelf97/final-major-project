using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMelee : MonoBehaviour
{
    [SerializeField] private int damage = 100;
    [SerializeField] private PlayerStats playerStats = null;
    private EnemyController enemyController = null;

    private void Start()
    {
        enemyController = GetComponentInParent<EnemyController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerStats.TakeDamage(damage);
            if (playerStats.currHP <= 0)
                enemyController.chasing = false;
        }
    }
}