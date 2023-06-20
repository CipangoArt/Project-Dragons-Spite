using UnityEngine;

public class Pounce : MonoBehaviour
{
    PlayerInput playerInput;
    PlayerController playerController;
    Rigidbody rb;

    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float pounceForce;
    bool isAllowedToGlide;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>();

        playerInput.OnPounce += DoPounce;
        playerInput.OnGlide += DoGlide;
        playerInput.OnAirborne += DoAirborne;
    }
    private void DoPounce()
    {
        //FromGroundToAirborne
        if (playerController._state == PlayerController.State.Grounded)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -transform.up, out hit, _layerMask))
            {
                playerController.animWings.SetTrigger("Jump");
                playerController.animBody.SetTrigger("Jump");
                isAllowedToGlide = false;
                playerController.GoAirborne();
                Vector3 ponceDir = (transform.forward + transform.up) / 2;
                rb.AddForce(ponceDir * pounceForce);
            }
        }
        else if (playerController._state == PlayerController.State.Airborne)
        {
            isAllowedToGlide = true;
        }
    }
    private void DoAirborne()
    {
        if (playerController._state == PlayerController.State.Gliding)
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
