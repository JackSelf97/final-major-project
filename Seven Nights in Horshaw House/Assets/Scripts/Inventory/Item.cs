using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [field: SerializeField] public ItemSO InventoryItem { get; private set; }
    [field: SerializeField] public int Count { get; set; } = 1;
    [SerializeField] private AudioSource audioSource = null;
    [SerializeField] private float duration = 0.3f;

    private void Start()
    {
        GetComponentInChildren<MeshFilter>().mesh = InventoryItem.ItemMesh; // item model (change via itemSO)
    }

    public void DestroyItem()
    {
        GetComponent<Collider>().enabled = false;
        StartCoroutine(AnimateItemPickup());
    }

    private IEnumerator AnimateItemPickup() // scale item down then destroy
    {
        audioSource.Play();
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero;
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, currentTime / duration);
            yield return null;
        }
        Destroy(gameObject);
    }
}
