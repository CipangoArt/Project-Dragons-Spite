using UnityEngine;
using UnityEngine.UI;

public class MeleeAttackSystem : MonoBehaviour
{
    [SerializeField] Animator animBody;
    [SerializeField] Animator animWings;
    PlayerInputSystem playerInput;

    [Header("AOE")]
    [SerializeField] private float fowardDistance;
    [SerializeField] private float areaOfEffect;

    [SerializeField] private GameObject comboAttackFiller;
    [SerializeField] private Image comboAttackBar;

    [Header("Combo")]
    [SerializeField] private bool canHit = false;
    [SerializeField] private int currentAttackNum;
    [SerializeField] private int maxAttackNum = 3;
    private Coroutine timeToComboCoroutine;

    private void Awake()
    {
        currentAttackNum = 0;
        playerInput = GetComponent<PlayerInputSystem>();
        playerInput.OnMeleeAttack += DoMeleeAttack;
    }
    private void DoMeleeAttack()
    {
        if (playerInput.isInputingAim)
            return;
        if (currentAttackNum == maxAttackNum)
            currentAttackNum = 0;
        if (currentAttackNum == 0)
        {
            ContinueCombo();
        }
        if (canHit)
        {
            ContinueCombo();
        }
        else
        {
            ResetCombo();
        }
        canHit = false;
    }
    public void OnHitAnimEvent(int damage)
    {
        canHit = true;
        Collider[] captedColliders = Physics.OverlapSphere(transform.position + transform.forward * fowardDistance, areaOfEffect);
        for (int i = 0; i < captedColliders.Length; i++)
        {
            if (captedColliders[i].CompareTag("Destructable"))
            {
                captedColliders[i].GetComponent<HealthManager>().TakeDamage(damage);
            }
        }
    }
    public void ResetCombo()
    {
        currentAttackNum = 0;
        animBody.SetTrigger("ResetCombo");
    }
    private void ContinueCombo()
    {
        currentAttackNum++;
        animBody.SetTrigger("ContinueCombo");
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * fowardDistance, areaOfEffect);
    }
}
