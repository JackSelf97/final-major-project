using UnityEngine;

public class FloatingItem : MonoBehaviour
{
    public float floatSpeed = 0.1f;
    public float rotationSpeed = 30f;
    private float timer = 0f;
    public float floatDuration = 60f;

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
            // Game over
            GameManager.gMan.lostPlayerSkull = true;
            GameManager.gMan.EnableEndGameState();
        }
    }
}