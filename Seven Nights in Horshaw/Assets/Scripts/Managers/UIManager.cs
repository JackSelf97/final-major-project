using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public bool mainMenu = true;

    [Header("General Menu Properties")]
    [SerializeField] private GameObject backButton = null;
    [SerializeField] private int backButtonIndex = 0;
    [SerializeField] private Text header = null;

    [Header("Testing")]
    [SerializeField] private GameObject testingPanel = null;

    [Header("Settings")]
    [SerializeField] private GameObject settingsPanel = null;

    [Header("Settings/Game")]
    [SerializeField] private GameObject respitePanel = null;
    [SerializeField] private GameObject defaultButton = null;

    [Header("Settings/Controls")]
    [SerializeField] private GameObject controlsPanel = null;

    [Header("Prompt")]
    [SerializeField] private GameObject promptPanel = null;
    [SerializeField] private Text promptText = null;
    [SerializeField] private Button promptYes = null;

    private readonly int subsettingsIndex = 2;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>().gameObject;
        SubmenuTemplate(string.Empty, null, false, 0);
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
                player.GetComponent<PlayerController>().LockUser(false);
                break;
            case "Settings":
                SubmenuTemplate("Settings", settingsPanel, true, 1);
                break;
            case "Testing":
                SubmenuTemplate("Testing", testingPanel, true, 1);
                break;
            case "Back":
                if (backButtonIndex == 1)
                {
                    // MainMenu/Testing
                    if (testingPanel.activeSelf)
                    {
                        SubmenuTemplate(string.Empty, testingPanel, false, -1);
                        return;
                    }
                    // MainMenu/Settings
                    if (settingsPanel.activeSelf)
                    {
                        SubmenuTemplate(string.Empty, settingsPanel, false, -1);
                        return;
                    }
                }
                if (backButtonIndex == subsettingsIndex)
                {
                    // Settings
                    SubmenuTemplate("Settings", settingsPanel, true, -1);

                    // Settings/Game
                    if (respitePanel.activeSelf)
                        respitePanel.SetActive(false);

                    // Settings/Controls
                    if (controlsPanel.activeSelf)
                        controlsPanel.SetActive(false);
                }
                break;
            case "No":
                ShowPrompt(false);
                break;
            case "Game":
                SubmenuTemplate(string.Empty, settingsPanel, false, 0);
                SubmenuTemplate("Game", respitePanel, true, 1);
                break;
            case "Controls":
                SubmenuTemplate(string.Empty, settingsPanel, false, 0);
                SubmenuTemplate("Controls", controlsPanel, true, 1);
                break;
            case "Display":
                SubmenuTemplate(string.Empty, settingsPanel, false, 0);
                SubmenuTemplate("Display", null, true, 1);
                break;
            case "Graphics":
                SubmenuTemplate(string.Empty, settingsPanel, false, 0);
                SubmenuTemplate("Graphics", null, true, 1);
                break;
            case "Audio":
                SubmenuTemplate(string.Empty, settingsPanel, false, 0);
                SubmenuTemplate("Audio", null, true, 1);
                break;
        }
    }

    private void SubmenuTemplate(string headerName, GameObject panel, bool state, int index)
    {
        // Control the header, back button and title. 
        header.enabled = state;
        header.text = headerName;
        backButton.SetActive(state);
        gameTitle.SetActive(!state);
        mainPanel.SetActive(!state);

        // When navigating through the menu, increment or decrement the 'backButtonIndex'
        backButtonIndex += index;

        // If there is no panel to change then this should be null.
        if (panel != null)
            panel.SetActive(state);
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
        ShowPrompt(true, "Reset to Default?");
        promptYes.onClick.AddListener(() => ResetRespiteMechanics());
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

    public void P_QuitGame()
    {
        ShowPrompt(true, "Are you sure you want to quit the game?");
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