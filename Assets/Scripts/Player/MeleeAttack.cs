using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MeleeAttack : MonoBehaviour
{
    Animator anim;
    PlayerInput playerInput;

    [Header("Attack")]
    [SerializeField] private int damage;

    [Header("AOE")]
    [SerializeField] private float fowardDistance;
    [SerializeField] private float areaOfEffect;

    [SerializeField] private GameObject comboAttackFiller;
    [SerializeField] private Image comboAttackBar;

    [Header("Combo")]
    [SerializeField] private float timeIn = .6f;
    [SerializeField] private float timeOut = 1f;
    [SerializeField] private float time = 1f;
    [SerializeField] private bool isTimeIn = true;
    [SerializeField] private string[] attackAnimations;
    [SerializeField] private int animCount;
    private Coroutine timeToComboCoroutine;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        time = timeIn + timeOut;
        playerInput.OnMeleeAttack += DoMeleeAttack;
    }
    private void DoMeleeAttack()
    {
        if (playerInput.isAiming) return;
        if (animCount < attackAnimations.Length && isTimeIn)
        {
            if (animCount == attackAnimations.Length - 1) comboAttackFiller.SetActive(false);
            else comboAttackFiller.SetActive(true);
            //Restart Existing Coroutine
            if (timeToComboCoroutine != null)
            {
                StopCoroutine(timeToComboCoroutine);
                isTimeIn = true;
            }

            //Start Coroutine
            timeToComboCoroutine = StartCoroutine(TimeToCombo(timeOut, timeIn));

            anim.SetTrigger("OnAttack");
            animCount++;
        }
        else
        {
            if (timeToComboCoroutine != null)
            {
                StopCoroutine(timeToComboCoroutine);
                isTimeIn = true;
            }

            comboAttackFiller.SetActive(false);
            StopCoroutine(TimeToCombo(timeOut, timeIn));
            animCount = 0;
        }
    }
    IEnumerator TimeToCombo(float timeOut, float timeIn)
    {
        comboAttackBar.color = Color.red;
        isTimeIn = false;
        float elapsedTime = 0f;

        while (elapsedTime < timeIn)
        {
            elapsedTime += Time.deltaTime;
            comboAttackBar.fillAmount = elapsedTime / time;
            yield return null;
        }

        isTimeIn = true;
        elapsedTime = 0f;

        while (elapsedTime < timeOut)
        {
            elapsedTime += Time.deltaTime;
            comboAttackBar.color = Color.green;
            comboAttackBar.fillAmount = (elapsedTime + timeIn) / time;
            yield return null;
        }
        comboAttackFiller.SetActive(false);
        animCount = 0;
    }
    public void OnHitAnimationEvent()
    {
        Collider[] captedColliders = Physics.OverlapSphere(transform.position + transform.forward * fowardDistance, areaOfEffect);
        for (int i = 0; i < captedColliders.Length; i++)
        {
            if (captedColliders[i].CompareTag("Destructable"))
            {
                captedColliders[i].GetComponent<HealthManager>().TakeDamage(damage);
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * fowardDistance, areaOfEffect);
    }
}
