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
    [SerializeField] private Text title = null;

    [Header("Settings")]
    [SerializeField] private GameObject settingsPanel = null;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>().gameObject;
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
                title.text = "Settings";
                gameTitle.SetActive(false);
                buttonPanel.SetActive(false);
                settingsPanel.SetActive(true);
                break;
            case "Quit":
                Application.Quit();
                break;
        }
    }
}
