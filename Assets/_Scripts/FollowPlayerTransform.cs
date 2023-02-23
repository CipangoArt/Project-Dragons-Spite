using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerTransform : MonoBehaviour
{
    [SerializeField] Transform playerTrans;

    private void Update()
    {
        transform.position = playerTrans.position;
    }
}
