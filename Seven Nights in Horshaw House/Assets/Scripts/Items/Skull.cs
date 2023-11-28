using UnityEngine;
using UnityEngine.Events;

public class Skull : MonoBehaviour, IInteractable
{
    private TimeManager timeManager = null;
    private AudioManager audioManager = null;

    UnityEvent IInteractable.onInteract { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    private void Start()
    {
        timeManager = FindObjectOfType<TimeManager>();
        audioManager = FindObjectOfType<AudioManager>();
    }

    public void Interact()
    {
        GameManager.gMan.collectedSkulls++;
        audioManager.Play("Skull Pick Up");
        timeManager.SetCheckpoint();
    }
}