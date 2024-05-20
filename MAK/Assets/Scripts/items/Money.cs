using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Money : MonoBehaviour
{
    [SerializeField] CapsuleCollider hitbox;
    [SerializeField] protected int worth;
    const float collectionSpeed = 21.5f; //Amount to move towards the player per frame
    Vector3 directionToPlayer;
    float ratio;

    // Update is called once per frame
    void Update()
    {
        directionToPlayer = GameplayManager.player.transform.position - transform.position;
        if (directionToPlayer.magnitude < GameplayManager.player.collectionRadius)
        {
            hitbox.enabled = false; //Disable hitbox while player is attracting money
            ratio = (GameplayManager.player.collectionRadius - directionToPlayer.magnitude) / GameplayManager.player.collectionRadius;
            ratio = 1.0f - ratio;
            directionToPlayer.Normalize(); //Make unit
            transform.position += ratio*collectionSpeed*Time.deltaTime*directionToPlayer; //Move towards player
        }
        else
            hitbox.enabled = true;
    }

    public void OnTriggerEnter(Collider other)
    {
        //If we collided with the player, collect this money
        if (other.gameObject == GameplayManager.player.gameObject)
            Collect();
    }

    //Called when a money item is collected
    protected virtual void Collect()
    {
        GameplayManager.saveData.money += worth; //Add the money to the total
        GameplayManager.audioPlayer.PlaySFX("items/coin");
        //Play particle effect TODO
        Destroy(this.gameObject);
    }
}
