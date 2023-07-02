using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryPage : MonoBehaviour
{
    [SerializeField] private InventoryItem itemPrefab = null;
    [SerializeField] private RectTransform contentPanel = null;

    private List<InventoryItem> items = new List<InventoryItem>();

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

    private void HandleItemSelection(InventoryItem obj)
    {
        Debug.Log(obj.name);
    }

    private void HandleBeginDrag(InventoryItem obj)
    {
        throw new NotImplementedException();
    }

    private void HandleSwap(InventoryItem obj)
    {
        throw new NotImplementedException();
    }

    private void HandleEndDrag(InventoryItem obj)
    {
        throw new NotImplementedException();
    }

    private void HandleShowItemActions(InventoryItem obj)
    {
        throw new NotImplementedException();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
