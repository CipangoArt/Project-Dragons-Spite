using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private PlayerInputSystem playerInput;
    [SerializeField] private GameObject pressShift;
    [SerializeField] private GameObject wasd;

    private void Awake()
    {
        Time.timeScale = 0;
    }
    private void Update()
    {
        if (playerInput.isInputingTurbo)
        {
            Time.timeScale = 1;
            pressShift.SetActive(false);
            wasd.SetActive(true);
        }
    }
}
