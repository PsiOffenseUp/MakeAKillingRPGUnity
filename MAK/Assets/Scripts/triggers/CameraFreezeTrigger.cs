using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//For triggers that lock the camera in place. Specifically used for LookingCameras
public class CameraFreezeTrigger : MonoBehaviour
{
    LookingCamera lCamera;

    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (lCamera == null)
            GetLCamera();

        lCamera.SetMode(LookingCamera.MODE.STATIC);
    }

    private void OnTriggerExit(Collider other)
    {
        if (lCamera == null)
            GetLCamera();

        lCamera.SetMode(LookingCamera.MODE.FOLLOW);
    }

    void GetLCamera()
    {
        lCamera = GameplayManager.mainCamera.GetComponent<LookingCamera>();
    }
}
