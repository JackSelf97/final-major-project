using UnityEngine;
using UnityEngine.Events;

public class Note : MonoBehaviour, IInteractable
{
    private AudioManager audioManager = null;

    UnityEvent IInteractable.onInteract { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
    }

    public void Interact()
    {
        audioManager.Play("Note Pick Up");
    }
}