using Panda;
using UnityEngine;
using UnityEngine.AI;

public class VillagerBehaviour : MonoBehaviour
{
    [SerializeField] private Animator anim;
    public VillageBehaviour villageBehaviour;
    public NavMeshAgent agent;


    [SerializeField] Transform houseTarget;
    [SerializeField] Transform balistaTarget;

    [SerializeField] bool isAware;
    [SerializeField] bool isUnloaded;
    [SerializeField] bool asBolts;
    [SerializeField] int currentBoltAmount = 0;
    [SerializeField] int maxBoltAmount = 3;
    [SerializeField] float updateStateFrequency;
    [SerializeField] float updateAsArrivedFrequency;

    private void Update()
    {
        anim.SetFloat("Move", agent.velocity.magnitude);
    }
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    [Task]
    public bool IsVillagerFullOfBolts()
    {
        Debug.Log(currentBoltAmount == maxBoltAmount);
        return currentBoltAmount == maxBoltAmount;
    }
    [Task]
    public bool IsBalistaFullOfBolts()
    {
        BalistaBehaviour balistaBehaviour = balistaTarget.GetComponent<BalistaBehaviour>();
        return balistaBehaviour.currentBoltAmount == balistaBehaviour.maxBoltAmount;
    }
    //Actions
    [Task]
    public void Idle()
    {
        anim.SetBool("Interact", false);
    }
    [Task]
    public void GetBolts()
    {
        anim.SetBool("Interact", false);
        agent.SetDestination(houseTarget.position);
        ThisTask.Succeed();
    }
    [Task]
    public void GettingBolts()
    {
        anim.SetBool("Interact", true);
        currentBoltAmount++;
        ThisTask.Succeed();
    }
    [Task]
    public void GoReload()
    {
        anim.SetBool("Interact", false);
        agent.SetDestination(balistaTarget.position);
        ThisTask.Succeed();
    }
    [Task]
    public void Reloading()
    {
        anim.SetBool("Interact", true);
        if (balistaTarget.Equals(null))
        {
            ThisTask.Fail();
            return;
        }
        currentBoltAmount--;
        balistaTarget.GetComponent<BalistaBehaviour>().currentBoltAmount++;
        ThisTask.Succeed();
    }
    //Bolleans
    [Task]
    public void AsArrived()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            ThisTask.Succeed();
        }
    }
    [Task]
    public bool AsBolts()
    {
        return currentBoltAmount > 0;
    }
    [Task]
    public bool IsAware()
    {
        return villageBehaviour.isInVillage;
    }
    [Task]
    public void ChooseRandomHouse()
    {
        int randomHouse = Random.Range(0, villageBehaviour.houses.Count);
        if (villageBehaviour.houses[randomHouse].Equals(null))
        {
            ThisTask.Fail();
            return;
        }
        houseTarget = villageBehaviour.houses[randomHouse].transform;
        ThisTask.Succeed();
    }
    [Task]
    public void ChooseRandomBalista()
    {
        int randomBalista = Random.Range(0, villageBehaviour.balistas.Count);
        if (villageBehaviour.balistas[randomBalista].Equals(null))
        {
            ThisTask.Fail();
            return;
        }
        balistaTarget = villageBehaviour.balistas[randomBalista].transform;
        ThisTask.Succeed();
    }

    private void OnDrawGizmos()
    {
        if (agent != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, agent.destination);
        }
    }
}
