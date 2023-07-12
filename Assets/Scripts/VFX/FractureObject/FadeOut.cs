using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOut : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Renderer[] childMat;
    void Start()
    {
        for (int i = 0; i < childMat.Length; i++)
        {
            StartCoroutine(FadeAlphaToZero(childMat[i].material, 8f));
        }
        Destroy(gameObject,8f);
    }

    IEnumerator FadeAlphaToZero(Material materials, float duration)
    {
        float time = 0;
        float targetAlpha = 0;
        
        while (materials.color.a > targetAlpha)
        {
            if (materials.HasProperty("_BaseColor"))
            {
                materials.color = new Color(
                    materials.color.r,
                    materials.color.g,
                    materials.color.b,
                    Mathf.Lerp(1, targetAlpha, time / duration)
                    );
            }
            time += Time.deltaTime;

            yield return null;
        }


    }
    // Update is called once per frame
    
}
