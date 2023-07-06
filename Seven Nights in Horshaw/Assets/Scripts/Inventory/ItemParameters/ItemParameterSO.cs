using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "New Item Parameter", menuName = "Item Parameter")]
    public class ItemParameterSO : ScriptableObject
    {
        [field: SerializeField] public string ParameterName { get; private set; }
    }
}
