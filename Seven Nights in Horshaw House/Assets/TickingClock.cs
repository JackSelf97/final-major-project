using UnityEngine;

public class TickingClock : MonoBehaviour
{
    [SerializeField] private Transform clockHourHand = null;
    [SerializeField] private Transform clockMinuteHand = null;
    private TimeManager timeManager = null;

    // Start is called before the first frame update
    void Start()
    {
        timeManager = FindObjectOfType<TimeManager>();
    }

    // Update is called once per frame
    void Update()
    {
        float hoursAngle = (timeManager.currentTime.Hour % 12) * 30f + (timeManager.currentTime.Minute / 60f) * 30f;
        float minutesAngle = timeManager.currentTime.Minute * 6f;

        // Rotate the clock hands based on the calculated angles
        clockHourHand.localRotation = Quaternion.Euler(new Vector3(hoursAngle, 0, 0));
        clockMinuteHand.localRotation = Quaternion.Euler(new Vector3(minutesAngle, 0, 0));
    }
}