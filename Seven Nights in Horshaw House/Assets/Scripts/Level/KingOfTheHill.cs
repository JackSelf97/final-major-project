using UnityEngine;
using UnityEngine.UI;

public class KingOfTheHill : MonoBehaviour
{
    public Slider controlPointSlider;
    public float captureRate = 0.2f; // Rate at which the control point is captured
    public float captureDecreaseRate = 0.1f; // Rate at which the control point decreases when not owned
    public float maxCaptureValue = 1f; // Maximum value for the control point
    public float captureThreshold = 0.8f; // Threshold for considering the control point captured

    public bool playerInside = false;
    public bool enemyInside = false;

    [SerializeField] private PlayerStats playerStats;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
        }
        else if (other.CompareTag("Enemy"))
        {
            enemyInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
        else if (other.CompareTag("Enemy"))
        {
            enemyInside = false;
        }
    }

    private void Update()
    {
        UpdateControlPointStatus();
    }

    private void UpdateControlPointStatus()
    {
        if (playerInside && !enemyInside && !playerStats.spiritRealm)
        {
            if (controlPointSlider.value < maxCaptureValue)
            {
                // Increase the control point slider value
                controlPointSlider.value += captureRate * Time.deltaTime;

                // Ensure the value doesn't exceed the maximum
                controlPointSlider.value = Mathf.Clamp(controlPointSlider.value, 0f, maxCaptureValue);
            }
        }
        else if (!playerInside && enemyInside || playerStats.spiritRealm)
        {
            if (controlPointSlider.value > 0f)
            {
                // Decrease the control point slider value
                controlPointSlider.value -= captureDecreaseRate * Time.deltaTime;

                // Ensure the value doesn't go below 0
                controlPointSlider.value = Mathf.Clamp(controlPointSlider.value, 0f, maxCaptureValue);
            }
        }

        // Keep the slider at the maximum value when fully captured
        if (controlPointSlider.value >= maxCaptureValue)
        {
            controlPointSlider.value = maxCaptureValue;
        }
    }
}