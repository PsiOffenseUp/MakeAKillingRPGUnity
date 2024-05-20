using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Just bilboards the sprite to look at the screen every frame
public class BilboardSprite : MonoBehaviour
{
    Vector3 lookDirection;
    Quaternion moveRotation;
    const float rollSpeed = 0.006f;

    // Update is called once per frame
    void Update()
    {
        //Smile for the camera
        lookDirection = transform.position - GameplayManager.mainCamera.transform.position;
        lookDirection.Normalize();
        moveRotation = Quaternion.FromToRotation(Vector3.forward, lookDirection);
        transform.rotation = Quaternion.Lerp(transform.rotation, moveRotation, rollSpeed * Time.time);
    }
}
