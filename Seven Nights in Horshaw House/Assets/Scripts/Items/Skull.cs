using UnityEngine;
using UnityEngine.Events;

public class Skull : MonoBehaviour, IInteractable
{
    UnityEvent IInteractable.onInteract { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public void Interact()
    {
        GameManager.gMan.collectedSkulls++;
        if (GameManager.gMan.collectedSkulls == GameManager.gMan.totalSkulls)
            GameManager.gMan.EnableEndGameState();
    }
}
