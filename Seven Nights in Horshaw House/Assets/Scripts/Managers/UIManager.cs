using Inventory;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Main Menu Properties")]
    [SerializeField] private Animator stateDrivenCameraAnimator = null;
    [SerializeField] private GameObject playerCanvas = null; 
    [SerializeField] private GameObject menuCanvas = null;
    [SerializeField] private GameObject gameTitle = null;
    [SerializeField] private GameObject mainButtonPanel = null;

    // Game Properties
    private GameObject player = null;
    private PlayerController playerController = null;
    private InteractableObject[] interactableObjects = new InteractableObject[0];

    [Header("Managers")]
    [SerializeField] private TimeManager timeManager = null;

    [Header("General Menu Properties")]
    [SerializeField] private GameObject backButton = null;
    [SerializeField] private int backButtonIndex = 0;

    [Header("Player Menu Properties")]
    [SerializeField] private GameObject pauseButtonPanel = null;

    [Header("Settings")]
    [SerializeField] private GameObject settingPanel = null;

    [Header("Settings/Game")]
    [SerializeField] private GameObject gamePanel = null;

    [Header("Settings/Controls")]
    [SerializeField] private GameObject controlPanel = null;

    [Header("Settings/Display")]
    [SerializeField] private GameObject displayPanel = null;

    [Header("Settings/Graphics")]
    [SerializeField] private GameObject graphicPanel = null;

    [Header("Settings/Audio")]
    [SerializeField] private GameObject audioPanel = null;

    [Header("Respite Mechanics")]
    [SerializeField] private GameObject RMSettingPanel = null;

    [Header("Respite Mechanics/Toggles")]
    [SerializeField] private GameObject togglePanel = null;

    [Header("Respite Mechanics/Info")]
    [SerializeField] private GameObject infoPanel = null;

    [Header("Prompts")] 
    [SerializeField] private GameObject promptPanel = null;
    [SerializeField] private Text promptText = null;
    [SerializeField] private Button promptYes = null;

    private readonly int RMSIndex = 2, SIndex = 3;

    // Start is called before the first frame update
    void Start()
    {
        InitialiseGame();
    }

    private void InitialiseGame()
    {
        // Locate GameObjects & Scripts
        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        timeManager = FindObjectOfType<TimeManager>();
        interactableObjects = FindObjectsOfType<InteractableObject>();

        // Game Prep
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
            case "Respite Mechanics":
                if (GameManager.gMan.mainMenu)
                    SubmenuTemplate(RMSettingPanel, true, 1);
                else
                {
                    // Player Pause Screen
                    pauseButtonPanel.SetActive(false);
                    menuCanvas.SetActive(true);
                    SubmenuTemplate(RMSettingPanel, true, 1);
                }
                break;
            case "Settings":
                if (GameManager.gMan.mainMenu)
                    SubmenuTemplate(settingPanel, true, 1);
                else
                {
                    // Player Pause Screen
                    pauseButtonPanel.SetActive(false);
                    menuCanvas.SetActive(true);
                    SubmenuTemplate(settingPanel, true, 1);
                }
                break;
            case "Back":
                if (backButtonIndex == 1)
                {
                    if (RMSettingPanel.activeSelf)
                    {
                        if (GameManager.gMan.mainMenu)
                            SubmenuTemplate(RMSettingPanel, false, -1);
                        else
                        {
                            SubmenuTemplate(RMSettingPanel, false, -1);
                            menuCanvas.SetActive(false);
                            pauseButtonPanel.SetActive(true);
                        }
                    }
                    else if (settingPanel.activeSelf)
                    {
                        if (GameManager.gMan.mainMenu)
                            SubmenuTemplate(settingPanel, false, -1);
                        else
                        {
                            SubmenuTemplate(settingPanel, false, -1);
                            menuCanvas.SetActive(false);
                            pauseButtonPanel.SetActive(true);
                        }
                    }
                }
                else if (backButtonIndex == RMSIndex)
                {
                    SubmenuTemplate(RMSettingPanel, true, -1);
                    TogglePanels(new GameObject[] { togglePanel, infoPanel }, false);
                }
                else if (backButtonIndex == SIndex)
                {
                    SubmenuTemplate(settingPanel, true, -2);
                    TogglePanels(new GameObject[] { gamePanel, controlPanel, displayPanel, graphicPanel, audioPanel }, false);
                }
                break;
            case "No":
                ShowPrompt(false);
                break;
            case "Toggles":
                SubmenuTemplate(RMSettingPanel, false, 0);
                SubmenuTemplate(togglePanel, true, 1);
                break;
            case "Info":
                SubmenuTemplate(RMSettingPanel, false, 0);
                SubmenuTemplate(infoPanel, true, 1);
                break;
            case "Game":
                SubmenuTemplate(settingPanel, false, 0);
                SubmenuTemplate(gamePanel, true, 2);
                break;
            case "Controls":
                SubmenuTemplate(settingPanel, false, 0);
                SubmenuTemplate(controlPanel, true, 2);
                break;
            case "Display":
                SubmenuTemplate(settingPanel, false, 0);
                SubmenuTemplate(displayPanel, true, 2);
                break;
            case "Graphics":
                SubmenuTemplate(settingPanel, false, 0);
                SubmenuTemplate(graphicPanel, true, 2);
                break;
            case "Audio":
                SubmenuTemplate(settingPanel, false, 0);
                SubmenuTemplate(audioPanel, true, 2);
                break;
        }
    }

    private void SubmenuTemplate(GameObject panel, bool state, int index)
    {
        // Control back button and title. 
        backButton.SetActive(state);
        gameTitle.SetActive(!state);
        mainButtonPanel.SetActive(!state);

        // When navigating through the menu, increment or decrement the 'backButtonIndex'
        backButtonIndex += index;

        // If there is no panel to change then this should be null.
        if (panel != null)
            panel.SetActive(state);
    }

    private void TogglePanels(GameObject[] panels, bool state)
    {
        foreach (GameObject panel in panels)
        {
            if (panel.activeSelf)
                panel.SetActive(state);
        }
    }

    public void Resume()
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

        if (togglePanel.activeSelf)
            promptYes.onClick.AddListener(() => ResetRespiteMechanics());

        if (audioPanel.activeSelf)
            promptYes.onClick.AddListener(() => ResetAudioValues());
    }

    private void ResetRespiteMechanics()
    {
        Toggle[] respiteToggles = GetComponent<ToggleController>().respiteToggles;
        for (int i = 0; i < respiteToggles.Length; i++)
        {
            respiteToggles[i].isOn = false;
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
        ShowPrompt(true, "ARE YOU SURE YOU WANT TO RESTART THE GAME?\nALL UNSAVED PROGRESS WILL BE LOST.");
        promptYes.onClick.AddListener(() => RestartGame());
    }

    public void P_MainMenu()
    {
        ShowPrompt(true, "ARE YOU SURE YOU WANT TO EXIT TO MAIN MENU?\nALL UNSAVED PROGRESS WILL BE LOST.");
        promptYes.onClick.AddListener(() => MainMenu());
    }

    public void P_QuitGame()
    {
        ShowPrompt(true, "ARE YOU SURE YOU WANT TO QUIT THE GAME?");
        promptYes.onClick.AddListener(() => QuitGame());
    }

    #region Restart Checks

    void RestartGame()
    {
        // Reset Functions
        playerController.isPaused = false;
        playerController.pauseScreen.SetActive(false);
        //stateDrivenCameraAnimator.Play("Main Menu");
        //playerCanvas.SetActive(false);
        //menuCanvas.SetActive(true);
        //GameManager.gMan.mainMenu = true;
        
        //stateDrivenCameraAnimator.Play("Player");
        //menuCanvas.SetActive(false);
        //playerCanvas.SetActive(true);
        playerController.LockUser(false);
        //GameManager.gMan.mainMenu = false;
        //GameManager.gMan.PlayerActionMap(true);
        GameManager.gMan.PlayerActionMap(true);

        ResetTime();
        ResetAllObjects();
        ResetEndGame();
        playerController.ResetPlayer();

        // Disable the Enemy
        if (timeManager.enemy.activeSelf)
            timeManager.enemy.SetActive(false);

        // Reset the Skulls
        GameManager.gMan.InstantiateSkulls();

        ShowPrompt(false);
    }

    void MainMenu() // Consider if you need all of these checks.
    {
        // Reset Functions
        ResetUI();
        ResetTime();
        ResetAllObjects();
        ResetEndGame();
        playerController.ResetPlayer();

        // Disable the Enemy
        if (timeManager.enemy.activeSelf)
            timeManager.enemy.SetActive(false);

        // Reset the Skulls
        GameManager.gMan.InstantiateSkulls();

        ShowPrompt(false);
    }

    void ResetUI()
    {
        playerController.isPaused = false;
        playerController.pauseScreen.SetActive(false);
        stateDrivenCameraAnimator.Play("Main Menu");
        playerCanvas.SetActive(false);
        menuCanvas.SetActive(true);
        GameManager.gMan.mainMenu = true;
        GameManager.gMan.PlayerActionMap(false);
    }

    void ResetTime()
    {
        Time.timeScale = 1f;
        timeManager.ResetTime();
        if (timeManager.accessPoint.isTimePaused)
            timeManager.accessPoint.isTimePaused = false;
    }

    void ResetEndGame()
    {
        if (GameManager.gMan.endGamePanel.activeSelf)
        {
            if (GameManager.gMan.gameWon)
            {
                GameManager.gMan.gameWon = false;
                GameManager.gMan.endGamePanel.transform.GetChild(0).gameObject.SetActive(false);
            }
            else
            {
                GameManager.gMan.endGamePanel.transform.GetChild(1).gameObject.SetActive(false);
            }
            GameManager.gMan.endGamePanel.SetActive(false);
        }
    }

    void ResetAllObjects()
    {
        for (int i = 0; i < interactableObjects.Length; i++)
        {
            // Reset or perform some operation on interactableObjects[i] here
            interactableObjects[i].ResetPositionAndRotation();
        }
    }

    void QuitGame()
    {
        Application.Quit();
        ShowPrompt(false);
    }

    #endregion

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