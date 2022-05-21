using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO; //Used for save files
using System.Xml;
using System.Xml.Serialization;

public class GameplayManager : MonoBehaviour
{

	//--------------Members--------------------
	//Necessary references

	//Important static references
	public static GameObject player { get; private set; }
	public static Camera mainCamera { get; private set; }

	//Other managers
	public static GameplayManager gameplayManager { get; private set; }
	public static ControlManager controlManager { get; private set; }
	public static AudioPlayer audioPlayer { get; private set; }
	public static UIManager uiManager { get; private set; }
	public static DialogueManager dialogueManager { get; private set; }
	public static DebugManager debugManager { get; private set; }
	public static Clock clock { get; private set; }

	//Save data and settings (things to save to files)
	public static BoogalooGame.Save saveData { get; private set; }
	public static BoogalooGame.Settings settings { get; private set; }

	//Game related variables
	public static ulong frameTimer { get; private set; }
	public static bool paused { get; private set; }
	public static double gameTimer { get; private set; } //Overall game time

	//Collision variables
	public static int collisionLayer { get; private set; }

	//Constants
	const int maxFrameRate = 60;

	#region Unity Overrides
	private void Awake()
    {
		paused = false;
		frameTimer = 0;

		if (gameplayManager != null) //If another GameplayManager already exists, delete this one
			Destroy(this.gameObject);
		else
		{
			//Set frame rate
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = maxFrameRate;

			//Get references to all of the other Manager objects
			controlManager = GetComponent<ControlManager>();
			audioPlayer = GetComponent<AudioPlayer>();
			uiManager = GetComponent<UIManager>();
			dialogueManager = GetComponent<DialogueManager>();
			debugManager = GetComponent<DebugManager>();
			clock = GetComponent<Clock>();
			gameplayManager = this;
			collisionLayer = LayerMask.GetMask("Collision");

			DontDestroyOnLoad(this.gameObject); //Have the GameplayManager persist across scenes

			//Set up settings
			LoadSettings();
			ApplySettings();

			//imagePrefab = GetPrefab("GUI/Image Prefab");

			debugMode = false;
		}
	}

    private void Update()
    {
		frameTimer++;

		//Update timers
		if (!paused)
			gameTimer += Time.deltaTime;

		//Debug mode stuff
		if (Input.GetKeyDown(KeyCode.B) && Input.GetKeyDown(KeyCode.S)) //Trigger Debug mode on B & S pressed
		{
			ToggleDebugMode();
			if (debugManager != null)
			{
				if (debugMode)
					debugManager.Activate();
				else
					debugManager.Deactivate();
			}
		}
	}
	#endregion

	#region Static Variables
	public static void Pause() { paused = true; }
	public static void Unpause() { paused = false; }
	public static void TogglePause() { paused = !paused; }
    #endregion

    #region Resource methods

    /// <summary> Plays the sound with the given path within the sounds folder </summary>
    /// <param name="sound_path"> Path string for the sound to be played, after "sounds/" </param>
    public static void PlaySFX(string sound_path) { audioPlayer.PlaySFX(Resources.Load<AudioClip>("sounds/" + sound_path)); }

	/// <summary> Gets a resource from the Resources/prefab folder and then returns it as a GameObject. </summary>
	/// <param name="path"> Path to the prefab, following "/Resources/" </param>
	public static GameObject GetPrefab(string path) { return Resources.Load<GameObject>("prefabs/" + path); }

	/// <summary> Instantiates a prefab GameObject and then returns it. This loads prefabs from the "Resources/prefabs" directory </summary>
	/// <returns></returns>
	public static GameObject InstantiatePrefab(string resource_path) { return Instantiate(Resources.Load<GameObject>("prefabs/" + resource_path)); }

	/// <summary> Instantiates a prefab GameObject and then returns it. This loads prefabs from the "Resources/prefabs" directory </summary>
	/// <param name="resource_path"></param>
	/// <returns></returns>
	public static GameObject InstantiatePrefab(string resource_path, Transform original) { return Instantiate(Resources.Load<GameObject>("prefabs/" + resource_path), original); }

	#endregion

	#region Other managers
	/// <summary> Enables all managers other than the GameplayManager, AudioPlayer, and ControlManager </summary>
	public static void EnableManagers() { uiManager.enabled = dialogueManager.enabled = debugManager.enabled = true; }
	/// <summary> Disables all managers other than the GameplayManager, AudioPlayer, and ControlManager </summary>
	public static void DisableManagers() { uiManager.enabled = dialogueManager.enabled = debugManager.enabled = false; }

	#endregion

	#region Settings methods
	public void LoadSettings()
	{
		settings = BoogalooGame.Settings.ReadSettings(); //Read the settings into our settings object

		if (settings == null) //If there is no settings file yet, create one
		{
			settings = new BoogalooGame.Settings(); //Make settings into the default settings
			SaveSettings(); //Save the save file for later
		}

		ApplySettings();
	}

	/// <summary>
	/// Applies the settings from the settings object
	/// </summary>
	public void ApplySettings()
	{
		Screen.SetResolution(settings.GetResolution().width, settings.GetResolution().height, settings.isFullscreen); //Set the resolution settings
		audioPlayer.SetVolume(settings.sfxVolume, settings.musicVolume);
	}

	public static void SetSettings(BoogalooGame.Settings new_settings) { settings = new_settings; }

	public void SaveSettings() { settings.WriteSettings(); }
	#endregion

	#region Static Methods
	public static void SetPlayer(GameObject player) { GameplayManager.player = player; }
	public static void SetMainCamera(Camera camera) 
	{ 
		mainCamera = camera;
		uiManager.SetRenderCamera(camera);
		debugManager.SetRenderCamera(camera);
	}
	#endregion

	//****Debug mode****
	bool debugMode = false;
	void ToggleDebugMode() { debugMode = !debugMode; }
}
