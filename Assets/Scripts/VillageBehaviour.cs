using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using System;

public class VillageBehaviour : MonoBehaviour
{
    [SerializeField] private bool isInVillage;
    [SerializeField] private float verifyInterval;
    [SerializeField] private float distanceToEnterVillage;
    private Transform player;

    public event Action OnVillageEnter;
    public event Action OnVillageExit;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(VerifyPlayerDistance(verifyInterval));
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
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, distanceToEnterVillage);
    }
}
