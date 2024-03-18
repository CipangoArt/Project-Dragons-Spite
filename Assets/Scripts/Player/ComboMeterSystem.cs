using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ComboMeterSystem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI comboText;

    public static Action comboMeter;

    int comboCount = 0;

    Coroutine coroutine;

    private void Start()
    {
        comboText.gameObject.SetActive(false);
        comboMeter += Combo;
    }
    void Combo()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        comboText.gameObject.SetActive(true);
        comboCount++;
        comboText.text = comboCount.ToString();
        coroutine = StartCoroutine(ComboCountLoose());
    }
    IEnumerator ComboCountLoose()
    {
        yield return new WaitForSeconds(5);
        while (comboCount > 0)
        {
            comboCount--;
            comboText.text = comboCount.ToString();
            yield return new WaitForSeconds(.05f);
        }
        comboText.gameObject.SetActive(false);
    }
}
