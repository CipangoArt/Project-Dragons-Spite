using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
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
        text_Speed.text = "" + (int)currentTimer;
    }
}
