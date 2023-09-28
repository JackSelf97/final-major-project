using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Inventory.UI
{
    public class InventoryDescription : MonoBehaviour
    {
        [SerializeField] private Image itemImage = null;
        [SerializeField] private Text title = null;
        [SerializeField] private Text description = null;

        private void Awake()
        {
            ResetDescription();
        }

        public void ResetDescription()
        {
            itemImage.gameObject.SetActive(false);
            title.text = "";
            description.text = "";
        }

        public void SetDescription(Sprite sprite, string itemName, string itemDescription)
        {
            itemImage.gameObject.SetActive(true);
            itemImage.sprite = sprite;
            title.text = itemName;
            description.text = itemDescription;
        }
    }
}