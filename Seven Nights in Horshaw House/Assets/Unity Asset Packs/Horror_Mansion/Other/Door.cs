using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Door : MonoBehaviour, IInteractable
{
    public bool isOpen = false;
    private Animator animator;

    // Navigation
    private NavMeshObstacle meshObstacle = null;

    UnityEvent IInteractable.onInteract { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        meshObstacle = GetComponent<NavMeshObstacle>();
        meshObstacle.carveOnlyStationary = false;
        meshObstacle.carving = isOpen;
        meshObstacle.enabled = isOpen;
    }

    public void Interact()
    {
        isOpen = !isOpen;
        if (isOpen)
            animator.SetBool("isOpen", true);
        else
            animator.SetBool("isOpen", false);

        meshObstacle.carving = isOpen;
        meshObstacle.enabled = isOpen;
    }

    public void ResetAnimation()
    {
        isOpen = false;
        animator.SetBool("isOpen", false);

        meshObstacle.carving = isOpen;
        meshObstacle.enabled = isOpen;
    }
}