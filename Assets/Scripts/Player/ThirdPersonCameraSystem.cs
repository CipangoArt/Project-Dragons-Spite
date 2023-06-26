using Cinemachine;
using UnityEngine;

public class ThirdPersonCameraSystem : MonoBehaviour
{
    public static ThirdPersonCameraSystem instance;
    private PlayerMovementStateSystem playerMovement;
    private PlayerInputSystem playerInput;

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

    [Header("FOV")]
    public CinemachineImpulseSource impulseSource;
    [SerializeField] private float globalShakeForce = 1f;

    private void Awake()
    {
        instance ??= this;
        impulseSource = GetComponent<CinemachineImpulseSource>();
        playerMovement = GetComponent<PlayerMovementStateSystem>();
        playerInput = GetComponent<PlayerInputSystem>();
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

            if (playerInput.isInputingAim)
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
        float t = playerMovement.currentSpeed / (playerMovement.defaultTargetSpeed * playerMovement.turboTargetMultiplier);
        float y = Mathf.Lerp(fovNormal, fovMax, t);

        fov = Mathf.Lerp(fov, y, fovSmoothTime * Time.deltaTime);

        _primaryCamera.m_Lens.FieldOfView = fov;
    }
    public void Aim()
    {
        _aimCamera.gameObject.SetActive(playerInput.isInputingAim);
        //if (playerInput.isAiming)
        //{
        //    //Vector3 worldAimTarget = mouseWorldPosition;
        //    //worldAimTarget.y = transform.position.y;
        //    //Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
        //    //transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
        //}
    }
    public void CameraShake(CinemachineImpulseSource impulseSource)
    {
        impulseSource.GenerateImpulseWithForce(globalShakeForce);
    }
}
