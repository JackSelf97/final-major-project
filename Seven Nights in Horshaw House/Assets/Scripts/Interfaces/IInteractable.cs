using UnityEngine.Events;

public interface IInteractable
{
    public UnityEvent onInteract { get; protected set; }
    public void Interact();
}