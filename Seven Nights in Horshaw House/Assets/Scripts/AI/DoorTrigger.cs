using UnityEngine;
using UnityEngine.AI;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private Door door = null;
    private int agentsInRange = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<NavMeshAgent>(out NavMeshAgent agent))
        {
            agentsInRange++;
            if (!door.isOpen)
            {
                door.Interact();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<NavMeshAgent>(out NavMeshAgent agent))
        {
            agentsInRange--;
            if (door.isOpen && agentsInRange == 0)
            {
                door.Interact();
            }
        }
    }
}
