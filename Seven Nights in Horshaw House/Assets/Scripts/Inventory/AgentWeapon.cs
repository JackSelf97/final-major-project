using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://www.youtube.com/watch?v=uKTLyjaF4hA&ab_channel=SunnyValleyStudio
public class AgentWeapon : MonoBehaviour
{
    [SerializeField] private EquippableItemSO weapon;
    [SerializeField] private InventorySO inventorySO;
    [SerializeField] private List<ItemParameter> modifiableParameters, itemCurrState;

    public void SetWeapon(EquippableItemSO weaponItemSO, List<ItemParameter> itemState)
    {
        if (weapon != null)
        {
            inventorySO.AddItem(weapon, 1, itemCurrState);
        }

        this.weapon = weaponItemSO;
        this.itemCurrState = new List<ItemParameter>(itemState);
        ModifyParameters();
    }

    private void ModifyParameters()
    {
        foreach (var parameter in modifiableParameters)
        {
            if (itemCurrState.Contains(parameter))
            {
                int index = itemCurrState.IndexOf(parameter);
                float newValue = itemCurrState[index].value + parameter.value; // if the value is negative then subtract, and if it is positive then add
                itemCurrState[index] = new ItemParameter
                {
                    itemParameter = parameter.itemParameter,
                    value = newValue
                };
            }
        }
    }
}
