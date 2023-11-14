using UnityEngine;

public class EnemyMelee : MonoBehaviour
{
    [SerializeField] private int damage = 25;
    private GameObject player = null;
    private PlayerStats playerStats = null;
    private EnemyController enemyController = null;
    
    private void Start()
    {
        enemyController = GetComponentInParent<EnemyController>();
        player = GameObject.FindWithTag("Player");
        playerStats = player.GetComponent<PlayerStats>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!playerStats.isDead)
                playerStats.TakeDamage(damage);

            if (playerStats.currHP <= 0)
                enemyController.chasing = false;
        }
    }
}