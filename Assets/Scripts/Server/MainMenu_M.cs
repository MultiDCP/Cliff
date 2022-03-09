using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu_M : MonoBehaviour {

    public MenuManager_M menu_manager;
    public int menu_num = 2;

    public void Onclick_GameStrat()
    {
        Debug.Log("Clicked Game Start");
        menu_manager.MenuChange(0);
        menu_manager.GetReadyGame();
    }
    public void Onclick_Setting()
    {
        Debug.Log("Clicked Setting");
        menu_manager.MenuChange(3);
    }

}
