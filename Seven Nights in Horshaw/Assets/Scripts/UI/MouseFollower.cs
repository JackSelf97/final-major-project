using Inventory.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFollower : MonoBehaviour
{
    [SerializeField] private Canvas canvas = null;
    [SerializeField] private InventoryItem item = null;
    private PlayerController playerController = null;

    private void Awake()
    {
        canvas = transform.root.GetComponent<Canvas>();
        item = GetComponentInChildren<InventoryItem>();
        playerController = FindObjectOfType<PlayerController>();
    }

    public void SetData(Sprite sprite, int count) // set the image and stack count of the dragged object
    {
        item.SetData(sprite, count);
    }

    void Update()
    {
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, Input.mousePosition, canvas.worldCamera, out position);
        transform.position = canvas.transform.TransformPoint(position);
    }

    public void Toggle(bool value)
    {
        Debug.Log($"Item toggled {value}");
        gameObject.SetActive(value);
    }
}
