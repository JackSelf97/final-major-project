using UnityEngine;
using UnityEngine.UI;

public class LevelBoundaries : MonoBehaviour
{
    public GameObject startPos; // Assign 'StartPos' GameObject in the Inspector
    public float countdownDuration = 10f;
    public Text warningText; // Assign a UI Text component in the Inspector to display the warning

    private bool playerInside = false;
    private float countdownTimer;

    private void Start()
    {
        countdownTimer = countdownDuration;

        // Ensure the warning text is initially hidden
        if (warningText != null)
            warningText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (playerInside)
            Countdown();
    }

    private void Countdown()
    {
        countdownTimer -= Time.deltaTime;

        // Update the warning text and countdown display
        if (warningText != null)
            warningText.text = $"Go back! Time remaining: {Mathf.CeilToInt(countdownTimer)}s";

        if (countdownTimer <= 0f)
            TeleportToStartPos();
    }

    private void TeleportToStartPos()
    {
        if (startPos != null)
        {
            // Teleport the player to the 'StartPos' GameObject
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                player.transform.position = startPos.transform.position;
        }

        // Reset the countdown timer
        countdownTimer = countdownDuration;

        // Hide the warning text
        if (warningText != null)
            warningText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;

            // Show the warning text when the player enters the boundaries
            if (warningText != null)
                warningText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            // Reset the countdown timer when the player exits the boundaries
            countdownTimer = countdownDuration;

            // Hide the warning text
            if (warningText != null)
                warningText.gameObject.SetActive(false);
        }
    }
}