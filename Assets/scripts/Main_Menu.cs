using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_Menu : MonoBehaviour
{
    public static bool isPlayerAI;
    
    public void LoadPlayerGame()
    {
        isPlayerAI = false;
        SceneManager.LoadScene(1);              
    }

    public void LoadAIGame()
    {
        isPlayerAI = true;
        SceneManager.LoadScene(1);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void RestartGame()
    {
        if(isPlayerAI)
        {
            LoadAIGame();
        }
        else
        {
            LoadPlayerGame();
        }
    }

    public void Exit()
    {
        Application.Quit();
    }

}
