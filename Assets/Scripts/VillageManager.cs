using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VillageManager : MonoBehaviour
{
    [SerializeField] GameObject[] villages;

    [SerializeField] Transform[] posUI;

    [SerializeField] GameObject villageUIPref;

    private void Awake()
    {
        for (int i = 0; i < villages.Length; i++)
        {
            // HUD
            GameObject NewHUD = Instantiate(villageUIPref, posUI[i]);
            NewHUD.GetComponentInChildren<TextMeshProUGUI>().text = villages[i].name;
            villages[i].GetComponent<VillageBehaviour>().newHud = NewHUD.GetComponent<Image>();
            villages[i].GetComponent<VillageBehaviour>().destructablesFiller = NewHUD.transform.Find("Bar Filler").GetComponent<Image>();
            villages[i].GetComponent<VillageBehaviour>().destructablesMinThresholdFiller = NewHUD.transform.Find("Bar Threshold").GetComponent<Image>();
            villages[i].GetComponent<VillageBehaviour>().destructablesBarBackground = NewHUD.transform.Find("Bar BackGround").GetComponent<Image>();
            villages[i].GetComponent<VillageBehaviour>().villageName = NewHUD.transform.Find("Village Name").GetComponent<TextMeshProUGUI>();
        }
    }
}
