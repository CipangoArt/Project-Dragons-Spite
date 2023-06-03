using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialInstancer : MonoBehaviour
{
    [Header("ParticleSystemRenderers")]
    [SerializeField] ParticleSystemRenderer[] psRenderer;
    float timer = 0;
    [Header("Materials to be Instanced (copy names from MaterialPool)")]
    [SerializeField] string[] materialNames;


    [SerializeField] int numberOfMaterials;
    int currentref;
    // Start is called before the first frame update


    private void Awake()
    {
        for (int i = 0; i < numberOfMaterials; i++)
        {
            Material resultMaterial;
            if (MaterialPool.Instance.materialNames.TryGetValue(materialNames[i], out resultMaterial))
            {
                psRenderer[i].material = resultMaterial;
                psRenderer[i].material.SetFloat("_Timer", 0);

            }

        }


    }

    private void Update()
    {
        timer += Time.deltaTime;
        for (int i = 0; i < numberOfMaterials; i++)
        {
            psRenderer[i].material.SetFloat("_Timer", timer);
        }
           
    }
}
