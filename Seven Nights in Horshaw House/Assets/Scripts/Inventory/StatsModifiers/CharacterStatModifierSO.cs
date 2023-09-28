using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterStatModifierSO : ScriptableObject
{
    public abstract void AffectCharacter(GameObject character, float value); // make it a generic float value so that it can be used for other instances
}
