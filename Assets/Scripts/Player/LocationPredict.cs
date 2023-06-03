using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationPredict : MonoBehaviour
{
    Rigidbody rb;

    [SerializeField] private Transform debugBall;
    [SerializeField] private float offSet;
    [SerializeField] private float heightOffSet;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        PredictPos();
    }
    private void PredictPos()
    {
        Vector3 accuracy = transform.position + transform.forward * offSet * rb.velocity.magnitude;

        if (Physics.Raycast(accuracy + Vector3.up * 1000, Vector3.down, out RaycastHit hitInfo))
        {
            accuracy = hitInfo.point + Vector3.up * heightOffSet;
        }
        debugBall.position = accuracy + Random.insideUnitSphere * 2f;
    }
}
