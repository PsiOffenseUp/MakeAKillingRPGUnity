using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{

	[SerializeField] AudioSource musicSource, soundEffectSource;

	Dictionary<string, AudioClip> audioLibrary;
	AudioClip currentSFX;

	BoogalooGame.Song previousSong, currentSong;

	enum PlayerState { STOPPING, PLAYING, CHANGING }
	PlayerState state;

	const float fadeSpeed = 0.02f;

    private void Awake()
    {
		previousSong = currentSong = null;
		audioLibrary = new Dictionary<string, AudioClip>();
	}

    // Start is called before the first frame update
    void Start()
    {
		state = PlayerState.CHANGING;
		musicSource.volume = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
		if (currentSong == null) //If there is no song to play, just bail
			return;

		//Have the player figure out what to do based on the state
		switch (state)
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

    #region Play Methods
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
	public void PlaySFX(AudioClip sound_effect) { 
		soundEffectSource.volume = GameplayManager.settings.sfxVolume; 
		soundEffectSource.PlayOneShot(sound_effect); 
	}

	/// <summary>
	/// Plays a sound effect using the audio source present.
	/// </summary>
	/// <param name="sound_effect"></param>
	public void PlaySFX(AudioClip sound_effect, float volume)
	{
		soundEffectSource.volume = volume;
		soundEffectSource.PlayOneShot(sound_effect);
	}

	/// <summary>
	/// Plays a sound effect using the audio source present and at the given point in space
	/// </summary>
	/// <param name="sound_effect"></param>
	public void PlaySFX(AudioClip sound_effect, Vector3 position)
	{
		AudioSource.PlayClipAtPoint(sound_effect, position, GameplayManager.settings.sfxVolume);
	}

	/// <summary>
	/// Plays a sound effect using the audio source present and at the given point in space
	/// </summary>
	/// <param name="sound_effect"></param>
	public void PlaySFX(AudioClip sound_effect, Vector3 position, float volume)
	{
		AudioSource.PlayClipAtPoint(sound_effect, position, volume);
	}

	/// <summary>
	/// Plays the given sound effect after the delay is up.
	/// </summary>
	/// <param name="sound_effect"></param>
	/// <param name="delay"></param>
	/// <param name="volume"></param>
	/// <returns></returns>
	public IEnumerator PlaySFXDelayed(AudioClip sound_effect, float delay) { 
		yield return new WaitForSeconds(delay); 
		PlaySFX(sound_effect, GameplayManager.settings.sfxVolume); 
	}

	//Plays the sound effect from the given path relative to the sound resources
	public void PlaySFX(string sound_path)
    {
		PlaySFX(GetSoundEffect(sound_path), GameplayManager.settings.sfxVolume);
	}

	//Plays the sound effect from the given path relative to the sound resources. Plays at the point in space given
	public void PlaySFX(string sound_path, Vector3 point)
	{
		PlaySFX(GetSoundEffect(sound_path), GameplayManager.settings.sfxVolume);
	}


	public void Pause() { musicSource.Pause(); }
	public void Resume() { musicSource.Play(); }
	public void Stop() { musicSource.Stop(); }

	#endregion

	#region Asset Related Methods
	AudioClip GetSoundEffect(string sound_path)
    {
		AudioClip clip;

		if (audioLibrary.TryGetValue(sound_path, out clip))
			return clip;
		
		clip = Resources.Load<AudioClip>("sounds/" + sound_path); //If the clip is not found, load it from resources
		audioLibrary[sound_path] = clip; //Add the loaded clip to the library

		return clip;
	}
	#endregion

	#region Helper Methods
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
			soundEffectSource.volume = sfx_volume;
			musicSource.volume = music_volume * currentSong.volume;
		}
		catch { soundEffectSource.volume = musicSource.volume = 0.0f; }
	}

	public void SetSFXVolume(float sfx_volume)
	{
		soundEffectSource.volume = sfx_volume;
	}

	public void SetMusicVolume(float music_volume)
	{
		musicSource.volume = music_volume * currentSong.volume;
	}
	#endregion
}
