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

    //Animation constants
    const int smokeFreq = 20;

    protected override void Awake()
    {
        GameplayManager.SetPlayer(this); //Set a static reference to the player

        base.Awake();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if(currentState == STATE.TALKING) //If talking
        {
            return;
        }

        //********** Handle the controls and move or do an action **********
        //Movement
        controllerMovement = ControlManager.GetMovement();

        HandleMovement();

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

        effectDict.MakeEffect("smoke_landing", transform.position + Vector3.down * raycastOffset);
        TransitionAction(ACTION.IDLE, 1); //Transition to being in the air as a state

        base.OnLanding();
    }

    /// <summary> Triggered by NPCs when they talk to the player (or vice versa) </summary>
    public void OnTalk()
    {
        TransitionState(STATE.TALKING);
    }

    public void EndTalk()
    {
        TransitionState(STATE.IDLE, 4);
    }
    #endregion

    #region Misc Methods
    void HandleMovement()
    {

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
            TransitionAction(ACTION.AERIAL, 1); //Transition to being in the air as a state

            //If we jumped, update the animation
            switch (direction)
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
            forwardVector = hSpeed;
            forwardVector.Normalize(); //Normalize the forward vector
            rightVector = Quaternion.Euler(0, -90, 0) * forwardVector;

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
                if (controllerMovement.y > 0) //Walking away from camera
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

            if (isGrounded && stateTimer % smokeFreq == 0)
                effectDict.MakeEffect("walk_smoke", transform.position + Vector3.down * raycastOffset - 0.06f*forwardVector);
        }
        else if (currentState == STATE.MOVING) //If no inputs and considered moving, switch to IDLE
            TransitionState(STATE.IDLE);
    }

    #endregion
}
