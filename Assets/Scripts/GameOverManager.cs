using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager instance;

    TimeManager timeManager;
    [SerializeField] GameObject timeOut;
    [SerializeField] GameObject destroyedVillages;
    [SerializeField] GameObject newHighScore;

    private void Awake()
    {
        instance ??= this;
        timeManager = GameObject.Find("--Managers--").GetComponent<TimeManager>();
    }
    private void OnEnable()
    {
        timeManager.OnTimeOut += OnTimeOut;
    }
    private void OnDisable()
    {
        timeManager.OnTimeOut -= OnTimeOut;
    }
    public void OnTimeOut()
    {
        Cursor.lockState = CursorLockMode.None;
        timeOut.SetActive(true);
        Time.timeScale = 0;
    }
    public void OnEveryVillageDestroyed()
    {
        Cursor.lockState = CursorLockMode.None;
        destroyedVillages.SetActive(true);
        Time.timeScale = 0;
    }
    public void ReturnToMainMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene(0);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
