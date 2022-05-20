using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Camera that looks at the focus every single frame </summary>
public class LookingCamera : BasicCamera
{

    public enum MODE { FOLLOW, STATIC } //Modes for working
    [SerializeField] MODE mode = MODE.STATIC;
    [SerializeField] Vector3 offset; //How far to position the camera away from the focus if it is in follow mode

    #region Unity Method Overrides
    protected override void Awake()
    {
        base.Awake();
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
    protected override void Update()
    {
        switch(mode)
        {
            case MODE.FOLLOW:
                targetPosition = focus.transform.position + offset;
                transform.position = Vector3.Lerp(transform.position, targetPosition, panSpeed * Time.deltaTime);
                break;
            case MODE.STATIC:
                break;
        }

        //Face the focus
        transform.LookAt(focus.transform);
    }
    #endregion

    #region Other Methods
    public void SetMode(MODE new_mode) { mode = new_mode; }

    #endregion

}
