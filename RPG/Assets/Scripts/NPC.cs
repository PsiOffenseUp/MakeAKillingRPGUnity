using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : TimeAffectedObject
{
    [SerializeField] CapsuleCollider talkTriggerRange;
    [SerializeField] GameObject talkEffect;

    //Checking if the player is in range to talk
    RaycastHit raycastInfo; //Info about raycasts we do to the player
    const float maxTalkDistance = 10.0f;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnTriggerStay(Collider other)
    {
        //Give the ability to talk to this NPC if it is idle
        if(currentState == STATE.IDLE)
        {
            //Check if the player is facing us
            switch (GameplayManager.player.GetDirection())
            {
                case Direction.LEFT:                
                    if(talkTriggerRange.Raycast(
                        new Ray(GameplayManager.player.transform.position, -GameplayManager.mainCamera.transform.right),
                        out raycastInfo, maxTalkDistance
                        ))
                    {

                    }
                    break;
                case Direction.RIGHT:
                    break;
                case Direction.FORWARD:
                    break;
                case Direction.BACK:
                    break;
            }
        }
    }
}
