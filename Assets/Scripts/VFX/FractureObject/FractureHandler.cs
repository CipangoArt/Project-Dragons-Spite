using UnityEngine;

public class FractureHandler : MonoBehaviour
{
    [SerializeField] GameObject DestroyVFX;
    [SerializeField] GameObject DamageVFX;
    [SerializeField] GameObject SoulVFX;
    [SerializeField] Transform centerPoint;
    // Start is called before the first frame update
    public void FractureHouse()
    {
        if (gameObject.CompareTag("Destructable"))
        {
            if (transform.childCount <= 20)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).GetComponent<FractureObject>() != null)
                    {
                        transform.GetChild(i).GetComponent<FractureObject>().Fracture();
                    }
                }
            }
            else
            {

                for (int i = 0; i < 20; i++)
                {
                    if (transform.GetChild(i).GetComponent<FractureObject>() != null)
                    {
                        transform.GetChild(i).GetComponent<FractureObject>().Fracture();
                    }
                }
            }
            
        }
        Instantiate(DestroyVFX, centerPoint.position, Quaternion.identity);
        Instantiate(SoulVFX, centerPoint.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public void DamageHouse()
    {
        Instantiate(DamageVFX, centerPoint.position, Quaternion.identity);
    }
}
