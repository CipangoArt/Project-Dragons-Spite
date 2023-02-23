using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    float horizontalInput;
    float verticalInput;
    Vector3 horizontalInputVector;
    Vector3 verticalInputVector;
    [SerializeField] float speed = 5f;
    [SerializeField] float rotationSpeed = 5f;
    private Vector3 wantedPos;
    [SerializeField] Vector3 centreOfMass;
    [SerializeField] Vector3 rotDir;

    private void Update()
    {

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        horizontalInputVector = new Vector3(horizontalInput, 0, 0);
        verticalInputVector = new Vector3(0, 0, verticalInput);
        rb.centerOfMass = centreOfMass;
        

    }
    private void Start()
    {
       
        
    }
    private void FixedUpdate()
    {
        //wantedPos = transform.position + (new Vector3(transform.forward.x,0,transform.forward.z)* verticalInput * speed * Time.deltaTime );
        //rb.MovePosition(wantedPos);
        rb.velocity = new Vector3(transform.forward.x, 0, transform.forward.z) * verticalInput * speed * Time.deltaTime + new Vector3(0, rb.velocity.y, 0);
        // rb.velocity = new Vector3(verticalInput * speed * Time.deltaTime, rb.velocity.y, -horizontalInput * speed * Time.deltaTime) ;
        Quaternion wantedRot = transform.rotation * Quaternion.Euler(transform.up * (horizontalInput * rotationSpeed * Time.deltaTime));

        //transform.Rotate(wantedRot.eulerAngles, Space.World);
        rb.AddTorque(rotDir * horizontalInput * rotationSpeed * Time.deltaTime, ForceMode.Force);
        //rb.MoveRotation(wantedRot);
        //rb.AddTorque(Vector3.up * horizontalInput* rotationSpeed * Time.deltaTime, ForceMode.Acceleration);
       /* var q = Quaternion.AngleAxis(30, Vector3.up);
        float angle;
        Vector3 axis;
        q.ToAngleAxis(out angle, out axis);
        Debug.Log(axis * angle * horizontalInput * rotationSpeed * Time.deltaTime * Mathf.Deg2Rad);
        rb.angularVelocity = axis * angle * horizontalInput * rotationSpeed * Time.deltaTime * Mathf.Deg2Rad;*/

    }





}
