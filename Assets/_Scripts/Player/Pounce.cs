using UnityEngine;

public class Pounce : MonoBehaviour
{
    PlayerInput playerInput;
    PlayerController playerController;
    Rigidbody rb;

    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float pounceForce;

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
                playerController.GoAirborne();
                Vector3 ponceDir = (transform.forward + hit.normal) / 2;
                rb.AddForce(ponceDir * pounceForce);
            }
        }
        if (playerController._state == PlayerController.State.Airborne && playerInput.isGliding)
        {
            playerController.GoGliding();
        }
    }
    private void DoGlide()
    {
        //From Airborne To Glide
        if (playerController._state == PlayerController.State.Airborne)
        {
            playerController.GoGliding();
        }
    }
    private void DoAirborne()
    {
        //From Airborne To Glide
        if (playerController._state == PlayerController.State.Gliding)
        {
            playerController.GoAirborne();
        }
    }
}
