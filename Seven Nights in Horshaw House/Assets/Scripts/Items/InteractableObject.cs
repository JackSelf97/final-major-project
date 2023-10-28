using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private UnityEvent _onInteract;
    UnityEvent IInteractable.onInteract { get => _onInteract; set => _onInteract = value; }

    public void Interact() => _onInteract.Invoke();

    private Vector3 originalPos = Vector3.zero;
    private Quaternion originalRot = Quaternion.identity;

    private void Start()
    {
        originalPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        originalRot = transform.rotation;
    }

    public void ResetPositionAndRotation()
    {
        transform.position = originalPos;
        transform.rotation = originalRot;
    }
}