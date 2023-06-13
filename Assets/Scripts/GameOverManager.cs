using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    TimeManager timeManager;

    private void Awake()
    {
        timeManager = GameObject.Find("Time Manager").GetComponent<TimeManager>();
    }
    private void OnEnable()
    {
        timeManager.OnTimeOut += GameOverScreen;
    }
    private void OnDisable()
    {
        timeManager.OnTimeOut -= GameOverScreen;
    }
    public void GameOverScreen()
    {

    }
}
