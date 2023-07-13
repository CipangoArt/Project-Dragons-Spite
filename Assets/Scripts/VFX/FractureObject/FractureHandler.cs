using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractureHandler : MonoBehaviour
{
    [SerializeField] GameObject DestroyVFX;
    [SerializeField] GameObject DamageVFX;
    [SerializeField] Transform centerPoint;
    // Start is called before the first frame update
    public void FractureHouse()
    {

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<FractureObject>() != null)
            {
                transform.GetChild(i).GetComponent<FractureObject>().Fracture();
                
            }
            
        }
        Instantiate(DestroyVFX, centerPoint.position, Quaternion.identity);

    }

    public void DamageHouse()
    {
        Instantiate(DamageVFX, centerPoint.position, Quaternion.identity);
    }
}
