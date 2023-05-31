using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private PlayerController playerMovement;
    private PlayerInput playerInput;

    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera _primaryCamera;
    [SerializeField] private CinemachineVirtualCamera _aimCamera;
    [SerializeField] private GameObject _cameraTarget;
    private const float _threshold = 0.01f;
    public bool LockCameraPosition = false;
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
            return false;
#endif
        }
    }
    public float TopClamp = 70.0f;
    public float BottomClamp = -30.0f;
    public float CameraAngleOverride = 0.0f;
    [SerializeField] private float camNormalSensivityX;
    [SerializeField] private float camNormalSensivityY;
    [SerializeField] private float camAimSensivityX;
    [SerializeField] private float camAimSensivityY;

    [Header("FOV")]
    [SerializeField] private float fov = 50f;
    [SerializeField] private float fovNormal = 50f;
    [SerializeField] private float fovMax = 75f;
    [SerializeField] private float fovSmoothTime = 1f;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerController>();
        playerInput = GetComponent<PlayerInput>();
    }
    private void Update()
    {
        Aim();
        CameraRotation();
    }
    private void CameraRotation()
    {
        if (playerInput.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            if (playerInput.isAiming)
            {
                _cinemachineTargetYaw += playerInput.look.x * deltaTimeMultiplier * camAimSensivityX;
                _cinemachineTargetPitch += playerInput.look.y * deltaTimeMultiplier * camAimSensivityY;
            }
            else
            {
                _cinemachineTargetYaw += playerInput.look.x * deltaTimeMultiplier * camNormalSensivityX;
                _cinemachineTargetPitch += playerInput.look.y * deltaTimeMultiplier * camNormalSensivityY;
            }
        }
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        _cameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
    public void Fov()
    {
        float t = playerMovement.currentSpeed / (playerMovement.targetSpeed * playerMovement.turboTargetMultiplier);
        float y = Mathf.Lerp(fovNormal, fovMax, t);

        fov = Mathf.Lerp(fov, y, fovSmoothTime * Time.deltaTime);

        _primaryCamera.m_Lens.FieldOfView = fov;
    }
    public void Aim()
    {
        _aimCamera.gameObject.SetActive(playerInput.isAiming);
        //if (playerInput.isAiming)
        //{
        //    //Vector3 worldAimTarget = mouseWorldPosition;
        //    //worldAimTarget.y = transform.position.y;
        //    //Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
        //    //transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
        //}
    }
}
