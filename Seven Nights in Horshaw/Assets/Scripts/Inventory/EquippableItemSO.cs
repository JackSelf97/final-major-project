using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "New Equippable Item", menuName = "Equippable Item")]
    public class EquippableItemSO : ItemSO, IDestroyableItem, IItemAction
    {
        public string ActionName => "Equip";

        [field: SerializeField] public AudioClip actionSFX { get; private set; }

        public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
        {
            AgentWeapon weaponSystem = character.GetComponent<AgentWeapon>();
            if (weaponSystem != null)
            {
                weaponSystem.SetWeapon(this, itemState == null ? DefaultParametersList : itemState);
                return true;
            }
            return false; // if your character cannot equip the item for some in-game reason then add some code here
        }
    }
}
