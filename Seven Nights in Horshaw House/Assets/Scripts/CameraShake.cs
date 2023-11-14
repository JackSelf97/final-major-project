using Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance { get; private set; }
    private CinemachineVirtualCamera virtualCamera = null;
    private float shakeTimer = 0;
    private float shakeTimerTotal = 0;
    private float startingIntensity = 0;

    void Awake()
    {
        instance = this;
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    private void Update()
    {
        shakeTimer -= Time.deltaTime;
        if (shakeTimer <= 0f)
        {
            CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = Mathf.Lerp(startingIntensity, 0f, (1 - (shakeTimer / shakeTimerTotal)));
        }
    }

    public void ShakeCamera(float intensity, float time)
    {
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;
        startingIntensity = intensity;
        shakeTimerTotal = time;
        shakeTimer = time;
    }
}
