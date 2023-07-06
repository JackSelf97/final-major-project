using Inventory.Model;
using Inventory.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    public class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController = null;
        [SerializeField] private InventoryPage inventoryUI = null;
        [SerializeField] private InventorySO inventorySO = null;

        public List<InventoryObj> initialItems = new List<InventoryObj>();

        // Start is called before the first frame update
        void Start()
        {
            playerController = GetComponent<PlayerController>();

            // set the size of the player's inventory
            PrepareInventoryUI();
            PrepareInventoryData();
        }

        private void PrepareInventoryData()
        {
            inventorySO.Initialise();
            inventorySO.OnInventoryChanged += UpdateInventoryUI;
            foreach (InventoryObj item in initialItems)
            {
                if (item.IsEmpty)
                    continue;
                inventorySO.AddItem(item);
            }
        }

        private void UpdateInventoryUI(Dictionary<int, InventoryObj> inventoryState)
        {
            inventoryUI.ResetAllItems();
            foreach (var item in inventoryState)
            {
                inventoryUI.UpdateData(item.Key, item.Value.itemSO.ItemImage, item.Value.count);
            }
        }

        // Update is called once per frame
        void Update()
        {
            Inventory();
        }

        private void PrepareInventoryUI()
        {
            inventoryUI.InitialiseInventoryUI(inventorySO.Size);
            inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
            inventoryUI.OnSwapItems += HandleSwapItems;
            inventoryUI.OnStartDragging += HandleDragging;
            inventoryUI.OnItemActionRequested += HandleItemActionRequest;
        }

        private void HandleDescriptionRequest(int itemIndex)
        {
            InventoryObj invItem = inventorySO.GetItem(itemIndex);
            if (invItem.IsEmpty)
            {
                inventoryUI.ResetSelection();
                return;
            }
            ItemSO itemSO = invItem.itemSO;
            inventoryUI.UpdateDescription(itemIndex, itemSO.ItemImage, itemSO.ItemName, itemSO.ItemDescription);
        }

        private void HandleSwapItems(int itemIndex1, int itemIndex2)
        {
            inventorySO.SwapItems(itemIndex1, itemIndex2);
        }

        private void HandleDragging(int itemIndex)
        {
            InventoryObj inventoryItem = inventorySO.GetItem(itemIndex);
            if (inventoryItem.IsEmpty)
                return;
            inventoryUI.CreateDraggedItem(inventoryItem.itemSO.ItemImage, inventoryItem.count);
        }

        private void HandleItemActionRequest(int itemIndex)
        {
            InventoryObj inventoryItem = inventorySO.GetItem(itemIndex);
            if (inventoryItem.IsEmpty)
                return;
            IItemAction itemAction = inventoryItem.itemSO as IItemAction;
            if (itemAction != null) // selected a consumable item because we have no other items in the game so far...
            {
                itemAction.PerformAction(gameObject);
            }
            IDestroyableItem destroyableItem = inventoryItem.itemSO as IDestroyableItem;
            if (destroyableItem != null)
            {
                inventorySO.RemoveItem(itemIndex, 1);
            }
        }

        public void Inventory()
        {
            if (playerController.InventoryInput())
            {
                if (inventoryUI.isActiveAndEnabled == false)
                {
                    inventoryUI.Show();
                    foreach (var item in inventorySO.GetCurrInventoryState()) // returns a dictionary
                    {
                        inventoryUI.UpdateData(item.Key, item.Value.itemSO.ItemImage, item.Value.count);
                    }
                }
                else
                {
                    inventoryUI.Hide();
                }
            }
        }
    }
}