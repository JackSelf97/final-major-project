using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    [SerializeField] private Image itemImage = null;
    [SerializeField] private Text countText = null;
    [SerializeField] private Image borderImage = null;

    // works as a delegate - does not call certain events if the item is empty
    public event Action<InventoryItem> OnItemClicked, OnItemDropped, OnItemBeginDrag, OnItemEndDrag, OnRMBClick;
    private bool empty = true;

    public void Awake()
    {
        ResetData();
        Deselect();
    }

    public void ResetData()
    {
        itemImage.gameObject.SetActive(false);
        empty = true;
    }

    public void Deselect()
    {
        borderImage.enabled = false;
    }

    public void SetData(Sprite sprite, int count)
    {
        itemImage.gameObject.SetActive(true);
        itemImage.sprite = sprite;
        countText.text = count.ToString();
        empty = false;
    }

    public void Select()
    {
        borderImage.enabled = true;
    }

    public void OnBeginDrag()
    {
        if (empty) { return; }
        OnItemBeginDrag?.Invoke(this);
    }

    public void OnDrop()
    {
        OnItemDropped?.Invoke(this);
    }

    public void OnEndDrag()
    {
        OnItemEndDrag?.Invoke(this);
    }

    public void OnPointerClick(BaseEventData data)
    {
        if (empty) { return; }
        PointerEventData pointerData = (PointerEventData)data;
        if (pointerData.button == PointerEventData.InputButton.Right)
        {
            OnRMBClick?.Invoke(this);
        }
        else
        {
            OnItemClicked?.Invoke(this);
        }
    }
}
