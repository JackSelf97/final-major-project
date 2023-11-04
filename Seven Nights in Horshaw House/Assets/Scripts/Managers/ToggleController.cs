using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ToggleController : MonoBehaviour
{
    public Toggle[] respiteToggles = new Toggle[0];

    private Dictionary<string, Action<bool>> toggleActions = new Dictionary<string, Action<bool>>();

    private void Start()
    {
        // Populate the dictionary with Toggle names from the editor and corresponding actions
        toggleActions.Add("Cutscenes_Toggle", T_Cutscenes);
        toggleActions.Add("Health_Toggle", T_Health);
        toggleActions.Add("Hints_Toggle", T_Hints);
        toggleActions.Add("Dialogue_Toggle", T_Dialogue);
        toggleActions.Add("Load_Toggle", T_Load);
        toggleActions.Add("Pause_Toggle", T_Pause);
        toggleActions.Add("Death_Toggle", T_Death);
        toggleActions.Add("QTE_Toggle", T_QTE);
        toggleActions.Add("SafeRooms_Toggle", T_SafeRooms);
        toggleActions.Add("SavePoints", T_Save);
        toggleActions.Add("Inventory_Toggle", T_Inventory);
        toggleActions.Add("Tutorial_Toggle", T_Tutorial);
        
        // Attach methods to the Toggles' onValueChanged events using a loop
        for (int i = 0; i < respiteToggles.Length; i++)
        {
            int toggleIndex = i; // Capture the current index in a variable to avoid closure issues
            respiteToggles[i].onValueChanged.AddListener(value => ToggleValueChanged(respiteToggles[toggleIndex], value));
        }
    }

    private void ToggleValueChanged(Toggle toggle, bool value)
    {
        // Get the name of the Toggle
        string toggleName = toggle.gameObject.name;

        // Check if the Toggle name exists in the dictionary
        if (toggleActions.TryGetValue(toggleName, out var action))
        {
            // Call the corresponding action with the new value
            action(value);
        }
        else
        {
            Debug.LogWarning($"Toggle name '{toggleName}' not found in dictionary.");
        }
    }

    #region Toggle Functions

    public void T_Cutscenes(bool value)
    {
        GameManager.gMan.cutscenesCheck = value;
    }

    public void T_Health(bool value)
    {
        GameManager.gMan.healthCheck = value;
    }

    public void T_Hints(bool value)
    {
        GameManager.gMan.hintsCheck = value;
    }

    public void T_Dialogue(bool value)
    {
        GameManager.gMan.dialogueCheck = value;
    }
    
    public void T_Load(bool value)
    {
        GameManager.gMan.loadCheck = value;
    }
    
    public void T_Pause(bool value)
    {
        GameManager.gMan.pauseCheck = value;
    }
    
    public void T_Death(bool value)
    {
        GameManager.gMan.deathCheck = value;
    }
    
    public void T_QTE(bool value)
    {
        GameManager.gMan.quickTimeEventsCheck = value;
    }
    
    public void T_SafeRooms(bool value)
    {
        GameManager.gMan.safeRoomsCheck = value;
    }
    
    public void T_Save(bool value)
    {
        GameManager.gMan.savePointsCheck = value;
    }
    
    public void T_Inventory(bool value)
    {
        GameManager.gMan.staticInventoryCheck = value;
    }
    
    public void T_Tutorial(bool value)
    {
        GameManager.gMan.tutorialCheck = value;
    }

    #endregion
}