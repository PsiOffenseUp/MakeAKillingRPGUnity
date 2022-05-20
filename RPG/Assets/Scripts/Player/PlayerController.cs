using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : TimeAffectedObject
{
    //Used to figure out which direction the player is facing for animations
    public enum Direction { LEFT, RIGHT, BACK, FORWARD}
    Direction direction = Direction.RIGHT;

    [SerializeField] Rigidbody rBody;
    [SerializeField] float moveSpeed = 0.05f;
    Vector3 hSpeed, speed;
    Vector2 controllerMovement;
    public static Vector3 cameraForwardProjected; //Used for sprite facing calculations
    float horizontalDot; //Dot product between the non-vertical components of moving and the camera
    const float horizontalMovePadding = 0.3f; //Used to make animation smoother
    protected override void Awake()
    {
        GameplayManager.SetPlayer(this.gameObject); //Set a static reference to the player

        base.Awake();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        //********** Handle the controls and move or do an action **********
        //Movement
        controllerMovement = ControlManager.GetMovement();
        hSpeed = 
            GameplayManager.mainCamera.transform.forward * controllerMovement.y +
            GameplayManager.mainCamera.transform.right * controllerMovement.x;
        hSpeed.y = 0; //Don't do any vertical component on the horizontal speed
        hSpeed.Normalize();
        hSpeed *= moveSpeed;

        /*
        cameraForwardProjected = GameplayManager.mainCamera.transform.forward;
        cameraForwardProjected.y = 0;
        cameraForwardProjected.Normalize();
        horizontalDot = Vector3.Dot(hSpeed, cameraForwardProjected);
        */

        speed = hSpeed; //For now, don't do a vertical component DEBUG

        //TODO Handle vertical components

        rBody.velocity = speed; //Set the speed to what we calculated

        //Figure out the sprite to display
        if (controllerMovement.x != 0.0f || controllerMovement.y != 0.0f) //If the player is inputting something, update the direction
        {
            if (Mathf.Abs(controllerMovement.x) > Mathf.Abs(controllerMovement.y) - horizontalMovePadding) //Figure out whether horizontal or forward magnitude is bigger
            {
                if (controllerMovement.x < 0) //Moving left
                {
                    if (direction != Direction.LEFT) //If not already considered facing left, update
                    {
                        direction = Direction.LEFT;
                        animationHandler.ChangeAnimation("walk_l");
                    }
                }
                else //Moving right
                {
                    if (direction != Direction.RIGHT) //If not already considered facing left, update
                    {
                        direction = Direction.RIGHT;
                        animationHandler.ChangeAnimation("walk_r");
                    }
                }
            }
            else //Forward/backward is bigger
            {
                if(controllerMovement.y > 0) //Walking away from camera
                {
                    if (direction != Direction.BACK) //If not already considered facing left, update
                    {
                        direction = Direction.BACK;
                        animationHandler.ChangeAnimation("walk_u");
                    }
                }
                else
                {
                    if (direction != Direction.FORWARD) //If not already considered facing left, update
                    {
                        direction = Direction.FORWARD;
                        animationHandler.ChangeAnimation("walk_d");
                    }
                }
            }

            if (currentState == STATE.IDLE)
                TransitionState(STATE.MOVING);
        }
        else if (currentState == STATE.MOVING) //If no inputs and considered moving, switch to IDLE
            TransitionState(STATE.IDLE);
    }

    #region State and Action related methods

    protected override void OnStateTransition()
    {
        base.OnStateTransition();

        switch(currentState)
        {
            case STATE.IDLE: //Currently idle
                if(previousState == STATE.MOVING) //If we just stopped moving, switch the animation
                {
                    switch(direction)
                    {
                        case Direction.LEFT:
                            animationHandler.ChangeAnimation("idle_l");
                            break;
                        case Direction.RIGHT:
                            animationHandler.ChangeAnimation("idle_r");
                            break;
                        case Direction.FORWARD:
                            animationHandler.ChangeAnimation("idle_d");
                            break;
                        case Direction.BACK:
                            animationHandler.ChangeAnimation("idle_u");
                            break;
                    }
                }
                break;
            case STATE.MOVING: //Currently moving
                switch (direction)
                {
                    case Direction.LEFT:
                        animationHandler.ChangeAnimation("walk_l");
                        break;
                    case Direction.RIGHT:
                        animationHandler.ChangeAnimation("walk_r");
                        break;
                    case Direction.FORWARD:
                        animationHandler.ChangeAnimation("walk_d");
                        break;
                    case Direction.BACK:
                        animationHandler.ChangeAnimation("walk_u");
                        break;
                }
                break;
        }
    }

    protected override void OnActionTransition()
    {
        base.OnActionTransition();
    }
    #endregion

    #region Debugging
    public Direction GetDirection() { return direction; }
    public STATE GetState() { return currentState; }
    public ACTION GetAction() { return currentAction; }
    #endregion
}
