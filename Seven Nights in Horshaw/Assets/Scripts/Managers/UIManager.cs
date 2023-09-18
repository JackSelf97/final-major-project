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
    [SerializeField] private GameObject buttonPanel = null;
    private GameObject player = null;
    public bool mainMenu = true;

    [Header("General Menu Properties")]
    [SerializeField] private GameObject backButton = null;
    [SerializeField] private int backButtonIndex = 0;
    [SerializeField] private Text title = null;

    [Header("Settings")]
    [SerializeField] private GameObject settingsPanel = null;

    [Header("Settings/Game")]
    [SerializeField] private GameObject respitePanel = null;
    [SerializeField] private Toggle[] respiteToggles = new Toggle[0];
    [SerializeField] private GameObject defaultButton = null;

    [Header("Prompt")]
    [SerializeField] private GameObject promptPanel = null;
    [SerializeField] private Text promptText = null;
    [SerializeField] private Button promptYes = null;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>().gameObject;
        title.enabled = false;
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
                backButton.SetActive(true);
                title.enabled = true;
                gameTitle.SetActive(false);
                buttonPanel.SetActive(false);
                ShowSettings("Settings", true);
                break;
            case "Back":
                if (backButtonIndex == 1)
                {
                    backButton.SetActive(false);
                    title.enabled = false;
                    gameTitle.SetActive(true);
                    buttonPanel.SetActive(true);
                    ShowSettings(string.Empty, false, -1);
                }
                if (backButtonIndex == 2)
                {
                    ShowSettings("Settings", true, -1);
                }
                break;
            case "No":
                ShowPrompt(false);
                break;
            case "Game":
                ShowSettings("Game", false);
                respitePanel.SetActive(true);
                break;
            case "Controls":
                ShowSettings("Controls", false);
                break;
            case "Display":
                ShowSettings("Display", false);
                break;
            case "Graphics":
                ShowSettings("Graphics", false);
                break;
            case "Audio":
                ShowSettings("Audio", false);
                break;
        }
    }

    private void ShowSettings(string name, bool state, int index = 1)
    {
        settingsPanel.SetActive(state);
        title.text = name;
        backButtonIndex += index;

        // Game
        if (respitePanel.activeSelf)
            respitePanel.SetActive(false);
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

    #endregion
}