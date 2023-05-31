using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    [Header("Press P to trigger VFX prefab in the sphere next to the well in the map")]
    [SerializeField] Transform spawnPos;
    [SerializeField] GameObject VFX_to_Debug;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Instantiate(VFX_to_Debug, spawnPos.position,VFX_to_Debug.transform.rotation);
            Debug.Log("Debug VFX Triggered");
        }
    }
}
