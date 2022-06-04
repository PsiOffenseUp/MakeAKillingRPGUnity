using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : TimeAffectedObject
{
    [SerializeField] CapsuleCollider talkTriggerRange;
    [SerializeField] GameObject talkEffect;

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
        GameplayManager.dialogueManager.EnqueueDialogue(new BoogalooGame.Dialogue(
            "Whoa... <link=\"wait_0.8\"></link> text effects!",
            "Test NPC"));
    }

    #region Trigger related methods
    protected virtual void OnTriggerEnter(Collider other)
    {
        //DEBUG
        //Debug.Log("Entered Trigger!");
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
            /*if()
            {

            }*/
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        talkEffect.SetActive(false); //Make the talk effect no longer show
    }
    #endregion
}
