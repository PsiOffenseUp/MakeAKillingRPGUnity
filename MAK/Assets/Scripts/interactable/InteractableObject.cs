using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : TimeAffectedObject
{
    [SerializeField] CapsuleCollider interactTriggerRange;
    [SerializeField] GameObject interactEffect;
    [SerializeField] string actionString; //Text to display for interacting with this object

    //Checking if the player is in range to talk
    RaycastHit raycastInfo; //Info about raycasts we do to the player
    Ray ray;
    const float MAX_INTERACT_DIST = 10.0f;
    bool wasFacing, playerFacing;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    #region Trigger related methods
    protected virtual void OnTriggerEnter(Collider other)
    {
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        //Give the ability to talk to this NPC if it is idle
        if (state.current == STATE.IDLE)
        {
            GameplayManager.player.forwardVector.Normalize(); //Make sure it's normalized
            //Check if the player is facing us
            wasFacing = playerFacing;
            playerFacing = solidCollider.Raycast(new Ray(GameplayManager.player.transform.position,
                GameplayManager.player.forwardVector), out raycastInfo, MAX_INTERACT_DIST);

            if (!wasFacing && playerFacing) //If player turned towards NPC
            {
                interactEffect.SetActive(true);
                GameplayManager.uiManager.ChangeActionText(actionString);
            }
            else if (wasFacing && !playerFacing) //If player turns away
            {
                interactEffect.SetActive(false);
                GameplayManager.uiManager.ChangeActionTextToNone();
            }

            //Initiate talking if the player is facing us and pressed the button to start talking
            if (state.current != STATE.TALKING && playerFacing && ControlManager.RightTriggerPressed() &&
                GameplayManager.player.GetState() != STATE.TALKING)
                OnPlayerInteract();
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        interactEffect.SetActive(false); //Make the talk effect no longer show
    }
    #endregion

    #region Events for Interaction

    protected virtual void OnPlayerInteract()
    {

    }

    protected virtual void OnPlayerFinishInteract()
    {

    }

    #endregion
}
