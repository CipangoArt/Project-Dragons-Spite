using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VillageSystem : MonoBehaviour
{
    public static VillageSystem instance;

    [SerializeField] int currentVillageQuantity;

    [SerializeField] List<GameObject> villages;

    [SerializeField] Transform[] posUI;

    [SerializeField] GameObject villageUIPref;

    private void Awake()
    {
        instance ??= this;

        FindAllVillages();

        currentVillageQuantity = villages.Count;

        VillagesHUD();
    }
    public void VerifyVillageQuantity()
    {
        currentVillageQuantity--;
        if (currentVillageQuantity <= 0)
        {
            GameOverSystem.instance.OnEveryVillageDestroyed();
        }
    }
    private void VillagesHUD()
    {
        for (int i = 0; i < villages.Count; i++)
        {
            // HUD
            GameObject NewHUD = Instantiate(villageUIPref, posUI[i]);
            NewHUD.GetComponentInChildren<TextMeshProUGUI>().text = villages[i].name;
            villages[i].GetComponent<VillageBehaviour>().newHud = NewHUD.GetComponent<Image>();
            villages[i].GetComponent<VillageBehaviour>().destructablesFiller = NewHUD.transform.Find("Bar Filler").GetComponent<Image>();
            villages[i].GetComponent<VillageBehaviour>().destroyed = NewHUD.transform.Find("Destroyed").GetComponent<Image>();
            villages[i].GetComponent<VillageBehaviour>().destructablesEmpty = NewHUD.transform.Find("Bar BackGround").GetComponent<Image>();
            villages[i].GetComponent<VillageBehaviour>().villageName = NewHUD.transform.Find("Village Name").GetComponent<TextMeshProUGUI>();
        }
    }
    private void FindAllVillages()
    {
        villages.Clear();
        GameObject[] village = GameObject.FindGameObjectsWithTag("Village");
        villages.AddRange(village);
    }
}
