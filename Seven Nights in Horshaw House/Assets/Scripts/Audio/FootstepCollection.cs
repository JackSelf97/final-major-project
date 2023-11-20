using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Footstep Collection", menuName = "Footstep Collection")]
public class FootstepCollection : ScriptableObject
{
    public List<AudioClip> footstepSpunds = new List<AudioClip>();
    public AudioClip jumpSound;
    public AudioClip landSound;
}