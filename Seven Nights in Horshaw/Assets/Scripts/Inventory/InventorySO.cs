using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory")]
    public class InventorySO : ScriptableObject
    {
        [SerializeField] private List<InvItem> inventoryItems;
        [field: SerializeField] public int Size { get; private set; } = 10;

        public void Initialise()
        {
            inventoryItems = new List<InvItem>();
            for (int i = 0; i < Size; i++)
            {
                inventoryItems.Add(InvItem.GetEmptyItem());
            }
        }

        public void AddItem(ItemSO itemSO, int count)
        {
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].IsEmpty)
                {
                    inventoryItems[i] = new InvItem
                    {
                        itemSO = itemSO,
                        count = count
                    };
                }
            }
        }

        public Dictionary<int, InvItem> GetCurrInventoryState() // update UI inventory class through the player controller
        {
            Dictionary<int, InvItem> returnValue = new Dictionary<int, InvItem>();
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].IsEmpty)
                    continue;
                returnValue[i] = inventoryItems[i];
            }
            return returnValue;
        }

        public InvItem GetItem(int itemIndex)
        {
            return inventoryItems[itemIndex];
        }
    }

    [Serializable]
    public struct InvItem // a struct will be used to modify values of inventory items
    {
        public int count;
        public ItemSO itemSO;
        public bool IsEmpty => itemSO == null;

        public InvItem ChangeCount(int newCount)
        {
            return new InvItem
            {
                itemSO = this.itemSO,
                count = newCount,
            };
        }

        public static InvItem GetEmptyItem() => new InvItem // structs cannot be null
        {
            itemSO = null,
            count = 0,
        };
    }

}