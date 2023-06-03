using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    Rigidbody rb;
    PlayerInput playerInput;
    Gauge gauge;

    [SerializeField] public int gaugeCost;

    [SerializeField] private GameObject fireBallPref;
    [SerializeField] private Transform fireBallSpawn;
    [SerializeField] private Vector3 mouseWorldPosition;
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();

    private void Awake()
    {
        gauge = GetComponent<Gauge>();
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        playerInput.OnFireBall += SpawnFireBall;
    }
    private void SpawnFireBall()
    {
        if (playerInput.isAiming && gaugeCost <= gauge.currentGauge)
        {
            gauge.LoseGauge(gaugeCost);
            FindHitTarget();
            InstantiateBall();
        }
    }
    private void InstantiateBall()
    {
        Vector3 aimDir = (mouseWorldPosition - fireBallSpawn.position).normalized;
        GameObject NewBall = Instantiate(fireBallPref, fireBallSpawn.position, Quaternion.LookRotation(aimDir, Vector3.up));
        NewBall.GetComponent<FireBallPref>().initialVelocity = rb.velocity.magnitude;
    }
    private void FindHitTarget()
    {
        mouseWorldPosition = Vector3.zero;
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            mouseWorldPosition = raycastHit.point;
        }
    }
}
