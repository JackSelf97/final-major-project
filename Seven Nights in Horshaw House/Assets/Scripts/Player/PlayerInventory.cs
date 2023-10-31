using Inventory.Model;
using Inventory.UI;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Inventory
{
    public class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController = null;
        public InventoryPage inventoryUI = null;
        [SerializeField] private InventorySO inventorySO = null;

        public List<InventoryObj> initialItems = new List<InventoryObj>();

        [SerializeField] private AudioClip dropClip;
        [SerializeField] private AudioSource audioSource;

        // Start is called before the first frame update
        void Start()
        {
            playerController = GetComponent<PlayerController>();

            // set the size of the player's inventory
            PrepareInventoryUI();
            PrepareInventoryData();
        }

        public void PrepareInventoryData()
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
            InventoryObj inventoryItem = inventorySO.GetItem(itemIndex);
            if (inventoryItem.IsEmpty)
            {
                inventoryUI.ResetSelection();
                return;
            }
            ItemSO itemSO = inventoryItem.itemSO;
            string description = PrepareDescription(inventoryItem);
            inventoryUI.UpdateDescription(itemIndex, itemSO.ItemImage, itemSO.ItemName, description);
        }

        private string PrepareDescription(InventoryObj inventoryItem)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(inventoryItem.itemSO.ItemDescription);
            sb.AppendLine();
            for (int i = 0; i < inventoryItem.itemState.Count; i++)
            {
                sb.Append($"{inventoryItem.itemState[i].itemParameter.ParameterName} : " +
                    $"{inventoryItem.itemState[i].value} / {inventoryItem.itemSO.DefaultParametersList[i].value}"); // e.g. Durability : 60 / 100
                sb.AppendLine();
            }
            return sb.ToString();
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
                inventoryUI.ShowItemAction(itemIndex);
                inventoryUI.AddAction(itemAction.ActionName, () => PerformAction(itemIndex)); // call the method when the button is pressed using a =>
            }
            IDestroyableItem destroyableItem = inventoryItem.itemSO as IDestroyableItem;
            if (destroyableItem != null)
            {
                inventoryUI.AddAction("Drop", () => DropItem(itemIndex, inventoryItem.count));
            }
        }

        private void DropItem(int itemIndex, int count)
        {
            inventorySO.RemoveItem(itemIndex, count);
            inventoryUI.ResetSelection();
            audioSource.PlayOneShot(dropClip);
        }

        public void PerformAction(int itemIndex)
        {
            InventoryObj inventoryItem = inventorySO.GetItem(itemIndex);
            if (inventoryItem.IsEmpty)
                return;
            IDestroyableItem destroyableItem = inventoryItem.itemSO as IDestroyableItem;
            if (destroyableItem != null)
            {
                inventorySO.RemoveItem(itemIndex, 1);
            }
            IItemAction itemAction = inventoryItem.itemSO as IItemAction;
            if (itemAction != null) // selected a consumable item because we have no other items in the game so far...
            {
                itemAction.PerformAction(gameObject, inventoryItem.itemState);
                audioSource.PlayOneShot(itemAction.actionSFX);
                if (inventorySO.GetItem(itemIndex).IsEmpty)
                    inventoryUI.ResetSelection();
            }
        }
    }
}