using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object to start a song on level start, and then destroys itself
/// </summary>
public class MusicObject : MonoBehaviour
{
	[SerializeField] BoogalooGame.Song song;
	bool hasSentSong;

    void Awake()
    {
		hasSentSong = false;
    }

    // Update is called once per frame
    void Update()
    {
		GameplayManager.audioPlayer.PlaySong(song); //Try to send the song to the AudioPlayer, assuming the reference has been made
	    Destroy(this.gameObject);
    }
}
