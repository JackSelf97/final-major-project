using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    public abstract class ItemSO : ScriptableObject
    {
        [field: SerializeField] public bool IsStackable { get; set; }
        public int ID => GetInstanceID();
        [field: SerializeField] public int MaxStackSize { get; set; } = 1;
        [field: SerializeField] public string ItemName { get; set; }

        [field: TextArea(15, 20)]
        [field: SerializeField] public string ItemDescription { get; set; }
        [field: SerializeField] public Sprite ItemImage { get; set; }

        [field: SerializeField] public List<ItemParameter> DefaultParametersList { get; set; }
    }

    [Serializable]
    public struct ItemParameter : IEquatable<ItemParameter> // compare two items
    {
        public ItemParameterSO itemParameter;
        public float value;

        public bool Equals(ItemParameter other) // check if one item parameter is equal to another
        {
            return other.itemParameter == itemParameter;
        }
    }
}