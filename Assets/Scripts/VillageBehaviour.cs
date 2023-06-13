using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VillageBehaviour : MonoBehaviour
{
    [SerializeField] private bool isInVillage;
    [SerializeField] private float verifyInterval;
    [SerializeField] private float distanceToEnterVillage;

    [SerializeField] GameObject marker;

    private Transform player;

    public Image newHud;
    public Image destructablesMinThresholdFiller;
    public Image destructablesFiller;
    public Image destructablesBarBackground;
    public TextMeshProUGUI villageName;

    public event Action OnVillageEnter;
    public event Action OnVillageExit;

    [SerializeField] float destructablesMax;
    [SerializeField] float destructablesCurrent;
    [SerializeField] float destructablesPercentage;
    [Range(0, 1)]
    [SerializeField] float destructablesMinThreshold;


    bool hasDestructables = true;

    private void Awake()
    {
        AnaliseDestructablesQuantity();
        if (!hasDestructables) { this.enabled = false; return; }
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(VerifyPlayerDistance(verifyInterval));
    }
    private void Start()
    {
        destructablesMinThresholdFiller.fillAmount = destructablesMinThreshold;
    }
    private void Update()
    {
        Vector3 direction = player.position - marker.transform.position;

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
    private void AnaliseDestructablesQuantity()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, distanceToEnterVillage);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].gameObject.CompareTag("Destructable"))
            {
                hitColliders[i].gameObject.GetComponent<HealthManager>().villageBehaviour = this;
                hitColliders[i].gameObject.transform.SetParent(transform);
                destructablesCurrent++;
            }
            if (hitColliders[i].gameObject.CompareTag("Balista"))
            {
                hitColliders[i].gameObject.GetComponent<BalistaBehaviour>().villageBehaviour = this;
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
        UpdateHealthFiller();
    }
    public void UpdateHealthFiller()
    {
        destructablesPercentage = destructablesCurrent / destructablesMax;
        if (destructablesPercentage <= destructablesMinThreshold)
        {
            villageName.fontStyle = FontStyles.Strikethrough;

            Color imageColor = destructablesBarBackground.color;
            imageColor.a = 0.2f;
            destructablesBarBackground.color = imageColor;

            Color imageColor2 = Color.red;
            imageColor2.a = 0.2f;
            newHud.color = imageColor2;

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
