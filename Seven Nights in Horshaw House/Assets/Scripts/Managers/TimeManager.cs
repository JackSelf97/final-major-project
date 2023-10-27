using System;
using UnityEngine;
using UnityEngine.UI;

// https://www.youtube.com/watch?v=L4t2c1_Szdk&t=42s&ab_channel=KetraGames
public class TimeManager : MonoBehaviour
{
    [Header("Time")]
    [NonSerialized] public float timeScale = 1000;
    public float timeMultiplier = 0f;
    public float startHour = 0f;
    public int lastRecordedDay = 0;
    public int newDayStartHour = 0;
    public int days = 0;
    public int maxDays = 7;
    public DateTime currentTime;
    public Text dayText = null;
    [SerializeField] private Text timeText = null;

    [Header("Sunlight")]
    [SerializeField] private Light sunlight = null;
    [SerializeField] private float sunriseHour = 0f;
    [SerializeField] private float sunsetHour = 0f;
    [SerializeField] private Color dayAmbientLight;
    [SerializeField] private Color nightAmbientLight;
    [SerializeField] private AnimationCurve lightChangeCurve = null;
    [SerializeField] private float maxSunlightIntensity = 0f;
    private TimeSpan sunriseTime;
    private TimeSpan sunsetTime;

    [Header("Moonlight")]
    [SerializeField] private Light moonlight = null;
    [SerializeField] private float maxMoonlightIntensity = 0f;

    [Header("Game Mechanics")]
    [SerializeField] private UIManager UIMan = null;
    public GameObject enemy = null;
    public AccessPoint accessPoint = null;

    // Start is called before the first frame update
    void Start()
    {
        currentTime = DateTime.Now.Date + TimeSpan.FromHours(startHour);
        sunriseTime = TimeSpan.FromHours(sunriseHour);
        sunsetTime = TimeSpan.FromHours(sunsetHour);
        lastRecordedDay = currentTime.Day; // Set the initial day
        enemy.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!accessPoint.isTimePaused && !GameManager.gMan.mainMenu)
        {
            UpdateTimeOfDay();
            RotateSun();
            UpdateLightSettings();
            EnemyState();
        }
    }

    private void UpdateTimeOfDay()
    {
        currentTime = currentTime.AddSeconds(Time.deltaTime * timeMultiplier);

        if (timeText != null)
        {
            timeText.text = currentTime.ToString("HH:mm tt"); // 24 hour format with AM/PM
        }
        if (dayText != null)
        {
            int currentDay = currentTime.Day;
            if (currentDay != lastRecordedDay)
            {
                days++;
                lastRecordedDay = currentDay;
            }
            dayText.text = "DAY " + days.ToString();
        }

        // End Game State
        if (days >= maxDays && !GameManager.gMan.gameWon)
        {
            GameManager.gMan.EnableEndGameState();

            // Reset Time & Day
            Time.timeScale = 0f;
            ResetTime();
        }
    }

    private void RotateSun()
    {
        float sunlightRotation;
        if (currentTime.TimeOfDay > sunriseTime && currentTime.TimeOfDay < sunsetTime)
        {
            TimeSpan sunriseToSunsetDuration = CalculateTimeDifference(sunriseTime, sunsetTime);
            TimeSpan timeSinceSunrise = CalculateTimeDifference(sunriseTime, currentTime.TimeOfDay);

            double percentage = timeSinceSunrise.TotalMinutes / sunriseToSunsetDuration.TotalMinutes;

            sunlightRotation = Mathf.Lerp(0,180, (float)percentage);
        }
        else
        {
            TimeSpan sunsetToSunriseDuration = CalculateTimeDifference(sunsetTime, sunriseTime);
            TimeSpan timeSinceSunset = CalculateTimeDifference(sunsetTime, currentTime.TimeOfDay);

            double percentage = timeSinceSunset.TotalMinutes / sunsetToSunriseDuration.TotalMinutes;

            sunlightRotation = Mathf.Lerp(180, 360, (float)percentage);
        }

        // rotate the light source
        sunlight.transform.rotation = Quaternion.AngleAxis(sunlightRotation, Vector3.right);
    }

    private void UpdateLightSettings()
    {
        float dotProduct = Vector3.Dot(sunlight.transform.forward, Vector3.down); // returns a value of 1 or -1
        sunlight.intensity = Mathf.Lerp(0, maxSunlightIntensity, lightChangeCurve.Evaluate(dotProduct));
        moonlight.intensity = Mathf.Lerp(maxMoonlightIntensity, 0, lightChangeCurve.Evaluate(dotProduct));
        RenderSettings.ambientLight = Color.Lerp(nightAmbientLight, dayAmbientLight, lightChangeCurve.Evaluate(dotProduct));
    }

    private TimeSpan CalculateTimeDifference(TimeSpan fromTime, TimeSpan toTime)
    {
        TimeSpan difference = toTime - fromTime;
        if (difference.TotalSeconds < 0)
        {
            difference += TimeSpan.FromHours(24);
        }
        return difference;
    }

    public void ResetTime()
    {
        currentTime = DateTime.Now.Date + TimeSpan.FromHours(startHour);
        lastRecordedDay = currentTime.Day;
        days = 0;
    }

    #region Triggers

    private void EnemyState()
    {
        // Spawning the enemy
        if (currentTime.Hour.Equals((int)sunsetHour) && !enemy.activeSelf) // consider using greater than rather than equals
        {
            enemy.SetActive(true);
        }
        else if (currentTime.Hour.Equals((int)sunriseHour) && enemy.activeSelf)
        {
            enemy.SetActive(false);
        }
    }

    #endregion
}