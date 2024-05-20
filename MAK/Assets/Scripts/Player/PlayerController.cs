using BoogalooGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : TimeAffectedObject
{
    [SerializeField] Rigidbody rBody;
    [SerializeField] float moveSpeed = 5.5f;
    [SerializeField] float jumpSpeed = 4.0f;
    [SerializeField] GameObject heldObjectAnchor; //Place to anchor heldObjects to
    [SerializeField] GameObject dropShadow;

    enum ACTION { IDLE, AERIAL, JUMPING } //TODO: Move this to specific uses
    StateMachine<ACTION> action;

    PickUpObject heldObject;
    Vector3 hSpeed;
    Vector2 controllerMovement;
    public static Vector3 cameraForwardProjected; //Used for sprite facing calculations

    float horizontalDot; //Dot product between the non-vertical components of moving and the camera
    const float horizontalMovePadding = 0.5f; //Used to make animation smoother

    //Animation constants
    const int smokeFreq = 20;

    //Item related variables
    public float collectionRadius { get; private set; } = 5;

    const float JUMP_SQUAT_DURATION = 3; //Duration in frames of jump squat

    protected override void Awake()
    {
        base.Awake();

        GameplayManager.SetPlayer(this); //Set a static reference to the player
        DontDestroyOnLoad(this);
        heldObject = null;

        //Initialize action state machine
        action = new StateMachine<ACTION>(ACTION.IDLE);
        state.OnTransition += OnStateTransition;
        action.OnTransition += OnActionTransition;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if(state.current == STATE.TALKING || state.current == STATE.LOCKED) //If talking or locked, don't move
        {
            return;
        }

        if (IsHoldingObject()) //If holding an object, move the object towards its anchor
        {
            heldObject.transform.position = heldObjectAnchor.transform.position;

            if (ControlManager.RightTriggerPressed()) //If holding on to an object and trigger is pressed, put it down
                OnPutDown();
        }

        //********** Handle the controls and move or do an action **********
        //Movement
        controllerMovement = ControlManager.GetMovement();

        action.Update();

        HandleMovement();
        HandleActions();

    }

    #region State and Action related methods
    protected void OnStateTransition(STATE previous, STATE current)
    {

        switch(current)
        {
            case STATE.IDLE: //Currently idle
                if(previous== STATE.MOVING) //If we just stopped moving, switch the animation
                {
                    if (isGrounded) //Only need to change animation if grounded
                    {
                        switch (direction)
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
                }
                break;
            case STATE.MOVING: //Currently moving
                switch (direction)
                {
                    case Direction.LEFT:
                        animationHandler.ChangeAnimation(isGrounded ? "walk_l" : (speed.y >= 0.0f ? "air_l" : "fall_l"));
                        break;
                    case Direction.RIGHT:
                        animationHandler.ChangeAnimation(isGrounded ? "walk_r" : (speed.y >= 0.0f ? "air_r" : "fall_r"));
                        break;
                    case Direction.FORWARD:
                        animationHandler.ChangeAnimation(isGrounded ? "walk_d" : (speed.y >= 0.0f ? "air_d" : "fall_d"));
                        break;
                    case Direction.BACK:
                        animationHandler.ChangeAnimation(isGrounded ? "walk_u" : (speed.y >= 0.0f ? "air_u" : "fall_u"));
                        break;
                }
                break;
            case STATE.TALKING:
                break;
        }
    }

    void OnActionTransition(ACTION previous, ACTION current)
    {
        switch(current)
        {
            case ACTION.JUMPING: //Jumping
                //If we jumped, update the animation
                switch (direction)
                {
                    case Direction.LEFT:
                        animationHandler.ChangeAnimation("jump_l");
                        break;
                    case Direction.RIGHT:
                        animationHandler.ChangeAnimation("jump_r");
                        break;
                    case Direction.BACK:
                        animationHandler.ChangeAnimation("jump_u");
                        break;
                    case Direction.FORWARD:
                        animationHandler.ChangeAnimation("jump_d");
                        break;
                }
                break;
            case ACTION.AERIAL:
                switch (direction)
                {
                    case Direction.LEFT:
                        animationHandler.ChangeAnimation(speed.y >= 0.0f ? "air_l" : "fall_l");
                        break;
                    case Direction.RIGHT:
                        animationHandler.ChangeAnimation(speed.y >= 0.0f ? "air_r" : "fall_r");
                        break;
                    case Direction.FORWARD:
                        animationHandler.ChangeAnimation(speed.y >= 0.0f ? "air_d" : "fall_d");
                        break;
                    case Direction.BACK:
                        animationHandler.ChangeAnimation(speed.y >= 0.0f ? "air_u" : "fall_u");
                        break;
                }
                break;
        }
    }

    void HandleActions()
    {
        switch(action.current)
        {
            case ACTION.JUMPING: //Jumping
                if (action.timer >= JUMP_SQUAT_DURATION)
                {
                    speed.y = jumpSpeed;
                    rBody.linearVelocity = speed;
                    animationHandler.Stretch(0.6f); //Stretch a bit
                    action.Transition(ACTION.AERIAL);
                    state.Transition(STATE.IDLE, 1); //Exit startup lag on the next frame
                }
                break;
        }
    }
    #endregion

    #region Debugging
    public Direction GetDirection() { return direction; }
    public STATE GetState() { return state.current; }
    public string GetActionString() { return action.current.ToString(); }
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
        GameplayManager.audioPlayer.PlaySFX("test/Boing");
        action.Transition(ACTION.IDLE, 1); //Transition to being in the air as a state

        base.OnLanding();
    }

    protected override void OnAerial()
    {
        base.OnAerial();
    }

    /// <summary> Triggered by NPCs when they talk to the player (or vice versa) </summary>
    public void OnTalk()
    {
        state.Transition(STATE.TALKING);
        GameplayManager.clock.FreezeClock(); //Freeze time while talking
    }

    public void EndTalk()
    {
        state.Transition(STATE.IDLE, 4);
        GameplayManager.clock.UnfreezeClock(); //Unfreeze time when done talking
    }

    const int PICKUP_DELAY = 6; //Number of frames to be locked while picking up an item
    //Triggered when an object is picked up. Called by the object being picked up which sends a reference to itself.
    public void OnPickUp(PickUpObject obj)
    {
        STATE prevState = state.current; //Preserve the current STATE
        state.Transition(STATE.LOCKED); //Lock the player while picking up the item
        state.Transition(prevState, PICKUP_DELAY); //Transition back to the previous STATE after a few frames of being locked
        heldObject = obj; //Set the held object
    }

    const float PUT_DOWN_DIST = 0.22f;
    //Called when putting an object down
    public void OnPutDown()
    {
        //TODO: Consider raycasting to find out if we're putting the item out of bounds
        //Let the object know it's being put down
        heldObject.transform.position = transform.position + PUT_DOWN_DIST*forwardVector;
        STATE prevState = state.current; //Preserve the current action
        state.Transition(STATE.LOCKED);
        state.Transition(prevState, PICKUP_DELAY); //Transition back to the previous STATE after a few frames of being locked
        heldObject.Putdown();
        heldObject = null; //Set the heldObject to be nothing, now
    }
    #endregion

    #region Saving Related Overloads

    public override void LoadFromSave(Save save)
    {
        //Overridden so that the player's position is not obtained from a file
        ObjectData temp;

        //Try to read ObjectData from the save file
        if (save.objectData.TryGetValue(uniqueId, out temp))
            this.lastUpdateTime = temp.lastUpdateTime;
        else //If we could not find an existing object data, set to default values
            UpdateLastUpdateTime();
    }

    #endregion

    #region Lock and Loading Related Methods

    public void SetLoading()
    {
        state.Transition(STATE.LOADING);
    }

    public void SetLoading(Vector3 walk_direction)
    {
        state.Transition(STATE.LOADING);

        switch (direction)
        {
            case Direction.LEFT:
                animationHandler.ChangeAnimation(isGrounded ? "walk_l" : (speed.y >= 0.0f ? "air_l" : "fall_l"));
                break;
            case Direction.RIGHT:
                animationHandler.ChangeAnimation(isGrounded ? "walk_r" : (speed.y >= 0.0f ? "air_r" : "fall_r"));
                break;
            case Direction.FORWARD:
                animationHandler.ChangeAnimation(isGrounded ? "walk_d" : (speed.y >= 0.0f ? "air_d" : "fall_d"));
                break;
            case Direction.BACK:
                animationHandler.ChangeAnimation(isGrounded ? "walk_u" : (speed.y >= 0.0f ? "air_u" : "fall_u"));
                break;
        }
    }

    public void SetDoneLoading()
    {
        state.Transition(STATE.IDLE);
    }

    #endregion

    #region Misc Methods
    void HandleMovement()
    {
        if (state.current == STATE.LAG) //Don't allow movement if the Player is in endlag/startup
            return;

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
            action.Transition(ACTION.JUMPING);
            state.Transition(STATE.LAG);
            return;
        }
        else
            speed.y = rBody.linearVelocity.y;

        rBody.linearVelocity = speed;

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

            if (state.current == STATE.IDLE)
                state.Transition(STATE.MOVING);

            if (isGrounded && state.timer % smokeFreq == 0)
                effectDict.MakeEffect("walk_smoke", transform.position + Vector3.down * raycastOffset - 0.06f*forwardVector);
        }
        else if (state.current == STATE.MOVING) //If no inputs and considered moving, switch to IDLE
            state.Transition(STATE.IDLE);
    }

    public bool IsHoldingObject() { return heldObject != null; }

    public float DistanceToPlayer(Vector3 position)
    {
        return (transform.position - position).magnitude;
    }

    #endregion
}
