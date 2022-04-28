using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPlayerPrefs : MonoBehaviour
{
    public MenuController menuController;
    // All Main Menu parameters 
 
    private void Start()
    {
        menuController = GetComponent<MenuController>();
    }
    public void SaveHoverTime(float sliderValue)
    {
        PlayerPrefs.SetFloat("hovertime", sliderValue);
        PlayerPrefs.Save();
    }
    public void SaveNumPaddles(int menuInt)
    {
        PlayerPrefs.SetInt("numpaddles", menuInt);
        PlayerPrefs.Save();
    }


    // Private methods to load PlayerPrefs into the menu. 

    private void LoadHoverTimeToMenu()
    {
        if (PlayerPrefs.HasKey("hovertime"))
        {
            menuController.UpdateHoverTime(PlayerPrefs.GetFloat("hovertime"));
        }
    }

    


    

    // Clears all saved main menu preferences
    public void ResetPlayerPrefs()
    {
        Debug.Log("Reset Menu Preferences");
        PlayerPrefs.DeleteAll();
        // TODO 
        // SetDefaultSettings();
    }
}
