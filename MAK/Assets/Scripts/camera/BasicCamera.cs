using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCamera : MonoBehaviour
{
    [SerializeField] protected GameObject focus = null;
    [SerializeField] protected float panSpeed = 1.0f;
    [SerializeField] protected float zoomSpeed = 1.0f;
    protected Vector3 targetPosition; //Where we want the camera to move to

    protected virtual void Awake()
    {
        //GameplayManager.SetMainCamera(GetComponent<Camera>()); //Set this to be the main camera
    }

    protected virtual void Start()
    {
        targetPosition = transform.position;

        //If there is no focus by the first frame, set this camera to focus on the player
        if (focus == null)
            focus = GameplayManager.player.gameObject;
    }

    protected virtual void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, panSpeed);
        //Add code for zoom maybe
    }

    public void SetFocus(GameObject focus)
    {
        this.focus = focus;
    }

}
