using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Camera that looks at the focus every single frame </summary>
public class LookingCamera : BasicCamera
{
    public enum MODE { FOLLOW, STATIC } //Modes for working
    [SerializeField] MODE mode = MODE.STATIC;
    [SerializeField] Vector3 offset; //How far to position the camera away from the focus if it is in follow mode
    [SerializeField] Vector3 focusOffset; //Offset relative to the focus to point at
    [SerializeField] protected float rollSpeed = 0.35f; //Speed for rolling while rotating
    Vector3 moveDirection;
    Vector3 lookDirection;
    Vector3 defaultFocusOffset, defaultOffset;
    Quaternion moveRotation;

    #region Unity Method Overrides
    protected override void Awake()
    {
        base.Awake();
        defaultFocusOffset = focusOffset; //Keep track of the default focus offset
        defaultOffset = offset;
    }

    protected override void Start()
    {
        base.Start();

        if (mode == MODE.FOLLOW)
        {
            transform.position = focus.transform.position + offset;
            targetPosition = transform.position;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch(mode)
        {
            case MODE.FOLLOW:
                targetPosition = focus.transform.position + offset;
                moveDirection = targetPosition - transform.position;

                //If the distance to the target position is small enough, move straight there
                if(moveDirection.magnitude < panSpeed * Time.deltaTime)
                {
                    transform.position = targetPosition;
                    break;
                }
                moveDirection.Normalize();
                //transform.position = Vector3.Lerp(transform.position, targetPosition, panSpeed * Time.deltaTime);
                transform.position += panSpeed * Time.deltaTime*moveDirection;
                break;
            case MODE.STATIC:
                break;
        }

        //Turn towards the focus
        lookDirection = focus.transform.position + focusOffset - transform.position;
        lookDirection.Normalize();
        moveRotation = Quaternion.FromToRotation(Vector3.forward, lookDirection);
        transform.rotation = Quaternion.Lerp(transform.rotation, moveRotation, rollSpeed * Time.time);
        //transform.LookAt(focus.transform);
    }
    #endregion

    #region Other Methods
    public void SetMode(MODE new_mode) { mode = new_mode; }
    public void SetPosition(Vector3 pos) { this.transform.position = pos; }
    public void LookAtFocus() { transform.LookAt(focus.transform); } //TODO: Update this to use rotation code
    public void ImmediatelyGoToOffet() { transform.position = focus.transform.position + offset; } 
    public void SetFocusOffset(Vector3 focus_offset) { this.focusOffset = focus_offset; }
    public void SetOffset(Vector3 offset) { this.offset = offset; }
    public void UseDefaultOffset() { this.offset = defaultOffset; }
    public void UseDefaultFocusOffset() { this.focusOffset = defaultFocusOffset; }
    public void UseDefaultOffsets() { this.offset = defaultOffset; this.focusOffset = defaultFocusOffset; }
    #endregion

}
