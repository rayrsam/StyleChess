using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnPlay()
    {
        SceneManager.LoadScene(1);
    }
    
    public void OnBuild(int army)
    {
        RosterHolder.RedactedArmy = army;
        SceneManager.LoadScene(2);
    }

    public void OnExit()
    {
        Application.Quit();
    }
}
