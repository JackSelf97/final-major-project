using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    public Light flickeringLight;

    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;
    public float flickerSpeed = 1.0f;

    public bool useBursts = true;
    public int flickerBurstCount = 3;
    public float timeBetweenBursts = 5.0f;

    private float originalIntensity;

    void Start()
    {
        if (flickeringLight == null)
        {
            flickeringLight = GetComponent<Light>();
        }

        if (flickeringLight != null)
        {
            originalIntensity = flickeringLight.intensity;
            if (useBursts)
            {
                InvokeRepeating("FlickerBurst", 0.0f, timeBetweenBursts);
            }
            else
            {
                InvokeRepeating("Flicker", 0.0f, flickerSpeed);
            }
        }
        else
        {
            Debug.LogError("FlickeringLight script is missing a reference to the Light component!");
        }
    }

    void Flicker()
    {
        float randomIntensity = Random.Range(minIntensity, maxIntensity);
        flickeringLight.intensity = originalIntensity * randomIntensity;
    }

    void FlickerBurst()
    {
        for (int i = 0; i < flickerBurstCount; i++)
        {
            Invoke("Flicker", i * (1.0f / flickerBurstCount));
        }
    }
}
