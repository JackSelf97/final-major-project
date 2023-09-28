using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character Health Modifier", menuName = "Character Health Modifier")]
public class CharacterStatHealthModifierSO : CharacterStatModifierSO
{
    public override void AffectCharacter(GameObject character, float value)
    {
        PlayerStats playerStats = character.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            //playerStats.AddHealth((int)value);
            Debug.Log("Health +1");
        }
    }
}
