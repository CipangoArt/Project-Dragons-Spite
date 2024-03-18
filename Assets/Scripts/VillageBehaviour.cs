using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class VillageBehaviour : MonoBehaviour
{
    public List<GameObject> balistas;
    public List<GameObject> houses;

    [SerializeField] public bool isInVillage;
    [SerializeField] private float verifyInterval;
    [SerializeField] private float distanceToEnterVillage;

    [SerializeField] GameObject marker;

    private Transform player;

    public Image newHud;
    public Image destructablesFiller;
    public Image destructablesEmpty;
    public Image destroyed;
    public TextMeshProUGUI villageName;

    public event Action OnVillageEnter;
    public event Action OnVillageExit;

    [SerializeField] float destructablesMax;
    [SerializeField] float destructablesCurrent;
    [SerializeField] float destructablesPercentage;
    [Range(0, 1)]
    [SerializeField] float destructablesMinThreshold;

    bool isDestroyed = false;

    bool hasDestructables = true;

    private void Awake()
    {
        GatherDestructablesQuantity();
        if (!hasDestructables) { this.enabled = false; return; }
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(VerifyPlayerDistance(verifyInterval));
    }
    private void Update()
    {
        MarkerRotation();
    }
    private void MarkerRotation()
    {
        Vector3 direction = marker.transform.position - player.position;
        direction.Normalize();

        if (direction != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            marker.transform.rotation = rotation;
        }
    }
    private IEnumerator VerifyPlayerDistance(float verifyInterval)
    {
        while (true)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance < distanceToEnterVillage && !isInVillage)
            {
                isInVillage = true;
                OnVillageEnter?.Invoke();
            }
            else if (distance > distanceToEnterVillage && isInVillage)
            {
                isInVillage = false;
                OnVillageExit?.Invoke();
            }

            yield return new WaitForSeconds(2f);
        }
    }
    private void GatherDestructablesQuantity()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, distanceToEnterVillage);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].gameObject.CompareTag("Destructable"))
            {
                hitColliders[i].gameObject.GetComponent<HealthManager>().villageBehaviour = this;
                hitColliders[i].gameObject.transform.SetParent(transform);
                houses.Add(hitColliders[i].gameObject);
                destructablesCurrent++;
            }
            if (hitColliders[i].gameObject.CompareTag("Balista"))
            {
                hitColliders[i].gameObject.GetComponent<BalistaBehaviour>().villageBehaviour = this;
                hitColliders[i].gameObject.GetComponent<HealthManager>().villageBehaviour = this;
                hitColliders[i].gameObject.transform.SetParent(transform);
                hitColliders[i].gameObject.GetComponent<BalistaBehaviour>().OnVillageBegin();
                balistas.Add(hitColliders[i].gameObject);
                destructablesCurrent++;
            }
            if (hitColliders[i].gameObject.CompareTag("Villager"))
            {
                hitColliders[i].gameObject.GetComponent<VillagerBehaviour>().villageBehaviour = this;
                hitColliders[i].gameObject.GetComponent<HealthManager>().villageBehaviour = this;
                hitColliders[i].gameObject.transform.SetParent(transform);
                destructablesCurrent++;
            }
        }
        destructablesMax = destructablesCurrent;
        if (destructablesMax == 0) hasDestructables = false;
    }
    public void LostDestructables()
    {
        destructablesCurrent--;
        UpdateDestructableQuantity();
    }
    public void UpdateDestructableQuantity()
    {
        destructablesPercentage = destructablesCurrent / destructablesMax;

        if (destructablesPercentage <= destructablesMinThreshold && !isDestroyed)
        {
            isDestroyed = true;
            VillageSystem.instance.VerifyVillageQuantity();

            Color imageColor1 = villageName.color;
            imageColor1.a = 0.3f;
            villageName.color = imageColor1;
            villageName.fontStyle = FontStyles.Strikethrough;

            Color imageColor2 = newHud.color;
            imageColor2.a = 0f;
            newHud.color = imageColor2;

            destructablesEmpty.gameObject.SetActive(false);
            destructablesFiller.gameObject.SetActive(false);
            destroyed.gameObject.SetActive(true);
            marker.SetActive(false);
        }
        destructablesFiller.fillAmount = destructablesPercentage;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, distanceToEnterVillage);
    }
}
