using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    [SerializeField] MeleeAttackSystem meleeAttackSystem;
    [SerializeField] JumpSystem jumpSystem;

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
}
