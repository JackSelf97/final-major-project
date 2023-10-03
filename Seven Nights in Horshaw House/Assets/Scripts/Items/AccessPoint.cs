using UnityEngine;
using UnityEngine.Events;

public class AccessPoint : MonoBehaviour, IInteractable
{
    public TimeManager timeManager = null;
    public bool isTimePaused = false;

    UnityEvent IInteractable.onInteract { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    //[SerializeField] private UnityEvent _onInteract;
    //UnityEvent IInteractable.onInteract { get => _onInteract; set => _onInteract = value; }

    public void Interact()
    {
        isTimePaused = !isTimePaused;
    }

    //public void Interact() => _onInteract.Invoke();

    // Start is called before the first frame update
    void Start()
    {
        timeManager = FindObjectOfType<TimeManager>();
    }
}