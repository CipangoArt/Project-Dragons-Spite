using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject gameOptions;
    [SerializeField] GameObject options;

    public void StartGame()
    {
        mainMenu.SetActive(false);
        gameOptions.SetActive(true);
    }
    public void GoBack()
    {
        mainMenu.SetActive(true);
        gameOptions.SetActive(false);
        options.SetActive(false);
    }
    public void Options()
    {
        mainMenu.SetActive(false);
        options.SetActive(true);
    }
    public void Gameplay()
    {
        SceneManager.LoadScene(1);
    }
    public void Tutorial()
    {
        SceneManager.LoadScene(1);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
