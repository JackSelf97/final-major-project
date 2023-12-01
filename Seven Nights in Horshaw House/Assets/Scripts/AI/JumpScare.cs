using System.Collections.Generic;
using UnityEngine;

public class JumpScare : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource = null;
    [SerializeField] private List<AudioClip> jumpScareSounds = new List<AudioClip>();

    public void PlayMonsterScream()
    {
        if (audioSource != null && jumpScareSounds.Count > 0)
        {
            // Choose a random sound from the jumpScareSounds list
            int randomIndex = Random.Range(0, jumpScareSounds.Count);
            audioSource.clip = jumpScareSounds[randomIndex];

            // Play the selected sound
            audioSource.PlayOneShot(audioSource.clip);
        }
    }

    public void StopMonsterScream()
    {
        audioSource.Stop();
    }
}