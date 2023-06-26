using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Panda;

public class VillagerBehaviour : MonoBehaviour
{
    public VillageBehaviour villageBehaviour;
    public NavMeshAgent agent;

    [SerializeField] Transform closestHouse;
    [SerializeField] bool isAware;
    [SerializeField] bool isUnloaded;
    [SerializeField] bool asBolts;
    [SerializeField] int boltQuantity = 0;
    [SerializeField] int boltMax = 5;
    [SerializeField] float updateStateFrequency;
    [SerializeField] float updateAsArrivedFrequency;
    public States state;
    public enum States
    {
        GetBolts,
        GettingBolts,
        Reload,
        Reloading,
        Idle
    }
    private void Awake()
    {
        state = States.Idle;
        agent = GetComponent<NavMeshAgent>();
        Coroutine coroutine = StartCoroutine(UpdateState());
    }

    
    IEnumerator UpdateState()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateStateFrequency);
            
        }
    }
    private void GetBolts()
    {
        // Claim First House As Closest
        closestHouse = villageBehaviour.houses[0].transform;
        float distance = Vector3.Distance(villageBehaviour.houses[0].transform.position, transform.position);

        //Check First House
        for (int i = 1; i < villageBehaviour.houses.Count; i++)
        {
            float newDistance = Vector3.Distance(villageBehaviour.houses[i].transform.position, transform.position);
            if (distance > newDistance)
            {
                distance = newDistance;
                closestHouse = villageBehaviour.houses[i].transform;
            }
        }
        agent.SetDestination(closestHouse.position);
    }
    private void Reload()
    {

    }
    IEnumerator GettingBolts()
    {
        while (true)
        {
        }
    }
    IEnumerator Reloading()
    {
        while (true)
        {

        }
    }
    IEnumerator AsArrivedUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateAsArrivedFrequency);
            AsArrived();
        }
    }
    [Task]
    private bool AsArrived()
    {
        return agent.remainingDistance <= agent.stoppingDistance;
    }
    [Task]
    private bool AsBolts()
    {
        return boltQuantity != 0;
    }
    private bool IsUnloaded()
    {
        return true;
    }
    private bool IsAware()
    {
        return true;
    }
}
