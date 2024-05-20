using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtomAnimation : MonoBehaviour
{
    [SerializeField] float radius = 80.0f;

    const float TAU = 6.28301852f;
    //const float HALF_PI = 1.57079632679f;

    private void Start()
    {
        transform.localPosition = radius * Vector3.right;
    }

    protected void MoveAlongCircle(float fraction)
    {
        fraction = 1.25f - fraction;
        transform.localPosition = radius * new Vector3(Mathf.Cos(TAU * fraction), Mathf.Sin(TAU * fraction), 0.0f);
    }
}
