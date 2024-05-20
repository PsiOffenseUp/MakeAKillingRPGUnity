using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BoogalooGame;

public enum STATE { IDLE, MOVING, AERIAL, LAG, TALKING, LOCKED, LOADING } //States which are shared across objects

/// <summary>
/// Parent class for objects in game, such as NPCs, enemies, and interactive items
/// </summary>
public abstract class Object : MonoBehaviour, ISaveable
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

	protected bool canMove, applyPhysics;

	protected Vector3 speed;

	//Collisions / checking the ground
	protected CapsuleCollider solidCollider { get; private set; }
	protected float raycastOffset { get; private set; }
	protected const float raycastDist = 0.1f;
	protected bool isGrounded { get; private set;}
	protected bool wasGrounded { get; private set;}

	//Unique id
	public string uniqueId { get; private set; }

	//Generic States
	protected StateMachine<STATE> state;

    #region Unity Method Overrides
    protected virtual void Start()
    {
		LoadFromSave(GameplayManager.saveData); //Try to load this object from the save data
	}

    protected virtual void Awake()
	{
		initialPosition = transform.position;

		objectDict.Add(gameObject, this);

		state = new StateMachine<STATE>(STATE.IDLE); //Initialize state machine

		animationHandler.Initialize(); //Initialize the animator
		effectDict.Initialize(); //Initialize the effects dictionary

		solidCollider = GetComponent<CapsuleCollider>();
		raycastOffset = solidCollider.height - 0.03f;
		isGrounded = false;
		forwardVector = transform.forward;
		forwardVector.Normalize();

		canMove = true;
		applyPhysics = true;

		uniqueId = GenerateUniqueID();
	}

	protected virtual void Update()
	{
		if (!applyPhysics)
			return;

		//Handle collision
		wasGrounded = isGrounded;
		isGrounded = Physics.Raycast(transform.position + Vector3.down * raycastOffset, Vector3.down, raycastDist, GameplayManager.collisionLayer);
		if (!wasGrounded && isGrounded) //If this is the first frame on the ground, call the OnLanding event method
			OnLanding();
		if (wasGrounded && !isGrounded)
			OnAerial();

		animationHandler.OnUpdate(); //Update any animations

		state.Update(); //Handle state machine updating

		//base.Update();
	}

	#endregion

	#region ***************** Movement-Related Methods *****************
	public void PausePhysics() { applyPhysics = false; }
	public void ResumePhysics() { applyPhysics = true; }
	public void TogglePhysics() { applyPhysics = !applyPhysics; }
	public void PauseMovement() { canMove = false; }
	public void ResumeMovement() { canMove = true; }
	public void ToggleMovement() { canMove = !canMove; }
	/* Moves the object to the given position until it reaches it at the given speed */
	public IEnumerator MoveToPosition(Vector3 position, float speed)
    {
		canMove = false;
		applyPhysics = false;

		//Continue to move until the distance is short enough
		while (Vector3.Distance(transform.position, position) < 0.04f)
        {
			//Update the position of the object towards its goal
			transform.position = Vector3.Lerp(transform.position, position, speed);
			yield return null;
        }

		//Re-enable controls
		canMove = true;
		applyPhysics = true;
	}
	/* Moves the object in the given direction with the speed for the given amount of time */
	public IEnumerator MoveInDirection(Vector3 direction, float speed, float seconds = 120.0f)
    {
		float elapsedTime = 0.0f;
		canMove = false; //Disable movement

		while(elapsedTime < seconds)
        {
			elapsedTime += Time.deltaTime;
			transform.position += Time.deltaTime * direction * speed; //Move in the given direction
			yield return new WaitForEndOfFrame(); //Pause the coroutine until the next frame
        }

		canMove = true; //Re-enable movement
    }
	#endregion

	#region ***************** Save-Related Methods *****************

	public virtual void CopyToSave(Save save)
	{
	}

	public virtual void LoadFromSave(Save save)
	{
	}

	#endregion

	#region ***************** Other Methods *****************
	//Called on the first frame when an object hits the ground
	protected virtual void OnLanding() { animationHandler.Squash(-0.2f * speed.y); }
	protected virtual void OnAerial() { state.Transition(STATE.AERIAL); }

	string GenerateUniqueID() {  
		return gameObject.name; //TODO find something better
	}
	#endregion
}
