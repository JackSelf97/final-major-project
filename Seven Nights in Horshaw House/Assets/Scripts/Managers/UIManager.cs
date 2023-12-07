using System.Collections;
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
    private GameObject enemy = null;
    private EnemyController enemyController = null;
    private InteractableObject[] interactableObjects = new InteractableObject[0];
    private Door[] interactableDoors = new Door[0];

    [Header("Managers")]
    [SerializeField] private TimeManager timeManager = null;
    private AudioManager audioManager = null;

    [Header("General Menu Properties")]
    [SerializeField] private Button backButton = null;
    public int backButtonIndex = 0;

    [Header("Player Menu Properties")]
    [SerializeField] private GameObject pauseButtonPanel = null;

    [Header("Settings")]
    [SerializeField] private GameObject settingPanel = null;

    [Header("Settings/Game")]
    [SerializeField] private GameObject gamePanel = null;
    [SerializeField] private Toggle classicModeToggle = null;

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
    public GameObject promptPanel = null;
    [SerializeField] private Text promptText = null;
    [SerializeField] private Button promptYes = null;

    // Credits
    private Credits credits = null;

    private readonly int RMSIndex = 2, SIndex = 3, CIndex = 4;

    // Start is called before the first frame update
    void Start()
    {
        InitialiseGame();
    }

    private void InitialiseGame()
    {
        // Locate GameObjects & Scripts
        player = GameObject.FindWithTag("Player");
        enemy = GameObject.FindWithTag("Enemy");
        playerController = player.GetComponent<PlayerController>();
        enemyController = enemy.GetComponent<EnemyController>();
        timeManager = FindObjectOfType<TimeManager>();
        interactableObjects = FindObjectsOfType<InteractableObject>();
        interactableDoors = FindObjectsOfType<Door>();
        credits = GetComponent<Credits>();
        audioManager = FindObjectOfType<AudioManager>();

        // Player setup
        playerController.LockUser(true);
        SubmenuTemplate(null, false, 0);
        GameManager.gMan.PlayerActionMap(false);

        // Game
        classicModeToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    // Anything regarding "Prompts" will not be used inside the switch statement.
    public void SwitchMenu(string menuName)
    {
        switch (menuName)
        {
            case "Play":
                // Check if the disclaimer is enabled
                if (GameManager.gMan.isDisclaimerOn)
                {
                    // Show the disclaimer panel
                    ShowDisclaimer();
                }
                else
                {
                    // Skip the disclaimer and start the game immediately
                    PlayAction();
                }
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
            case "Credits":
                SubmenuTemplate(null, true, CIndex);
                backButton.onClick.AddListener(() => credits.SkipCredits());
                break;
        }
    }

    private void SubmenuTemplate(GameObject panel, bool state, int index)
    {
        // Control back button and title. 
        backButton.gameObject.SetActive(state);
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

    private void PlayAction()
    {
        stateDrivenCameraAnimator.Play("Player");
        menuCanvas.SetActive(false);
        playerCanvas.SetActive(true);
        playerController.LockUser(false);

        GameManager.gMan.mainMenu = false;
        GameManager.gMan.PlayerActionMap(true);
        GameManager.gMan.InstantiateSkulls();

        // Manage the soundtrack
        audioManager.Stop("Horshaw Theme");
        audioManager.Play("House");

        // Testing mode
        if (GameManager.gMan.testingEnemy)
            GameManager.gMan.EnableEnemy();
    }

    private void ShowDisclaimer()
    {
        GameManager.gMan.disclaimerPanel.SetActive(true);

        // Start a coroutine to hide the disclaimer after a specified duration
        StartCoroutine(HideDisclaimerAfterDelay(GameManager.gMan.disclaimerDuration));
    }

    private IEnumerator HideDisclaimerAfterDelay(float delay)
    {
        // Wait for the specified duration
        yield return new WaitForSeconds(delay);

        // Hide the disclaimer panel
        GameManager.gMan.disclaimerPanel.SetActive(false);

        // Start the game or load the main scene
        PlayAction();
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
        else if (backButtonIndex == CIndex)
        {
            ResetCredits();
        }
    }

    #endregion

    #region Functions w/ Prompts

    public void ShowPrompt(bool state, string prompt = "")
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

        if (gamePanel.activeSelf)
            promptYes.onClick.AddListener(() => ResetGameValues());

        if (audioPanel.activeSelf)
            promptYes.onClick.AddListener(() => ResetAudioValues());

    }

    public void P_RestartGame()
    {
        ShowPrompt(true, "ARE YOU SURE YOU WANT TO RESTART THE GAME?\nALL UNSAVED PROGRESS WILL BE LOST.");
        promptYes.onClick.AddListener(() => RestartGame());
    }

    public void P_LoadLastCheckpoint()
    {
        ShowPrompt(true, "ARE YOU SURE YOU WANT TO LOAD THE LAST CHECKPOINT?\nDay: " + timeManager.dayStamp + " | Time: " + timeManager.timeStamp.ToString("hh:mm tt"));
        promptYes.onClick.AddListener(() => LoadLastCheckpoint());
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
        GameManager.gMan.ResetEndGame();
        playerController.ResetPlayer();

        // Jump Scare
        GameManager.gMan.JumpScareRestart();

        // Disable the Enemy
        if (enemyController.isActive)
        {
            GameManager.gMan.DisableEnemy();
            GameManager.gMan.EnableEnemy();
        }
            
        // Reset the Skulls
        GameManager.gMan.ClearSkulls();

        ShowPrompt(false);
    }

    private void LoadLastCheckpoint()
    {
        timeManager.GoToCheckpoint();
        ShowPrompt(false);
    }

    private void ExitGame()
    {
        // Reset Functions
        ResetUI();
        ResetTime();
        ResetAllObjects();
        GameManager.gMan.ResetEndGame();
        playerController.ResetPlayer();

        // Jump Scare
        GameManager.gMan.JumpScareRestart();

        // Disable the Enemy
        if (enemyController.isActive)
            GameManager.gMan.DisableEnemy();

        // Clear the Skulls
        GameManager.gMan.ClearSkulls();

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
        if (timeManager.activeAccessPoint.isTimePaused)
            timeManager.activeAccessPoint.isTimePaused = false;
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

    private void ResetGameValues()
    {
        classicModeToggle.isOn = false;
        ShowPrompt(false);
    }

    public void ResetCredits()
    {
        SubmenuTemplate(null, false, -CIndex);
        backButton.onClick.RemoveAllListeners();
    }

    private void QuitGame()
    {
        Application.Quit();
        ShowPrompt(false);
    }

    #endregion

    #region Settings/Game

    public void OnToggleChanged(bool newValue)
    {
        // Set the state of all respite mechanics based on the new value
        SetAllRespiteMechanics(newValue);
    }

    private void SetAllRespiteMechanics(bool enable)
    {
        Toggle[] respiteToggles = GetComponent<ToggleController>().respiteToggles;

        foreach (Toggle toggle in respiteToggles)
        {
            toggle.isOn = enable;
            toggle.interactable = !enable;
        }

        Debug.Log(enable ? "RM ON" : "RM OFF");
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