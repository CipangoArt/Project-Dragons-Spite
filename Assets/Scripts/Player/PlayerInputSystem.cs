using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : MonoBehaviour
{
    public PlayerInputActions playerInputActions;

    //Triggers
    public event Action OnJump;
    public event Action OnMeleeAttack;
    public event Action OnFireBall;
    public event Action OnGlide;
    public event Action OnAirborne;
    public event Action OnTurbo;
    public event Action OnTurboRelease;
    //Holders
    public bool isInputingAim;
    public bool isInputingTurbo;
    //Vectors
    public Vector3 direction;
    public Vector2 look;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Gameplay.Enable();
    }
    private void OnEnable()
    {
        playerInputActions.Gameplay.Pounce.performed += Jump;

        playerInputActions.Gameplay.Glide.performed += GlideInput;
        playerInputActions.Gameplay.Glide.canceled += GlideInput;

        playerInputActions.Gameplay.Turbo.performed += TurboInput;
        playerInputActions.Gameplay.Turbo.canceled += TurboInput;
        playerInputActions.Gameplay.Turbo.started += TurboInput;

        playerInputActions.Gameplay.MeleeAttack.performed += MeleeAttackInput;

        playerInputActions.Gameplay.FireBall.performed += FireBallInput;

        playerInputActions.Gameplay.Aim.performed += AimInput;
        playerInputActions.Gameplay.Aim.canceled += AimInput;
        playerInputActions.Gameplay.Aim.started += AimInput;

        playerInputActions.Gameplay.Move.performed += MoveInput;
        playerInputActions.Gameplay.Move.canceled += MoveInput;
        playerInputActions.Gameplay.Move.started += MoveInput;

        playerInputActions.Gameplay.Look.performed += LookInput;
        playerInputActions.Gameplay.Look.canceled += LookInput;
    }
    private void OnDisable()
    {
        playerInputActions.Gameplay.Pounce.performed -= Jump;

        playerInputActions.Gameplay.Glide.performed += GlideInput;
        playerInputActions.Gameplay.Glide.canceled += GlideInput;

        playerInputActions.Gameplay.Turbo.performed -= TurboInput;
        playerInputActions.Gameplay.Turbo.canceled -= TurboInput;

        playerInputActions.Gameplay.MeleeAttack.performed -= MeleeAttackInput;

        playerInputActions.Gameplay.FireBall.performed -= FireBallInput;

        playerInputActions.Gameplay.Aim.performed -= AimInput;
        playerInputActions.Gameplay.Aim.canceled -= AimInput;

        playerInputActions.Gameplay.Move.performed -= MoveInput;
        playerInputActions.Gameplay.Move.canceled -= MoveInput;

        playerInputActions.Gameplay.Look.performed -= LookInput;
        playerInputActions.Gameplay.Look.canceled -= LookInput;
    }
    public void Jump(InputAction.CallbackContext context)
    {
        OnJump?.Invoke();
    }
    public void GlideInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnGlide?.Invoke();
        }
        if (context.canceled)
        {
            OnAirborne?.Invoke();
        }
    }
    public void TurboInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnTurbo?.Invoke();
            isInputingTurbo = true;
        }
        if (context.canceled)
        {
            OnTurboRelease?.Invoke();
            isInputingTurbo = false;
        }
    }
    public void MeleeAttackInput(InputAction.CallbackContext context)
    {
        OnMeleeAttack?.Invoke();
    }
    public void FireBallInput(InputAction.CallbackContext context)
    {
        OnFireBall?.Invoke();
    }
    public void AimInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isInputingAim = true;
        }
        if (context.canceled)
        {
            isInputingAim = false;
        }
    }
    public void MoveInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Vector2 moveInput = context.ReadValue<Vector2>();
            direction = new Vector3(moveInput.x, 0f, moveInput.y);
        }
        if (context.canceled || context.performed)
        {
            Vector2 moveInput = context.ReadValue<Vector2>();
            direction = new Vector3(moveInput.x, 0f, moveInput.y);
        }
    }
    public void LookInput(InputAction.CallbackContext context)
    {
        look = context.ReadValue<Vector2>();
    }
}