using System.Collections;
using UnityEngine;

public class JumpSystem : MonoBehaviour
{
    PlayerInputSystem playerInput;
    PlayerMovementStateSystem playerController;
    Rigidbody rb;

    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float pounceForce;
    bool isAllowedToGlide;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInputSystem>();
        playerController = GetComponent<PlayerMovementStateSystem>();
        rb = GetComponent<Rigidbody>();

        playerInput.OnJump += DoJump;
        playerInput.OnGlide += DoGlide;
        playerInput.OnAirborne += DoAirborne;
    }
    bool canJump = true;
    private void DoJump()
    {
        if (playerController._state == PlayerMovementStateSystem.State.Airborne)
        {
            isAllowedToGlide = true;
            return;
        }
        if (canJump)
        {
            playerController.animBody.SetTrigger("Jump");
            canJump = false;
            StartCoroutine(CanJump());
        }
    }
    private IEnumerator CanJump()
    {
        yield return new WaitForSeconds(2);
        canJump = true;

    }
    public void DoJumpOnAnimEvent()
    {
        //FromGroundToAirborne
        if (playerController._state == PlayerMovementStateSystem.State.Grounded)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -transform.up, out hit, _layerMask))
            {
                isAllowedToGlide = false;
                playerController.GoAirborne();
                Vector3 ponceDir = (transform.forward + transform.up) / 2;
                rb.AddForce(ponceDir * pounceForce);
            }
        }
    }
    private void DoAirborne()
    {
        if (playerController._state == PlayerMovementStateSystem.State.Gliding)
        {
            playerController.GoAirborne();
        }
    }
    private void DoGlide()
    {
        if (isAllowedToGlide)
        {
            playerController.GoGliding();
        }
    }
}
