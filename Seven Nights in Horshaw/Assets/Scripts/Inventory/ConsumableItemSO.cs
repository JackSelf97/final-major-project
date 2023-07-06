using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "New Consumable Item", menuName = "Consumable Item")]
    public class ConsumableItemSO : ItemSO, IDestroyableItem, IItemAction
    {
        [SerializeField] private List<ModifierData> modifierData = new List<ModifierData>(); // list of modifiers we want to apply by consuming the item

        public string ActionName => "Consume";

        public AudioClip actionSFX { get; private set; }

        public bool PerformAction(GameObject character)
        {
            foreach (ModifierData data in modifierData)
            {
                data.statModifierSO.AffectCharacter(character, data.value);
            }
            return true;
        }
    }

    public interface IDestroyableItem
    {

    }

    public interface IItemAction
    {
        public string ActionName { get; }
        public AudioClip actionSFX { get; }
        bool PerformAction(GameObject character);
    }

    [Serializable]
    public class ModifierData
    {
        public CharacterStatModifierSO statModifierSO;
        public float value;
    }
}