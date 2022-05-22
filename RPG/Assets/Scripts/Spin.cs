using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    [SerializeField] float spinSpeed = 1.2f;

    void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed);
    }
}
