using UnityEngine;
using UnityEngine.Events;

public class AccessPoint : MonoBehaviour, IInteractable
{
    public TimeManager timeManager = null;
    public bool isTimePaused = false;

    UnityEvent IInteractable.onInteract { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    // Check Tag and Layer to enable interaction
    public void Interact()
    {
        isTimePaused = !isTimePaused;
        Debug.LogWarning("Please ensure there is only one 'AccessPoint' active at a time. Check TimeManager to identify the active one.");
    }

    // Start is called before the first frame update
    void Start()
    {
        timeManager = FindObjectOfType<TimeManager>();
    }
}