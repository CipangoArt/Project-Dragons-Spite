using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeCountDown : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text_Speed;
    [SerializeField] private float maxTimer;

    private float currentTimer;

    private void Awake()
    {
        currentTimer = maxTimer;
    }
    private void Update()
    {
        UI();
        CountDown();
    }
    public void GainTime(float timeGain)
    {
        currentTimer += timeGain;
    }
    public void CountDown()
    {
        currentTimer -= UnityEngine.Time.deltaTime;
    }
    public void UI()
    {
        text_Speed.text = "Time: " + currentTimer;
    }
}
