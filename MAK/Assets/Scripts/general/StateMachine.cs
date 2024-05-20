using UnityEngine;

//State and Action enums
public enum GENERIC_ACTION { IDLE, AERIAL, LOCKED, ACTION0, ACTION1 } //TODO: Move this to specific uses

/// <summary>
/// Templated class for a State machine, allowing a class to 
/// </summary>
/// <typeparam name="T"> Enum </typeparam>
public class StateMachine<T>
{
    public T previous { get; private set; }
    public T current { get; private set; }
    private T target;
    bool transitioning = false;
    public uint transitionTimer { get; private set; } //How many frames are left to spend transitioning
    public uint timer { get; private set; } //How many frames the current state has been active for

    #region Transitioning Methods

    public delegate void TransitionCallback(T previous, T next);

    public TransitionCallback OnTransition; //Method for when a transition occurs

    //Transitions to the target_state after the transition_time many frames
    public void Transition(T target, uint transition_time)
    {
        //currentState = targetState; //Transition to any pending state
        this.target = target;
        transitionTimer = transition_time;
        transitioning = true;
    }

    //Transitions to the target immediately
    public void Transition(T target)
    {
        //Immediately transition if there is no time
        this.target = target;
        Transition();       
    }

    //Transitions to the cached target state
    private void Transition()
    {
        transitionTimer = 0;
        timer = 0;
        previous = current; //Record the state we were just in
        current = target; //Update to the target state
        transitioning = false; //Stop transitioning
        OnTransition(previous, current); //Call the state transition event
    }
    #endregion

    #region Other Methods

    public void Update()
    {
        timer++;

        //Handle transitioning
        if (transitioning)
        {
            if (transitionTimer > 0)
                transitionTimer--;

            if (transitionTimer == 0) //If the transition is finished, update action
                Transition();
        }
    }

    #endregion

    #region Constructors

    public StateMachine()
    {
        transitionTimer = timer = 0; //Set timers to 0
        OnTransition = (previous, current) => { }; //Initialize nothing to happen on transition
    }

    //Creates a State Machine starting in the initial state
    public StateMachine(T initial) : this()
    {
        current = target = initial;
    }

    #endregion
}



