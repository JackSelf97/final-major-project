using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [Header("Display")]
    public Dropdown resolutionDropdown;
    public Resolution[] resolutions;
    public int resolutionIndex;
    public Dropdown screenModeDropdown;
    public int screenMode;

    // Audio
    [Header("Audio")]
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
        // Display
        resolutionDropdown.onValueChanged.AddListener(delegate { OnResolutionChange(); });
        screenModeDropdown.onValueChanged.AddListener(delegate { OnScreenModeChange(screenModeDropdown.value); });
        resolutions = Screen.resolutions;
        foreach (Resolution resolution in resolutions)
        {
            resolutionDropdown.options.Add(new Dropdown.OptionData(resolution.ToString()));
        }
        resolutionDropdown.value = resolutions.Length;

        // Audio 
        masterVolumeSlider.onValueChanged.AddListener(delegate { SetMasterVolume(masterVolumeSlider.value); });
        musicVolumeSlider.onValueChanged.AddListener(delegate { SetMusicVolume(musicVolumeSlider.value); });
        ambientVolumeSlider.onValueChanged.AddListener(delegate { SetAmbientVolume(ambientVolumeSlider.value); });
        SFXVolumeSlider.onValueChanged.AddListener(delegate { SetSFXVolume(SFXVolumeSlider.value); });
    }

    #region Display

    public void OnResolutionChange()
    {
        Screen.SetResolution(resolutions[resolutionDropdown.value].width, resolutions[resolutionDropdown.value].height, Screen.fullScreen);
        resolutionIndex = resolutionDropdown.value;
    }

    public void OnScreenModeChange(int screenModeInts)
    {
        screenMode = screenModeInts;
        switch (screenModeInts)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow; // windowed borderless
                break;
            default:
                break;
        }
    }

    #endregion

    #region Audio (Logic)

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