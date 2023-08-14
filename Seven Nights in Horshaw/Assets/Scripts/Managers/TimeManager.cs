using System;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

// https://www.youtube.com/watch?v=L4t2c1_Szdk&t=42s&ab_channel=KetraGames
public class TimeManager : MonoBehaviour
{
    [Header("Time")]
    public float timeMultiplier = 0f;
    [NonSerialized] public float timeScale = 1000;
    [SerializeField] private float startHour = 0f;
    [SerializeField] private Text timeText = null;
    private DateTime currentTime;

    [Header("Sunlight")]
    [SerializeField] private Light sunlight = null;
    [SerializeField] private float sunriseHour = 0f;
    [SerializeField] private float sunsetHour = 0f;
    private TimeSpan sunriseTime;
    private TimeSpan sunsetTime;
    [SerializeField] private Color dayAmbientLight;
    [SerializeField] private Color nightAmbientLight;
    [SerializeField] private AnimationCurve lightChangeCurve = null;
    [SerializeField] private float maxSunlightIntensity = 0f;

    [Header("Moonlight")]
    [SerializeField] private Light moonlight = null;
    [SerializeField] private float maxMoonlightIntensity = 0f;

    [Header("Game Mechanics")]
    [SerializeField] private GameObject enemy = null;
    public AccessPoint accessPoint = null;

    // Start is called before the first frame update
    void Start()
    {
        currentTime = DateTime.Now.Date + TimeSpan.FromHours(startHour);
        sunriseTime = TimeSpan.FromHours(sunriseHour);
        sunsetTime = TimeSpan.FromHours(sunsetHour);
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
            timeText.text = currentTime.ToString("HH:mm"); // 24 hour format
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
        if (difference.TotalSeconds < 0) // is it negative?
        {
            difference += TimeSpan.FromHours(24);
        }
        return difference;
    }

    private void EnemyState()
    {
        // Spawning the enemy
        if (currentTime.Hour.Equals(13) && !enemy.activeSelf) // consider using greater than rather than equals
        {
            Debug.Log("Enable the enemy!");
            enemy.SetActive(true);
        }
        else if (currentTime.Hour.Equals((int)sunriseHour) && enemy.activeSelf)
        {
            Debug.Log("Disable the enemy!");
            enemy.SetActive(false);
        }
    }
}
