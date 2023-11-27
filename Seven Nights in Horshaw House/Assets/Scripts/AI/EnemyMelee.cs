using UnityEngine;

public class EnemyMelee : MonoBehaviour
{
    public int damage = 100;
    private GameObject player = null;
    private PlayerStats playerStats = null;
    
    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerStats = player.GetComponent<PlayerStats>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!playerStats.spiritRealm)
                playerStats.TakeDamage(damage);
        }
    }
}