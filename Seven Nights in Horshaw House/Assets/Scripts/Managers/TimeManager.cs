using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

// https://www.youtube.com/watch?v=L4t2c1_Szdk&t=42s&ab_channel=KetraGames
public class TimeManager : MonoBehaviour
{
    [Header("Time")]
    [NonSerialized] public float timeScale = 1000;
    public float timeMultiplier = 0f;
    public float startHour = 0f;
    public int days = 0;
    public int maxDays = 7;
    public DateTime currentTime;
    public Text dayText = null;
    [SerializeField] private Text timeText = null;
    private int lastRecordedDay = 0;

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

    [Header("Game Properties")]
    public GameObject enemy = null;
    public AccessPoint accessPoint = null;

    [Header("Post-Processing Effects")]
    [SerializeField] private PostProcessVolume nightProfile = null;
    [SerializeField] private PostProcessVolume dayProfile = null;

    // Start is called before the first frame update
    void Start()
    {
        InitialiseTimeOfDay();
    }

    void InitialiseTimeOfDay()
    {
        currentTime = DateTime.Now.Date + TimeSpan.FromHours(startHour);
        sunriseTime = TimeSpan.FromHours(sunriseHour);
        sunsetTime = TimeSpan.FromHours(sunsetHour);
        lastRecordedDay = currentTime.Day;
        //enemy.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!accessPoint.isTimePaused && !GameManager.gMan.mainMenu)
        {
            UpdateTimeOfDay();
            RotateSun();
            UpdateLightSettings();
            //ManageEnemyActivation();
        }
    }

    private void UpdateTimeOfDay()
    {
        currentTime = currentTime.AddSeconds(Time.deltaTime * timeMultiplier);

        if (timeText != null)
        {
            timeText.text = currentTime.ToString("hh:mm tt");
        }

        if (dayText != null)
        {
            UpdateDayText();
        }

        // End Game State
        if (days >= maxDays && !GameManager.gMan.gameWon)
        {
            GameManager.gMan.EnableEndGameState();
            Time.timeScale = 0f;
            ResetTimeOfDay();
        }
    }

    private void UpdateDayText()
    {
        int currentDay = currentTime.Day;
        if (currentDay != lastRecordedDay)
        {
            days++;
            lastRecordedDay = currentDay;
        }
        dayText.text = "DAY " + days.ToString();
    }

    private void RotateSun()
    {
        float sunlightRotation;
        TimeSpan duration;
        TimeSpan timeSinceEvent;

        if (currentTime.TimeOfDay > sunriseTime && currentTime.TimeOfDay < sunsetTime)
        {
            duration = CalculateTimeDifference(sunriseTime, sunsetTime);
            timeSinceEvent = CalculateTimeDifference(sunriseTime, currentTime.TimeOfDay);
            sunlightRotation = CalculateSunlightRotation(0, 180, duration, timeSinceEvent);
        }
        else
        {
            duration = CalculateTimeDifference(sunsetTime, sunriseTime);
            timeSinceEvent = CalculateTimeDifference(sunsetTime, currentTime.TimeOfDay);
            sunlightRotation = CalculateSunlightRotation(180, 360, duration, timeSinceEvent);
        }

        // Rotate the light source
        sunlight.transform.rotation = Quaternion.AngleAxis(sunlightRotation, Vector3.right);
    }

    private float CalculateSunlightRotation(float startRotation, float endRotation, TimeSpan duration, TimeSpan timeSinceEvent)
    {
        double percentage = timeSinceEvent.TotalMinutes / duration.TotalMinutes;
        return Mathf.Lerp(startRotation, endRotation, (float)percentage);
    }

    private void UpdateLightSettings()
    {
        float dotProduct = Vector3.Dot(sunlight.transform.forward, Vector3.down); // returns a value of 1 or -1
        float timeOfDay = lightChangeCurve.Evaluate(dotProduct);

        // Update light settings
        UpdateSunlightIntensity(timeOfDay);
        UpdateMoonlightIntensity(timeOfDay);
        UpdateAmbientLightColor(timeOfDay);

        // Blending between Post-Processing profiles
        BlendPostProcessingProfiles(timeOfDay);
    }

    private void UpdateSunlightIntensity(float timeOfDay)
    {
        sunlight.intensity = Mathf.Lerp(0, maxSunlightIntensity, timeOfDay);
    }

    private void UpdateMoonlightIntensity(float timeOfDay)
    {
        moonlight.intensity = Mathf.Lerp(maxMoonlightIntensity, 0, timeOfDay);
    }

    private void UpdateAmbientLightColor(float timeOfDay)
    {
        RenderSettings.ambientLight = Color.Lerp(nightAmbientLight, dayAmbientLight, timeOfDay);
    }

    private void BlendPostProcessingProfiles(float timeOfDay)
    {
        if (dayProfile != null && nightProfile != null)
        {
            float blendFactor = Mathf.Lerp(0, 1, timeOfDay);
            nightProfile.weight = 1 - blendFactor;
            dayProfile.weight = blendFactor;
        }
        else
        {
            Debug.LogWarning("No post-processing profiles detected.");
        }
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

    public void ResetTimeOfDay()
    {
        currentTime = DateTime.Now.Date + TimeSpan.FromHours(startHour);
        lastRecordedDay = currentTime.Day;
        days = 0;
    }

    #region Triggers

    private void ManageEnemyActivation()
    {
        // Check if the monster is jump scaring the player
        if (GameManager.gMan.isjumpScaring) { return; }

        // Get the current hour from the currentTime
        int currentHour = currentTime.Hour;
        const int enemySpawnDay = 2;

        // Check if the enemy should be active during the evening (between sunsetHour and sunriseHour)
        if ((currentHour >= sunsetHour && currentHour < 24) || (currentHour >= 0 && currentHour < sunriseHour))
        {
            if (!enemy.activeSelf && days >= enemySpawnDay)
            {
                // Activate the enemy during the evening
                enemy.SetActive(true);

                // Change enemy behavior for night
                // For example, you can set the enemy to be more aggressive or move faster
                //enemy.GetComponent<EnemyController>().SetNightBehavior();

                Debug.Log("Nighttime: Activate the enemy with night behavior.");
            }
        }
        else
        {
            if (enemy.activeSelf)
            {
                // Reset the enemy
                enemy.GetComponent<EnemyController>().EnemyReset();

                // Deactivate the enemy outside the evening hours
                enemy.SetActive(false);

                // Change enemy behavior for the day
                // For example, you can set the enemy to be less aggressive or move slower
                //enemy.GetComponent<EnemyController>().SetDayBehavior();

                Debug.Log("Daytime: Deactivate the enemy with day behavior.");
            }
        }
    }

    #endregion
}