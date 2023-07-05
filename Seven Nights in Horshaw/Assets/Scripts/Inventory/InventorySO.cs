using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory")]
    public class InventorySO : ScriptableObject
    {
        [SerializeField] private List<InventoryObj> inventoryItems;
        [field: SerializeField] public int Size { get; private set; } = 10;

        public event Action<Dictionary<int, InventoryObj>> OnInventoryChanged;

        public void Initialise()
        {
            inventoryItems = new List<InventoryObj>();
            for (int i = 0; i < Size; i++)
            {
                inventoryItems.Add(InventoryObj.GetEmptyItem());
            }
        }

        public int AddItem(ItemSO itemSO, int count)
        {
            if (!itemSO.IsStackable)
            {
                for (int i = 0; i < inventoryItems.Count; i++)
                {
                    while(count > 0 && !IsInventoryFull())
                    {
                        count -= AddItemToFirstEmptySlot(itemSO, 1);
                    }
                    InformChange();
                    return count;
                }
            }
            count = AddStackableItem(itemSO, count); // for stackable items going into the inventory
            InformChange();
            return count;
        }

        private int AddItemToFirstEmptySlot(ItemSO itemSO, int count)
        {
            InventoryObj newItem = new InventoryObj
            {
                itemSO = itemSO,
                count = count
            };

            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].IsEmpty)
                {
                    inventoryItems[i] = newItem; // assign the item to the slot
                    return count;
                }
            }
            return 0; // we haven't found an empty slot for this item
        }

        private bool IsInventoryFull() => !inventoryItems.Where(item => item.IsEmpty).Any(); // return the empty items

        private int AddStackableItem(ItemSO itemSO, int count)
        {
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].IsEmpty)
                    continue;
                if (inventoryItems[i].itemSO.ID == itemSO.ID)
                {
                    int amountPossibleToTake = inventoryItems[i].itemSO.MaxStackSize - inventoryItems[i].count; // calculate how much is possible to take on this slot before creating another slot

                    if (count > amountPossibleToTake) // more items to put into the slot
                    {
                        inventoryItems[i] = inventoryItems[i].ChangeCount(inventoryItems[i].itemSO.MaxStackSize);
                        count -= amountPossibleToTake;
                    }
                    else
                    {
                        inventoryItems[i] = inventoryItems[i].ChangeCount(inventoryItems[i].count + count);
                        InformChange();
                        return 0;
                    }
                }
            }
            while(count > 0 && !IsInventoryFull())
            {
                int newCount = Mathf.Clamp(count, 0, itemSO.MaxStackSize);
                count -= newCount;
                AddItemToFirstEmptySlot(itemSO, newCount);
            }
            return count; // if the inventory is full then leave the remaining count behind
        }

        public void AddItem(InventoryObj item)
        {
            AddItem(item.itemSO, item.count);
        }

        public Dictionary<int, InventoryObj> GetCurrInventoryState() // update UI inventory class through the player controller
        {
            Dictionary<int, InventoryObj> returnValue = new Dictionary<int, InventoryObj>();
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].IsEmpty)
                    continue;
                returnValue[i] = inventoryItems[i];
            }
            return returnValue;
        }

        public InventoryObj GetItem(int itemIndex)
        {
            return inventoryItems[itemIndex];
        }

        public void SwapItems(int itemIndex1, int itemIndex2)
        {
            InventoryObj item1 = inventoryItems[itemIndex1];
            inventoryItems[itemIndex1] = inventoryItems[itemIndex2];
            inventoryItems[itemIndex2] = item1;
            InformChange();
        }

        private void InformChange()
        {
            OnInventoryChanged?.Invoke(GetCurrInventoryState());
        }
    }

    [Serializable]
    public struct InventoryObj // a struct will be used to modify values of inventory items
    {
        public int count;
        public ItemSO itemSO;
        public bool IsEmpty => itemSO == null;

        public InventoryObj ChangeCount(int newCount) // changes the count whilst taking the same item reference
        {
            return new InventoryObj
            {
                itemSO = this.itemSO,
                count = newCount,
            };
        }

        public static InventoryObj GetEmptyItem() => new InventoryObj // structs cannot be null
        {
            itemSO = null,
            count = 0,
        };
    }

}