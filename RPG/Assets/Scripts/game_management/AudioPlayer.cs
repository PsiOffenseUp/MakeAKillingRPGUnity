using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{

	[SerializeField] AudioSource musicSource, soundEffectSource;

	BoogalooGame.Song previousSong, currentSong;

	enum PlayerState { STOPPING, PLAYING, CHANGING }
	PlayerState state;

	const float fadeSpeed = 0.02f;

    // Start is called before the first frame update
    void Start()
    {
		previousSong = currentSong = null;
		state = PlayerState.CHANGING;
		musicSource.volume = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
		if (currentSong == null) //If there is no song to play, just bail
			return;

        //Have the player figure out what to do based on the state
		switch(state)
		{
			case PlayerState.STOPPING: //If the player is stopping, make the audio fade out, then swap the song
				musicSource.volume -= fadeSpeed;
				if (musicSource.volume <= 0.0f) //If the music has faded out all the way, switch to the next song
				{
					musicSource.Stop(); //Stop the music player
					musicSource.clip = currentSong.clip;
					musicSource.Play();
					state = PlayerState.CHANGING;
				}
				break;
			case PlayerState.CHANGING:
				musicSource.volume = Mathf.Lerp(musicSource.volume, GameplayManager.settings.musicVolume * currentSong.volume, fadeSpeed);
				if (musicSource.volume >= GameplayManager.settings.musicVolume * currentSong.volume - 0.01f) //If the song has faded all the way in (with a 1% difference to account for float rounding errors)
					state = PlayerState.PLAYING;
				break;
			case PlayerState.PLAYING:
				if(!musicSource.isPlaying) //If the clip finished, check what to do
				{
					//Check if the song is over, and loop if applicable
					if (currentSong.isLooping)
					{
						musicSource.clip = currentSong.clip;
						musicSource.time = currentSong.loopPoint;
						musicSource.Play();
					}
					else
						currentSong = null; //If the song does not loop, get rid of it
				}
				break;
			default:
				break;
		}
    }

	/// <summary>
	/// /// Plays the given song, fading out the current song and then swapping
	/// </summary>
	/// <param name="song"></param>
	public void PlaySong(BoogalooGame.Song song)
	{
		previousSong = currentSong;
		currentSong = song;
		state = PlayerState.STOPPING;
	}

	/// <summary>
	/// Plays the given song at the given volume, fading out the current song and then playing the one given
	/// </summary>
	/// <param name="song"></param>
	/// <param name="volume"></param>
	public void PlaySong(AudioClip song, float volume = 0.3f, bool loop = false)
	{
		PlaySong(new BoogalooGame.Song(song, volume, 0.0f, loop)); //Set up the next song
	}

	/// <summary>
	/// Plays a sound effect using the audio source present.
	/// </summary>
	/// <param name="sound_effect"></param>
	public void PlaySFX(AudioClip sound_effect, float volume = 0.3f) { soundEffectSource.volume = volume; soundEffectSource.PlayOneShot(sound_effect); }
	/// <summary>
	/// Plays the given sound effect after the delay is up.
	/// </summary>
	/// <param name="sound_effect"></param>
	/// <param name="delay"></param>
	/// <param name="volume"></param>
	/// <returns></returns>
	public IEnumerator PlaySFX(AudioClip sound_effect, float delay, float volume = 0.3f) { yield return new WaitForSeconds(delay); PlaySFX(sound_effect, volume); }

	public void Pause() { musicSource.Pause(); }
	public void Resume() { musicSource.Play(); }
	public void Stop() { musicSource.Stop(); }
	public void RestoreLastSong ()
	{
		BoogalooGame.Song tempSong = currentSong;
		currentSong = previousSong;
		previousSong = tempSong;
		state = PlayerState.STOPPING;
	}

	public void SetVolume(float sfx_volume, float music_volume)
	{
		try
		{
			soundEffectSource.volume = sfx_volume * currentSong.volume;
			musicSource.volume = music_volume * currentSong.volume;
		}
		catch { soundEffectSource.volume = musicSource.volume = 0.0f; }
	}
}
