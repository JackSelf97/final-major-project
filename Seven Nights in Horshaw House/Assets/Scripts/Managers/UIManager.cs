using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Main Menu Properties")]
    [SerializeField] private Animator stateDrivenCameraAnimator = null;
    [SerializeField] private GameObject playerCanvas = null; 
    [SerializeField] private GameObject menuCanvas = null;
    [SerializeField] private GameObject gameTitle = null;
    [SerializeField] private GameObject mainPanel = null;
    private GameObject player = null;
    private PlayerController playerController = null;

    [Header("General Menu Properties")]
    [SerializeField] private GameObject backButton = null;
    [SerializeField] private int backButtonIndex = 0;

    [Header("Testing")]
    [SerializeField] private GameObject testingPanel = null;

    [Header("Settings")]
    [SerializeField] private GameObject settingsPanel = null;

    [Header("Settings/Game")]
    [SerializeField] private GameObject gamePanel = null;
    [SerializeField] private GameObject defaultButton = null;

    [Header("Settings/Controls")]
    [SerializeField] private GameObject controlsPanel = null;

    [Header("Settings/Display")]
    [SerializeField] private GameObject displayPanel = null;

    [Header("Settings/Graphics")]
    [SerializeField] private GameObject graphicsPanel = null;

    [Header("Settings/Audio")]
    [SerializeField] private GameObject audioPanel = null;

    [Header("Prompt")] 
    [SerializeField] private GameObject promptPanel = null;
    [SerializeField] private Text promptText = null;
    [SerializeField] private Button promptYes = null;

    private readonly int subsettingsIndex = 2;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        
        playerController.LockUser(true);
        SubmenuTemplate(null, false, 0);
        GameManager.gMan.PlayerActionMap(false);
    }

    // Anything regarding "Prompts" will not be used inside the switch statement.
    public void SwitchMenu(string menuName)
    {
        switch (menuName)
        {
            case "Continue":
                stateDrivenCameraAnimator.Play("Player");
                menuCanvas.SetActive(false);
                playerCanvas.SetActive(true);
                playerController.LockUser(false);
                GameManager.gMan.mainMenu = false;
                GameManager.gMan.PlayerActionMap(true);
                break;
            case "Settings":
                SubmenuTemplate(settingsPanel, true, 1);
                break;
            case "Testing":
                SubmenuTemplate(testingPanel, true, 1);
                break;
            case "Back":
                if (backButtonIndex == 1)
                {
                    // MainMenu/Testing
                    if (testingPanel.activeSelf)
                    {
                        SubmenuTemplate(testingPanel, false, -1);
                        return;
                    }
                    // MainMenu/Settings
                    if (settingsPanel.activeSelf)
                    {
                        SubmenuTemplate(settingsPanel, false, -1);
                        return;
                    }
                }
                if (backButtonIndex == subsettingsIndex)
                {
                    // Settings
                    SubmenuTemplate(settingsPanel, true, -1);

                    // List of panels to toggle
                    GameObject[] panelsToToggle = { gamePanel, controlsPanel, displayPanel, graphicsPanel, audioPanel };

                    // Toggle panels
                    foreach (var panel in panelsToToggle)
                    {
                        if (panel.activeSelf)
                            panel.SetActive(false);
                    }
                }
                break;
            case "No":
                ShowPrompt(false);
                break;
            case "Game":
                SubmenuTemplate(settingsPanel, false, 0);
                SubmenuTemplate(gamePanel, true, 1);
                break;
            case "Controls":
                SubmenuTemplate(settingsPanel, false, 0);
                SubmenuTemplate(controlsPanel, true, 1);
                break;
            case "Display":
                SubmenuTemplate(settingsPanel, false, 0);
                SubmenuTemplate(displayPanel, true, 1);
                break;
            case "Graphics":
                SubmenuTemplate(settingsPanel, false, 0);
                SubmenuTemplate(graphicsPanel, true, 1);
                break;
            case "Audio":
                SubmenuTemplate(settingsPanel, false, 0);
                SubmenuTemplate(audioPanel, true, 1);
                break;
        }
    }

    private void SubmenuTemplate(GameObject panel, bool state, int index)
    {
        // Control back button and title. 
        backButton.SetActive(state);
        gameTitle.SetActive(!state);
        mainPanel.SetActive(!state);

        // When navigating through the menu, increment or decrement the 'backButtonIndex'
        backButtonIndex += index;

        // If there is no panel to change then this should be null.
        if (panel != null)
            panel.SetActive(state);
    }

    public void Resume() // Makes part of the Pause() redundant
    {
        playerController.LockUser(false);
        playerController.isPaused = false;
        playerController.pauseScreen.SetActive(false);

        GameManager.gMan.PlayerActionMap(true);
        Time.timeScale = 1f;
    }

    #region Functions w/ Prompts

    private void ShowPrompt(bool state, string prompt = "")
    {
        promptPanel.SetActive(state);
        promptText.text = prompt;
        promptYes.onClick.RemoveAllListeners();
    }

    public void P_ResetToDefault()
    {
        ShowPrompt(true, "RESET TO DEFAULT?");

        if (gamePanel.activeSelf)
            promptYes.onClick.AddListener(() => ResetRespiteMechanics());

        if (audioPanel.activeSelf)
            promptYes.onClick.AddListener(() => ResetAudioValues());
    }

    private void ResetRespiteMechanics()
    {
        Toggle[] respiteToggles = GetComponent<ToggleController>().respiteToggles;
        for (int i = 0; i < respiteToggles.Length; i++)
        {
            respiteToggles[i].isOn = true;
        }
        ShowPrompt(false);
    }

    private void ResetAudioValues()
    {
        Settings settings = FindObjectOfType<Settings>();
        Slider[] volumes = { settings.masterVolumeSlider, settings.musicVolumeSlider, settings.ambientVolumeSlider, settings.SFXVolumeSlider };
        for (int i = 0; i < volumes.Length; i++)
        {
            volumes[i].value = 100f;
        }
        ShowPrompt(false);
    }

    public void P_RestartGame()
    {
        ShowPrompt(true, "ARE YOU SURE YOU WANT TO RESTART THE GAME?");
        promptYes.onClick.AddListener(() => RestartGame());
    }

    private void RestartGame() // Currently does not reset positions or time
    {
        Time.timeScale = 1f;
        playerController.isPaused = false;
        playerController.pauseScreen.SetActive(false);
        stateDrivenCameraAnimator.Play("Main Menu");
        playerCanvas.SetActive(false);
        menuCanvas.SetActive(true);
        GameManager.gMan.mainMenu = true;
        GameManager.gMan.PlayerActionMap(false);

        ShowPrompt(false);
    }

    public void P_QuitGame()
    {
        ShowPrompt(true, "ARE YOU SURE YOU WANT TO QUIT THE GAME?");
        promptYes.onClick.AddListener(() => QuitGame());
    }

    private void QuitGame()
    {
        Application.Quit();
        ShowPrompt(false);
    }

    public void ShowInfo(GameObject infoPanel)
    {
        infoPanel.SetActive(true);
    }

    public void HideInfo(GameObject infoPanel)
    {
        infoPanel.SetActive(false);
    }

    #endregion
}