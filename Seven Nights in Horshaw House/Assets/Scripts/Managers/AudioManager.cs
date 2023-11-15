using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    // Start is called before the first frame update
    void Awake()
    {
        foreach (Sound item in sounds)
        {
            item.audioSource = gameObject.AddComponent<AudioSource>();
            item.audioSource.clip = item.clip;
            item.audioSource.volume = item.volume;
            item.audioSource.pitch = item.pitch;
            item.audioSource.loop = item.loop;
            item.audioSource.bypassEffects = item.bypassListenerEffects;
            item.audioSource.outputAudioMixerGroup = item.mixerGroup;
        }
    }

    public void Play(string name)
    {
        Sound item = Array.Find(sounds, sound => sound.name == name);
        if (item == null) 
        {
            Debug.LogWarning("Sound: " + name + " not found! Have you spelt it correctly?");
            return; 
        }
        item.audioSource.Play();

        // 'FindObjectOfType<AudioManager>().Play("PlayerDeath");' -> Use this to trigger sound effects.
    }

    public void Stop(string name)
    {
        Sound item = Array.Find(sounds, sound => sound.name == name);
        if (item == null)
        {
            Debug.LogError("Sound: " + name + " not found! Have you spelt it correctly?");
            return;
        }
        item.audioSource.Stop();
    }

    #region UI Sound Effect Functions

    public void OnSelectSound()
    {
        Play("Selected");
    }
    public void OnPressSound()
    {
        Play("Pressed");
    }

    #endregion
}
