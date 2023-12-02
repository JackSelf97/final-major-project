using Inventory.Model;
using System.Collections;
using UnityEngine;

public class Item : MonoBehaviour
{
    [field: SerializeField] public ItemSO InventoryItem { get; private set; }
    [field: SerializeField] public int Count { get; set; } = 1;
    [SerializeField] private AudioSource audioSource = null;
    [SerializeField] private float duration = 0.3f;

    public void DestroyItem()
    {
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject); // Replace with 'StartCoroutine(AnimateItemPickup());'
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