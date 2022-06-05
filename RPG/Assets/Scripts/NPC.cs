using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : TimeAffectedObject
{
    [SerializeField] CapsuleCollider talkTriggerRange;
    [SerializeField] GameObject talkEffect;

    STATE storedState; //State before they started talking

    //Checking if the player is in range to talk
    RaycastHit raycastInfo; //Info about raycasts we do to the player
    Ray ray;
    const float maxTalkDistance = 10.0f;
    bool playerFacing;

    protected override void Awake()
    {
        base.Awake();
    }

    protected void Start()
    {
    }

    #region Trigger related methods
    protected virtual void OnTriggerEnter(Collider other)
    {
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        //Give the ability to talk to this NPC if it is idle
        if(currentState == STATE.IDLE)
        {

            GameplayManager.player.forwardVector.Normalize(); //Make sure it's normalized
            //Check if the player is facing us
            playerFacing = solidCollider.Raycast(
                        new Ray(GameplayManager.player.transform.position, GameplayManager.player.forwardVector),
                        out raycastInfo, maxTalkDistance
                        );

            talkEffect.SetActive(playerFacing);
            //Initiate talking if the player is facing us and pressed the button to start talking
            if (currentState != STATE.TALKING && playerFacing && ControlManager.AttackPressed() && 
                GameplayManager.player.currentState != STATE.TALKING)
            {
                //Call events for starting a talk
                OnStartTalk();
                GameplayManager.player.OnTalk();
            }
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        talkEffect.SetActive(false); //Make the talk effect no longer show
    }
    #endregion

    #region Events related to talking
    /// <summary>
    /// Called when the player first initiates starting talking with this NPC. Use this to queue up proper dialogue
    /// </summary>
    protected virtual void OnStartTalk()
    {
        storedState = currentState;
        TransitionState(STATE.TALKING);
        GameplayManager.dialogueManager.SetTalkingNPC(this); //Mark this NPC as the one talking

        //DEBUG
        GameplayManager.dialogueManager.EnqueueDialogue(new BoogalooGame.Dialogue(
            "Hey... <link=\"wait_0.5\"></link> <color=#db21d2><size=140%><link=\"spin\">Dude!</link></size></color>" +
            "<link=\"wait_1.2\"></link><link=\"wave_4\">\nWhat's up?</link>",
            "Test NPC"));

        GameplayManager.dialogueManager.EnqueueDialogue(new BoogalooGame.Dialogue(
            "Things sure got real <link=\"chspd_2.0\"></link><color=#0489c2><size=140%><link=\"wave_8\">funky</link></size></color><link=\"resspd\"></link> real quick, huh?",
            "Test NPC"
            ));
    }

    public virtual void OnEndTalk()
    {
        TransitionState(storedState, 4); //Restore the state from before they were talking
    }
    #endregion
}
