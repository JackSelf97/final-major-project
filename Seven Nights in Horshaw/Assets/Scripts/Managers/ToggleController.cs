using UnityEngine;
using UnityEngine.UI;

public class ToggleController : MonoBehaviour
{
    [SerializeField] private Toggle cutscenesToggle;
    [SerializeField] private Toggle healthToggle;
    [SerializeField] private Toggle hintsToggle;
    [SerializeField] private Toggle interactiveDialogueToggle;
    [SerializeField] private Toggle loadingToggle;
    [SerializeField] private Toggle pauseToggle;
    [SerializeField] private Toggle deathToggle;
    [SerializeField] private Toggle quickTimeEventsToggle;
    [SerializeField] private Toggle safeRoomsToggle;
    [SerializeField] private Toggle savePointsToggle;
    [SerializeField] private Toggle staticInventoryToggle;
    [SerializeField] private Toggle tutorialToggle;

    private void Start()
    {
        // Attach methods to the Toggles' onValueChanged events
        cutscenesToggle.onValueChanged.AddListener(ToggleCutscenes);
        healthToggle.onValueChanged.AddListener(ToggleHealth);
        hintsToggle.onValueChanged.AddListener(ToggleHints);
        interactiveDialogueToggle.onValueChanged.AddListener(ToggleInteractiveDialogue);
        loadingToggle.onValueChanged.AddListener(ToggleLoading);
        pauseToggle.onValueChanged.AddListener(TogglePause);
        deathToggle.onValueChanged.AddListener(ToggleDeath);
        quickTimeEventsToggle.onValueChanged.AddListener(ToggleQuickTimeEvents);
        safeRoomsToggle.onValueChanged.AddListener(ToggleSafeRooms);
        savePointsToggle.onValueChanged.AddListener(ToggleSavePoints);
        staticInventoryToggle.onValueChanged.AddListener(ToggleStaticInventory);
        tutorialToggle.onValueChanged.AddListener(ToggleTutorial);
    }

    public void ToggleCutscenes(bool value)
    {
        GameManager.gMan.cutscenes = value;
    }

    public void ToggleHealth(bool value)
    {
        GameManager.gMan.healthRecovery = value;
    }

    public void ToggleHints(bool value)
    {
        GameManager.gMan.hints = value;
    }

    public void ToggleInteractiveDialogue(bool value)
    {
        GameManager.gMan.interactiveDialogue = value;
    }
    
    public void ToggleLoading(bool value)
    {
        GameManager.gMan.loadingScreens = value;
    }
    
    public void TogglePause(bool value)
    {
        GameManager.gMan.pauseScreen = value;
    }
    
    public void ToggleDeath(bool value)
    {
        GameManager.gMan.playerDeath = value;
    }
    
    public void ToggleQuickTimeEvents(bool value)
    {
        GameManager.gMan.quickTimeEvents = value;
    }
    
    public void ToggleSafeRooms(bool value)
    {
        GameManager.gMan.safeRooms = value;
    }
    
    public void ToggleSavePoints(bool value)
    {
        GameManager.gMan.savePoints = value;
    }
    
    public void ToggleStaticInventory(bool value)
    {
        GameManager.gMan.staticInventory = value;
    }
    
    public void ToggleTutorial(bool value)
    {
        GameManager.gMan.tutorialSections = value;
    }
}