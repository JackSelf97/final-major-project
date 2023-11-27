using UnityEngine;
using UnityEngine.Events;

public class Skull : MonoBehaviour, IInteractable
{
    private TimeManager timeManager = null;

    UnityEvent IInteractable.onInteract { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    private void Start()
    {
        timeManager = FindObjectOfType<TimeManager>();
    }

    public void Interact()
    {
        GameManager.gMan.collectedSkulls++;
        timeManager.SetCheckpoint();
    }
}