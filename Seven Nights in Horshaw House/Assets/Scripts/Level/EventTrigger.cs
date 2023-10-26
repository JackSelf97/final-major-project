using UnityEngine;

public class EventTrigger : MonoBehaviour
{
    private enum Trigger
    { 
        KillBox,
    }

    [SerializeField] private Trigger trigger;

    private void OnTriggerEnter(Collider other)
    {
        if (trigger == Trigger.KillBox)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<PlayerStats>().Permadeath();
            }
        }
    }
}