using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowRotation : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;

    private void FixedUpdate()
    {
        transform.right = Vector3.Slerp(transform.right, rb.velocity.normalized, Time.fixedDeltaTime);
    }
}
