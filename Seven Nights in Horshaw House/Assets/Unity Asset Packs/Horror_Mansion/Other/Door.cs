using UnityEngine;
using UnityEngine.Events;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] private bool isOpen = false;
    private Animator animator;

    UnityEvent IInteractable.onInteract { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Interact()
    {
        isOpen = !isOpen;
        if (isOpen)
            animator.SetBool("isOpen", true);
        else
            animator.SetBool("isOpen", false);
    }
}