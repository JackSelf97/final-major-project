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
        [SerializeField] private InventoryPage inventoryPage = null;
        [SerializeField] private InventorySO inventorySO = null;

        // Start is called before the first frame update
        void Start()
        {
            playerController = GetComponent<PlayerController>();

            // set the size of the player's inventory
            PrepareInventoryUI();
            //inventorySO.Initialise();
        }

        // Update is called once per frame
        void Update()
        {
            Inventory();
        }

        private void PrepareInventoryUI()
        {
            inventoryPage.InitialiseInventoryUI(inventorySO.Size);
            inventoryPage.OnDescriptionRequested += HandleDescriptionRequest;
            inventoryPage.OnSwapItems += HandleSwapItems;
            inventoryPage.OnStartDragging += HandleDragging;
            inventoryPage.OnItemActionRequested += HandleItemActionRequest;
        }

        private void HandleDescriptionRequest(int itemIndex)
        {
            InvItem invItem = inventorySO.GetItem(itemIndex);
            if (invItem.IsEmpty)
            {
                inventoryPage.ResetSelection();
                return;
            }
            ItemSO itemSO = invItem.itemSO;
            inventoryPage.UpdateDescription(itemIndex, itemSO.ItemImage, itemSO.ItemName, itemSO.ItemDescription);
        }

        private void HandleSwapItems(int itemIndex1, int itemIndex2)
        {

        }

        private void HandleDragging(int itemIndex)
        {

        }

        private void HandleItemActionRequest(int itemIndex)
        {

        }

        public void Inventory()
        {
            if (playerController.InventoryInput())
            {
                if (inventoryPage.isActiveAndEnabled == false)
                {
                    inventoryPage.Show();
                    foreach (var item in inventorySO.GetCurrInventoryState()) // returns a dictionary
                    {
                        inventoryPage.UpdateData(item.Key, item.Value.itemSO.ItemImage, item.Value.count);
                    }
                }
                else
                {
                    inventoryPage.Hide();
                }
            }
        }
    }
}