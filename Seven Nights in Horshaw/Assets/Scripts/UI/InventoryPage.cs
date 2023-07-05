using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.UI
{
    public class InventoryPage : MonoBehaviour
    {
        [SerializeField] private InventoryItem itemPrefab = null;
        [SerializeField] private RectTransform contentPanel = null;
        [SerializeField] private InventoryDescription itemDescription = null;

        [SerializeField] private MouseFollower mouseFollower = null;

        private List<InventoryItem> items = new List<InventoryItem>();

        private int currDraggedItemIndex = -1; // sits outside the bounds of the array

        public event Action<int> OnDescriptionRequested, OnItemActionRequested, OnStartDragging;
        public event Action<int, int> OnSwapItems; // dragged item and the item you want to swap with

        private void Awake()
        {
            Hide();
            mouseFollower.Toggle(false);
            itemDescription.ResetDescription();
        }

        public void InitialiseInventoryUI(int inventorySize)
        {
            for (int i = 0; i < inventorySize; i++)
            {
                InventoryItem item = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
                item.transform.SetParent(contentPanel, false);
                items.Add(item);

                // subscribing to events
                item.OnItemClicked += HandleItemSelection;
                item.OnItemBeginDrag += HandleBeginDrag;
                item.OnItemDropped += HandleSwap;
                item.OnItemEndDrag += HandleEndDrag;
                item.OnRMBClick += HandleShowItemActions;
            }
        }

        public void UpdateData(int itemIndex, Sprite itemImage, int itemCount)
        {
            if (items.Count > itemIndex)
            {
                items[itemIndex].SetData(itemImage, itemCount);
            }
        }

        private void HandleItemSelection(InventoryItem inventoryItem)
        {
            int index = items.IndexOf(inventoryItem);
            if (index == -1) { return; }
            OnDescriptionRequested?.Invoke(index); // update the description if the item is the player list
        }

        private void HandleBeginDrag(InventoryItem inventoryItem)
        {
            int index = items.IndexOf(inventoryItem); // accessing the inventory model
            if (index == -1) { return; }
            currDraggedItemIndex = index;
            HandleItemSelection(inventoryItem); // select the item when dragging
            OnStartDragging?.Invoke(index);
        }

        public void CreateDraggedItem(Sprite sprite, int count)
        {
            mouseFollower.Toggle(true);
            mouseFollower.SetData(sprite, count);
        }

        private void HandleSwap(InventoryItem inventoryItem)
        {
            int index = items.IndexOf(inventoryItem);
            if (index == -1)
            {
                return;
            }
            OnSwapItems?.Invoke(currDraggedItemIndex, index);
        }

        private void ResetDraggedItem()
        {
            mouseFollower.Toggle(false);
            currDraggedItemIndex = -1;
        }

        private void HandleEndDrag(InventoryItem inventoryItem)
        {
            ResetDraggedItem();
        }

        private void HandleShowItemActions(InventoryItem inventoryItem)
        {

        }

        public void Show()
        {
            gameObject.SetActive(true);
            ResetSelection();
        }

        public void ResetSelection()
        {
            itemDescription.ResetDescription();
            DeselectAllItems();
        }

        private void DeselectAllItems()
        {
            foreach (InventoryItem item in items)
            {
                item.Deselect();
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            ResetDraggedItem();
        }

        internal void UpdateDescription(int itemIndex, Sprite itemImage, string name, string description)
        {
            itemDescription.SetDescription(itemImage, name, description);
            DeselectAllItems();
            items[itemIndex].Select();
        }
    }
}