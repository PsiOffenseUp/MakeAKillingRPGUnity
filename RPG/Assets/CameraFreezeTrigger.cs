using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//For triggers that lock the camera in place. Specifically used for LookingCameras
public class CameraFreezeTrigger : MonoBehaviour
{
    LookingCamera camera;

    // Start is called before the first frame update
    void Start()
    {
        camera = GameplayManager.mainCamera.GetComponent<LookingCamera>();
    }

    private void OnTriggerEnter(Collider other)
    {
        camera.SetMode(LookingCamera.MODE.STATIC);
    }

    private void OnTriggerExit(Collider other)
    {
        camera.SetMode(LookingCamera.MODE.FOLLOW);
    }
}
