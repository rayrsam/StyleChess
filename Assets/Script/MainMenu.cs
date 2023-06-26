using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    //[SerializeField] private GameObject menu;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject playMenu;
    
    
    
    public void OnPlay()
    {
        mainMenu.SetActive(false);
        playMenu.SetActive(true);
    }

    public void OnPlayLocal()
    {
        SceneManager.LoadScene(1);
    }

    public void OnPlayOnline()
    {
        
    }

    public void OnPlayBack()
    {
        playMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
    
    public void OnBuild()
    {
        SceneManager.LoadScene(2);
    }

    public void OnExit()
    {
        Application.Quit();
    }
}
