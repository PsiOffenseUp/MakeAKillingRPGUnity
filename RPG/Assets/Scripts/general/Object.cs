using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Parent class for objects in game, such as NPCs, enemies, and interactive items
/// </summary>
public abstract class Object : MonoBehaviour
{
	//Used to figure out which direction the object is facing for animations
	public enum Direction { LEFT, RIGHT, BACK, FORWARD }
	protected Direction direction = Direction.RIGHT;

	public Vector3 initialPosition { get; private set; }
	public Vector3 forwardVector { get; protected set; } //Separate forward vector to avoid weird transform stuff
	public Vector3 rightVector { get; protected set; } //Separate right vector to avoid weird transform stuff

	static Dictionary<GameObject, Object> objectDict = new Dictionary<GameObject, Object>();
	[SerializeField] protected AnimationHandler animationHandler;
	[SerializeField] protected EffectDictionary effectDict;

	//Collisions / checking the ground
	protected CapsuleCollider solidCollider { get; private set; }
	protected float raycastOffset { get; private set; }
	protected const float raycastDist = 0.1f;
	protected bool isGrounded { get; private set;}
	protected bool wasGrounded { get; private set;}

	#region Unity Method Overrides
	protected virtual void Awake()
	{
		initialPosition = transform.position;

		objectDict.Add(gameObject, this);

		this.stateTimer = this.actionTimer = 0;

		animationHandler.Initialize(); //Initialize the animator
		effectDict.Initialize(); //Initialize the effects dictionary

		solidCollider = GetComponent<CapsuleCollider>();
		raycastOffset = solidCollider.height - 0.03f;
		isGrounded = false;
		forwardVector = transform.forward;
		forwardVector.Normalize();
	}

	protected virtual void Update()
	{
		//Handle collision
		wasGrounded = isGrounded;
		isGrounded = Physics.Raycast(transform.position + Vector3.down * raycastOffset, Vector3.down, raycastDist, GameplayManager.collisionLayer);
		if (!wasGrounded && isGrounded) //If this is the first frame on the ground, call the OnLanding event method
			OnLanding();

		animationHandler.OnUpdate(); //Update any animations

		//Handle states and actions
		actionTimer++;
		stateTimer++;

		//Handle action transitioning
		if(transitioningAction)
        {
			if (actionTransitionTimer > 0)
				actionTransitionTimer--;

			if (actionTransitionTimer == 0) //If the transition is finished, update action
			{
				actionTransitionTimer = 0; //Reset timers
				actionTimer = 0;
				previousAction = currentAction;
				currentAction = targetAction;
				transitioningAction = false;
				OnActionTransition();
			}
		}

		//Handle state transitioning
		if(transitioningState)
        {
			if (stateTransitionTimer > 0)
				stateTransitionTimer--;

			if (actionTransitionTimer == 0) //If the transition is finished, update action
			{
				stateTransitionTimer = 0;
				stateTimer = 0;
				previousState = currentState; //Record the state we were just in
				currentState = targetState; //Update to the target state
				transitioningState = false; //Stop transitioning
				OnStateTransition(); //Call the state transition event
			}
		}

		//base.Update();
	}

	#endregion

	#region ***************** State and Action methods *****************
	public enum ACTION { IDLE, AERIAL }
	public enum STATE { IDLE, MOVING, TALKING }

	public ACTION currentAction { get; private set; }
	protected ACTION previousAction { get; private set; }
	private ACTION targetAction;
	protected STATE previousState { get; private set; }
	public STATE currentState { get; private set; }
	private STATE targetState;

	protected uint actionTransitionTimer { get; private set; } //How many frames are left to spend transitioning
	protected uint stateTransitionTimer { get; private set; } //How many frames are left to spend transitioning
	protected uint actionTimer { get; private set; } //How many frames the current action has been active for
	protected uint stateTimer { get; private set; }//How many frames the current state has been active for
	bool transitioningAction = false; //Whether we are in the process of transitioning between actions
	bool transitioningState = false;

	protected virtual void OnActionTransition() { }
	protected virtual void OnStateTransition() { }
	protected void TransitionState(STATE target_state, uint transition_time = 0)
	{
		//currentState = targetState; //Transition to any pending state
		targetState = target_state;
		stateTransitionTimer = transition_time;
		transitioningState = true;
	}

	protected void TransitionAction(ACTION target_action, uint transition_time = 0)
    {
		targetAction = target_action;
		actionTransitionTimer = transition_time;
		transitioningAction = true;
	}
	#endregion

	#region ***************** Other Methods *****************
	//Called on the first frame when an object hits the ground
	protected virtual void OnLanding() { }
	#endregion
}
