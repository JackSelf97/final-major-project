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
    private Door[] interactableDoors = new Door[0];

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
        interactableDoors = FindObjectsOfType<Door>();

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
                ContinueAction();
                break;
            case "Respite Mechanics":
                RespiteMechanicsAction();
                break;
            case "Settings":
                SettingsAction();
                break;
            case "Back":
                BackAction();
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

    #region Switch Statement Functions

    private void TogglePanels(GameObject[] panels, bool state)
    {
        foreach (GameObject panel in panels)
        {
            if (panel.activeSelf)
                panel.SetActive(state);
        }
    }

    private void ContinueAction()
    {
        stateDrivenCameraAnimator.Play("Player");
        menuCanvas.SetActive(false);
        playerCanvas.SetActive(true);
        playerController.LockUser(false);
        GameManager.gMan.mainMenu = false;
        GameManager.gMan.PlayerActionMap(true);
    }

    private void RespiteMechanicsAction()
    {
        if (GameManager.gMan.mainMenu)
            SubmenuTemplate(RMSettingPanel, true, 1);
        else
        {
            pauseButtonPanel.SetActive(false);
            menuCanvas.SetActive(true);
            SubmenuTemplate(RMSettingPanel, true, 1);
        }
    }

    private void SettingsAction()
    {
        if (GameManager.gMan.mainMenu)
            SubmenuTemplate(settingPanel, true, 1);
        else
        {
            pauseButtonPanel.SetActive(false);
            menuCanvas.SetActive(true);
            SubmenuTemplate(settingPanel, true, 1);
        }
    }

    private void BackAction()
    {
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
    }

    #endregion

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

    public void P_RestartGame()
    {
        ShowPrompt(true, "ARE YOU SURE YOU WANT TO RESTART THE GAME?\nALL UNSAVED PROGRESS WILL BE LOST.");
        promptYes.onClick.AddListener(() => RestartGame());
    }

    public void P_ExitGame()
    {
        ShowPrompt(true, "ARE YOU SURE YOU WANT TO EXIT TO MAIN MENU?\nALL UNSAVED PROGRESS WILL BE LOST.");
        promptYes.onClick.AddListener(() => ExitGame());
    }

    public void P_QuitGame()
    {
        ShowPrompt(true, "ARE YOU SURE YOU WANT TO QUIT THE GAME?");
        promptYes.onClick.AddListener(() => QuitGame());
    }

    #region Reset Checks

    public void Resume()
    {
        playerController.isPaused = false;
        playerController.pauseScreen.SetActive(false);
        playerController.LockUser(false);
        GameManager.gMan.PlayerActionMap(true);
        Time.timeScale = 1f;
    }

    private void RestartGame()
    {
        playerController.isPaused = false;
        playerController.pauseScreen.SetActive(false);
        playerController.LockUser(false);
        GameManager.gMan.PlayerActionMap(true);

        // Reset Functions
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

    private void ExitGame()
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

    private void ResetUI()
    {
        playerController.isPaused = false;
        playerController.pauseScreen.SetActive(false);
        stateDrivenCameraAnimator.Play("Main Menu");
        playerCanvas.SetActive(false);
        menuCanvas.SetActive(true);
        GameManager.gMan.mainMenu = true;
        GameManager.gMan.PlayerActionMap(false);
    }

    private void ResetTime()
    {
        Time.timeScale = 1f;
        timeManager.ResetTimeOfDay();
        if (timeManager.accessPoint.isTimePaused)
            timeManager.accessPoint.isTimePaused = false;
    }

    private void ResetEndGame()
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

    private void ResetAllObjects()
    {
        for (int i = 0; i < interactableObjects.Length; i++)
        {
            // Reset object's position and rotation
            interactableObjects[i].ResetPositionAndRotation();
        }

        // Doors
        for (int i = 0; i < interactableDoors.Length; i++)
        {
            // Reset animation and bool
            interactableDoors[i].ResetAnimation();
        }
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

    private void QuitGame()
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

    public void SurveyLink()
    {
        Application.OpenURL("https://forms.gle/616rCeVsvSospqtY7");
    }

    #endregion
}