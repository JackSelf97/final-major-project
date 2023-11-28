using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Door : MonoBehaviour, IInteractable
{
    public bool isOpen = false;
    private Animator animator;
    private AudioSource audioSource = null;
    [SerializeField] private AudioClip[] audioClip = null;

    UnityEvent IInteractable.onInteract { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public void Interact()
    {
        isOpen = !isOpen;
        animator.SetBool("isOpen", isOpen);

        if (isOpen)
        {
            audioSource.PlayOneShot(audioClip[0]);
        }
        else
        {
            StartCoroutine(PlayClosedDoorSoundDelayed());
        }
    }

    private IEnumerator PlayClosedDoorSoundDelayed()
    {
        yield return new WaitForSeconds(1);
        audioSource.PlayOneShot(audioClip[1]);
    }

    public void ResetAnimation()
    {
        isOpen = false;
        animator.SetBool("isOpen", false);
    }
}