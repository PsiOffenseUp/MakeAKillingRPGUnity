using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BoogalooGame;

public class PickUpObject : MonoBehaviour
{
    [SerializeField] new CapsuleCollider collider;
    [SerializeField] CapsuleCollider trigger;
    [SerializeField] new Rigidbody rigidbody;
    ItemData item;
    const float maxGrabDistance = 10.0f;

    //Variables for collision checks
    bool playerFacing;
    RaycastHit raycastInfo; //Info about raycasts we do to the player
    Ray ray;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Event Handlers
    public void Pickup()
    {
        //Disable collisions
        collider.enabled = false;
        trigger.enabled = false;
        rigidbody.isKinematic = false; //Turn off physics
        rigidbody.useGravity = false;

        //Make the player a parent of this object
        transform.SetParent(GameplayManager.player.transform);
    }

    public virtual void Putdown()
    {
        //Unparent the player
        transform.SetParent(null);

        //Possibly wait for the animation to play before re-enabling the collider

        collider.enabled = true;
        trigger.enabled = true;
        rigidbody.isKinematic = true;
        rigidbody.useGravity = true;
    }

    bool wasFacing = false;
    protected virtual void OnTriggerStay(Collider other)
    {
        //Do nothing if the object detected is not the player
        if (other.gameObject != GameplayManager.player.gameObject)
            return;

        //While the player is in range, check if they can pickup this item
        GameplayManager.player.forwardVector.Normalize(); //Make sure it's normalized
        //Check if the player is facing us
        playerFacing = collider.Raycast(
                    new Ray(GameplayManager.player.transform.position, GameplayManager.player.forwardVector),
                    out raycastInfo, maxGrabDistance
                    );

        if (!wasFacing && playerFacing) GameplayManager.uiManager.ChangeActionText("Pick Up");
        else if (wasFacing && !playerFacing) GameplayManager.uiManager.ChangeActionTextToNone();

        //talkEffect.SetActive(playerFacing);
        //Initiate picking up if the player is facing us and pressed the button to pick up
        if (playerFacing && ControlManager.RightTriggerPressed() &&
            !GameplayManager.player.IsHoldingObject() &&
            GameplayManager.player.GetState() != STATE.TALKING &&
            GameplayManager.player.GetState() != STATE.LOCKED)
        {
            //Call events for starting being picked up
            Pickup();
            GameplayManager.player.OnPickUp(this);
        }
    }

    #endregion
}
