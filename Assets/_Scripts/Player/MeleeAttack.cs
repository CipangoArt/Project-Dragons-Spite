using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MeleeAttack : MonoBehaviour
{
    Animator anim;
    PlayerInput playerInput;

    [SerializeField] private GameObject meleeAttackAOE;
    [SerializeField] private GameObject comboAttackBarStuff;
    [SerializeField] private Image comboAttackBar;
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
        if (animCount < attackAnimations.Length && isTimeIn)
        {
            if (animCount == attackAnimations.Length - 1) comboAttackBarStuff.SetActive(false);
            else comboAttackBarStuff.SetActive(true);
            //Restart Existing Coroutine
            if (timeToComboCoroutine != null)
            {
                StopCoroutine(timeToComboCoroutine);
                isTimeIn = true;
            }

            //Start Coroutine
            timeToComboCoroutine = StartCoroutine(TimeToCombo(timeOut, timeIn));

            anim.Play(attackAnimations[animCount]);
            animCount++;
        }
        else
        {
            if (timeToComboCoroutine != null)
            {
                StopCoroutine(timeToComboCoroutine);
                isTimeIn = true;
            }

            comboAttackBarStuff.SetActive(false);
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
        comboAttackBarStuff.SetActive(false);
        animCount = 0;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Structure"))
        {
            other.GetComponent<HealthManager>().TakeDamage(2);
            meleeAttackAOE.SetActive(false);
        }
    }
    private void OnHitAnimationEvent()
    {
        meleeAttackAOE.SetActive(true);
    }
    private void OnFinishAnimationEvent()
    {
        meleeAttackAOE.SetActive(false);
    }
}
