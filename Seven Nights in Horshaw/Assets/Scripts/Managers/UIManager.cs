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

    [Header("Game")]
    [SerializeField] private GameObject respiteMechanicsPanel = null;

    [Header("Quit")]
    [SerializeField] private GameObject promptPanel = null;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>().gameObject;
        title.enabled = false;
    }

    //public void SwitchState()
    //{
    //    if (mainMenu)
    //    {
            
    //    }
    //    else
    //    {
    //        stateDrivenCameraAnimator.Play("Main Menu");
    //        playerCanvas.SetActive(false);
    //        menuCanvas.SetActive(true);
    //        player.GetComponent<PlayerController>().LockUser(true);
    //    }
    //    mainMenu = !mainMenu;
    //}

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
            case "Quit":
                promptPanel.SetActive(true);
                break;
            case "No":
                promptPanel.SetActive(false);
                break;
            case "Yes":
                Application.Quit();
                break;
            case "Game":
                ShowSettings("Game", false);
                respiteMechanicsPanel.SetActive(true);
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
        if (respiteMechanicsPanel.activeSelf)
            respiteMechanicsPanel.SetActive(false);
    }
}