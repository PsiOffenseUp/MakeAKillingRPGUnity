using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingTrigger : MonoBehaviour
{
    [SerializeField] BoxCollider boxTrigger;
    [SerializeField] string sceneToLoad;
    [SerializeField] Vector3 spawnPosition;

    // Start is called before the first frame update
    void Start()
    {

    }

    //Called when something enters the trigger
    private void OnTriggerEnter(Collider other)
    {
        //Check if the object that entered is the player
        if(other.gameObject == GameplayManager.player.gameObject)
        {
            //Turn off the collider for this object
            boxTrigger.enabled = false;
            StartCoroutine(GameplayManager.gameplayManager.LoadNextRoom(sceneToLoad, transform, spawnPosition));
        }
    }

}
