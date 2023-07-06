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

        [field: TextArea]
        [field: SerializeField] public string ItemDescription { get; set; }
        [field: SerializeField] public Sprite ItemImage { get; set; }
        [field: SerializeField] public Mesh ItemMesh { get; set; }
    }
}