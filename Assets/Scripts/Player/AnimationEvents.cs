using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    [SerializeField] FireBallSystem fireBallSystem;
    [SerializeField] MeleeAttackSystem meleeAttackSystem;
    [SerializeField] JumpSystem jumpSystem;

    public void OnFireBall()
    {
        fireBallSystem.FireOnAnimationEvent();
    }
    public void OnJump()
    {
        jumpSystem.DoJumpOnAnimEvent();
    }
    public void OnHit(int damage)
    {
        meleeAttackSystem.OnHitAnimEvent(damage);
    }
    public void OnAnimationEnd()
    {
        meleeAttackSystem.ResetCombo();
    }
    public void OnPlaySound(string sound)
    {
        AudioManager.instance.PlaySound(sound);
    }
}
