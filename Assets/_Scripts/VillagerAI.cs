using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class VillagerAI : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Dragon")
        {
            anim.SetTrigger("DragonDetect");
            Invoke("SetDest", 1F);
        }
       
    }


    public void SetDest()
    {
        agent.SetDestination(target.position);
        agent.speed = 8;
    }
}
