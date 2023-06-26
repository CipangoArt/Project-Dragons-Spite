using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InGameDebugSystem : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;

    [SerializeField] private TextMeshProUGUI text_Speed;

    void Update()
    {
        text_Speed.text = "Speed: " + (int)rb.velocity.magnitude + "km/h";
    }
}
