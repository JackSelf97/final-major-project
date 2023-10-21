using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private UnityEvent _onInteract;
    UnityEvent IInteractable.onInteract { get => _onInteract; set => _onInteract = value; }

    public void Interact() => _onInteract.Invoke();
}