using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    // Audio
    [Header("Audio Settings")]
    public AudioMixer audioMixer;
    public float masterVolume = 0f;
    public Slider masterVolumeSlider = null;
    public Text masterValueText = null;
    public float musicVolume = 0f;
    public Slider musicVolumeSlider = null;
    public Text musicValueText = null;
    public float ambientVolume = 0f;
    public Slider ambientVolumeSlider = null;
    public Text ambientValueText = null;
    public float SFXVolume = 0f;
    public Slider SFXVolumeSlider = null;
    public Text SFXValueText = null;

    // Start is called before the first frame update
    void Start()
    {
        // Audio 
        masterVolumeSlider.onValueChanged.AddListener(delegate { SetMasterVolume(masterVolumeSlider.value); });
        musicVolumeSlider.onValueChanged.AddListener(delegate { SetMusicVolume(musicVolumeSlider.value); });
        ambientVolumeSlider.onValueChanged.AddListener(delegate { SetAmbientVolume(ambientVolumeSlider.value); });
        SFXVolumeSlider.onValueChanged.AddListener(delegate { SetSFXVolume(SFXVolumeSlider.value); });
    }

    #region Audio (Logic)
    // currently results in long floats -- try rounding to two decimal places

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", volume);
        masterVolume = masterVolumeSlider.value;
        masterValueText.text = Mathf.RoundToInt(masterVolumeSlider.normalizedValue * 100).ToString();
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
        musicVolume = musicVolumeSlider.value;
        musicValueText.text = Mathf.RoundToInt(musicVolumeSlider.normalizedValue * 100).ToString();
    }

    public void SetAmbientVolume(float volume)
    {
        audioMixer.SetFloat("AmbientVolume", volume);
        ambientVolume = ambientVolumeSlider.value;
        ambientValueText.text = Mathf.RoundToInt(ambientVolumeSlider.normalizedValue * 100).ToString();
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", volume);
        SFXVolume = SFXVolumeSlider.value;
        SFXValueText.text = Mathf.RoundToInt(SFXVolumeSlider.normalizedValue * 100).ToString();
    }

    #endregion
}
