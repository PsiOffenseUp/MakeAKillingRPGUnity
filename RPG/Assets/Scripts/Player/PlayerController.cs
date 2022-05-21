using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : TimeAffectedObject
{
    [SerializeField] Rigidbody rBody;
    [SerializeField] float moveSpeed = 5.5f;
    [SerializeField] float jumpSpeed = 4.0f;
    Vector3 hSpeed, speed;
    Vector2 controllerMovement;
    public static Vector3 cameraForwardProjected; //Used for sprite facing calculations

    float horizontalDot; //Dot product between the non-vertical components of moving and the camera
    const float horizontalMovePadding = 0.5f; //Used to make animation smoother

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

        speed = hSpeed; //Set the horizontal component of the speed

        //TODO Handle vertical components
        if (isGrounded && ControlManager.JumpPressed())
        {
            speed.y = jumpSpeed;
            //If we jumped, update the animation
            switch(direction)
            {
                case Direction.LEFT:
                    animationHandler.ChangeAnimation("air_l");
                    break;
                case Direction.RIGHT:
                    animationHandler.ChangeAnimation("air_r");
                    break;
                case Direction.BACK:
                    animationHandler.ChangeAnimation("air_u");
                    break;
                case Direction.FORWARD:
                    animationHandler.ChangeAnimation("air_d");
                    break;
            }
        }
        else
            speed.y = rBody.velocity.y;

        rBody.velocity = speed;

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
                        animationHandler.ChangeAnimation(isGrounded ? "walk_l" : "air_l");
                    }
                }
                else //Moving right
                {
                    if (direction != Direction.RIGHT) //If not already considered facing right, update
                    {
                        direction = Direction.RIGHT;
                        animationHandler.ChangeAnimation(isGrounded ? "walk_r" : "air_r");
                    }
                }
            }
            else //Forward/backward is bigger
            {
                if(controllerMovement.y > 0) //Walking away from camera
                {
                    if (direction != Direction.BACK) //If not already considered facing back, update
                    {
                        direction = Direction.BACK;
                        animationHandler.ChangeAnimation(isGrounded ? "walk_u" : "air_u");
                    }
                }
                else
                {
                    if (direction != Direction.FORWARD) //If not already considered facing forward, update
                    {
                        direction = Direction.FORWARD;
                        animationHandler.ChangeAnimation(isGrounded ? "walk_d" : "air_d");
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

    #region Other Events
    protected override void OnLanding()
    {
        //Update the animation on landing
        switch (direction)
        {
            case Direction.LEFT:
                animationHandler.ChangeAnimation((controllerMovement.x != 0.0f || controllerMovement.y != 0.0f) ? "walk_l" : "idle_l");
                break;
            case Direction.RIGHT:
                animationHandler.ChangeAnimation((controllerMovement.x != 0.0f || controllerMovement.y != 0.0f) ? "walk_r" : "idle_r");
                break;
            case Direction.BACK:
                animationHandler.ChangeAnimation((controllerMovement.x != 0.0f || controllerMovement.y != 0.0f) ? "walk_u" : "idle_u");
                break;
            case Direction.FORWARD:
                animationHandler.ChangeAnimation((controllerMovement.x != 0.0f || controllerMovement.y != 0.0f) ? "walk_d" : "idle_d");
                break;
        }

        effectDict.MakeEffect("smoke_landing", transform.position + Vector3.down * raycastOffset, transform.rotation);

        base.OnLanding();
    }
    #endregion
}
