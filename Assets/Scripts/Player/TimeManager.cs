using System;
using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text_Speed;
    [SerializeField] private float maxTimer;
    [SerializeField] private float currentTimer;

    public event Action OnTimeOut;

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
        if (currentTimer <= 0)
        {
            OnTimeOut?.Invoke();
        }
        currentTimer -= Time.deltaTime;
    }
    public void UI()
    {
        text_Speed.text = "" + (int)currentTimer;
    }
}
