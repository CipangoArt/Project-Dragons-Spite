using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class FractureObject : MonoBehaviour
{
    [SerializeField] private string[] sounds;
    private GameObject originalObject;
    [SerializeField] private GameObject fracturedObject;
    [SerializeField] private float explosionMinForce = 5;
    [SerializeField] private float explosionMaxForce = 100;
    [SerializeField] private float explosionForceRadius = 10;
    public bool isThrown = false;
    [SerializeField] bool isbreakable = true;
    [SerializeField] GameObject DestroyVFX;
    private GameObject fractObj;

    private Rigidbody rb;

    private void Start()
    {
        originalObject = gameObject;
    }
    public void Fracture()
    {

        if (sounds.Length != 0)
        {
            AudioManager.instance.PlaySound(sounds[Random.Range(0, sounds.Length - 1)]);
        }
        if (isbreakable)
        {
            if (originalObject != null)
            {
                originalObject.SetActive(false);
                if (fracturedObject != null)
                {
                    GameObject fractObj = Instantiate(fracturedObject, originalObject.transform.position, Quaternion.identity) as GameObject;
                    foreach (Transform t in fractObj.transform)
                    {
                        rb = t.GetComponent<Rigidbody>();
                        if (rb != null)
                            rb.AddExplosionForce(Random.Range(explosionMinForce, explosionMaxForce), originalObject.transform.position, explosionForceRadius);
                        
                      
                    }
                }
            }
        }

    }
   

    
  

   
        private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isThrown = false;
            Fracture();
            Instantiate(DestroyVFX, transform.position, Quaternion.identity);
            if (collision.gameObject.GetComponent<FractureObject>() != null)
            {
                collision.gameObject.GetComponent<FractureObject>().Fracture();
            }
            
        }
    }

    private void OnDestroy()
    {
        
    }
}