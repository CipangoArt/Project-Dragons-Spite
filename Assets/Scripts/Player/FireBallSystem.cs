using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallSystem : MonoBehaviour
{
    Rigidbody rb;
    PlayerInputSystem playerInput;
    GaugeSystem gaugeManager;

    [SerializeField] public int gaugeCost;

    [SerializeField] private GameObject fireBallPref;
    [SerializeField] private Transform fireBallSpawn;
    [SerializeField] private Vector3 mouseWorldPosition;
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();

    private void Awake()
    {
        gaugeManager = GetComponent<GaugeSystem>();
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInputSystem>();
        playerInput.OnFireBall += FireProjectile;
    }
    private void FireProjectile()
    {
        if (playerInput.isInputingAim && gaugeCost <= gaugeManager.currentGauge)
        {
            gaugeManager.LoseGauge(gaugeCost);
            FindHitTarget();
            InstantiateProjectile();
        }
    }
    private void InstantiateProjectile()
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
