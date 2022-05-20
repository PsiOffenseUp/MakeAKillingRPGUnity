using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Just bilboards the sprite to look at the screen every frame
public class BilboardSprite : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        //Smile for the camera (DEBUG Maybe replace this with a shader at some point?)
        transform.rotation = GameplayManager.mainCamera.transform.rotation;
    }
}
