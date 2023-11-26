using UnityEngine;

public class FloatingItem : MonoBehaviour
{
    public float floatSpeed = 0.1f; // Speed of floating
    public float rotationSpeed = 30f; // Speed of rotation in degrees per second
    private float timer = 0f;
    public float floatDuration = 60f; // Duration of floating in seconds

    void Update()
    {
        // Increment the timer
        timer += Time.deltaTime;

        // Float upwards
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);

        // Rotate the item continuously
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        // Check if the duration has elapsed
        if (timer >= floatDuration)
        {
            // Deactivate the GameObject
            gameObject.SetActive(false);
        }
    }
}
