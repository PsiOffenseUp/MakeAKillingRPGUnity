using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sine : MonoBehaviour
{
	//Attach this script to make the object move in a sinusoidal pattern
	[SerializeField] float amplitude, speed, phaseShift;
	Vector3 offset;

    // Start is called before the first frame update
    void Awake()
    {
		offset = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
		transform.position = new Vector3(transform.position.x, offset.y + amplitude * (float)System.Math.Sin(speed*GameplayManager.frameTimer + phaseShift), transform.position.z);
    }
}
