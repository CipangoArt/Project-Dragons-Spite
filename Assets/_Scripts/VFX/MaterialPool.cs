using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialPool : MonoBehaviour
{
    [SerializeField] Material FirebalExplosionZ;
    [SerializeField] Material FirebalExplosionX;
    [SerializeField] Material FirebalExplosionRender;

   public Dictionary<string, Material> materialNames = new Dictionary<string, Material>();

    public static MaterialPool Instance { get; private set; }
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        materialNames.Add("FirebalExplosionZ", FirebalExplosionZ);
        materialNames.Add("FirebalExplosionX", FirebalExplosionX);
        materialNames.Add("FirebalExplosionRender", FirebalExplosionRender);

        DontDestroyOnLoad(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
