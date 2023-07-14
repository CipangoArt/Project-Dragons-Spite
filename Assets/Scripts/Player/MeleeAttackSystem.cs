using UnityEngine;
using UnityEngine.UI;

public class MeleeAttackSystem : MonoBehaviour
{
    [SerializeField] Animator animBody;
    [SerializeField] Animator animWings;
    PlayerInputSystem playerInput;

    [SerializeField] private GameObject prefHitIndicator;

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
        {
            prefHitIndicator.SetActive(false);
            currentAttackNum = 0;
        }
        if (currentAttackNum == 0 || canHit)
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
        if (currentAttackNum != 3)
        {
            prefHitIndicator.SetActive(true);
        }
        canHit = true;
        Collider[] captedColliders = Physics.OverlapSphere(transform.position + transform.forward * fowardDistance, areaOfEffect);
        for (int i = 0; i < captedColliders.Length; i++)
        {
            if (captedColliders[i].CompareTag("Destructable")||  captedColliders[i].CompareTag("Villager") || captedColliders[i].CompareTag("Balista"))
            {
            captedColliders[i].GetComponent<HealthManager>().TakeDamage(damage);
        }
    }
    }
    public void ResetCombo()
    {
        prefHitIndicator.SetActive(false);
        currentAttackNum = 0;
        animBody.SetTrigger("ResetCombo");
    }
    private void ContinueCombo()
    {
        prefHitIndicator.SetActive(false);
        currentAttackNum++;
        animBody.SetTrigger("ContinueCombo");
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * fowardDistance, areaOfEffect);
    }
}
