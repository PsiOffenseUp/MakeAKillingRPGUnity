using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using System.IO; //Used for save files
using System.Xml;
using System.Xml.Serialization;

using BoogalooGame;

public class GameplayManager : MonoBehaviour
{
	[SerializeField] Camera defaultCamera;
	//--------------Members--------------------
	//Necessary references

	//Important static references
	public static PlayerController player { get; private set; }
	public static Camera mainCamera { get; private set; }
	public static LookingCamera customCam { get; private set; }

	//Other managers
	public static GameplayManager gameplayManager { get; private set; }
	public static ControlManager controlManager { get; private set; }
	public static AudioPlayer audioPlayer { get; private set; }
	public static UIManager uiManager { get; private set; }
	public static DialogueManager dialogueManager { get; private set; }
	public static DebugManager debugManager { get; private set; }
	public static Clock clock { get; private set; }

	//Save data and settings (things to save to files)
	public static Save saveData { get; private set; }
	public static Settings settings { get; private set; }
	public static RoomInfo currentRoom { get; private set; }
	//[SerializeField] List<RoomInfo> allRooms;

	//Game related variables
	public static ulong frameTimer { get; private set; }
	public static bool paused { get; private set; }
	public static double gameTimer { get; private set; } //Overall game time
	static ulong lastRand; //Last RNG value

	//Collision variables
	public static int collisionLayer { get; private set; }

	//Constants
	const int MAX_FRAME_RATE = 60;

	#region Unity Overrides
	private void Awake()
    {
		paused = false;
		frameTimer = 0;
		lastRand = (ulong)System.DateTime.Now.Millisecond;

		if (gameplayManager != null) //If another GameplayManager already exists, delete this one
			Destroy(this.gameObject);
		else
		{
			//Set frame rate
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = MAX_FRAME_RATE;

			if (saveData == null) LoadSave(0); //If no save data is loaded, load save 0 DEBUG

			//Get references to all of the other Manager objects
			controlManager = GetComponent<ControlManager>();
			clock = GetComponent<Clock>();
			clock.LoadFromSave(saveData); //Make sure the clock gets loaded from save
			mainCamera = defaultCamera;
			customCam = mainCamera.GetComponent<LookingCamera>();
			audioPlayer = GetComponent<AudioPlayer>();
			uiManager = GetComponent<UIManager>();
			dialogueManager = GetComponent<DialogueManager>();
			debugManager = GetComponent<DebugManager>();
			gameplayManager = this;
			collisionLayer = LayerMask.GetMask("Collision");

			Debug.Log(clock.time.ToString());

			//Initialize UI
			uiManager.Initialize();
			uiManager.SetMoneyValueImmediately(saveData.money);
			uiManager.UpdateTimeOfDay();

			DontDestroyOnLoad(this.gameObject); //Have the GameplayManager persist across scenes

			//Set up settings
			LoadSettings();
			ApplySettings();

			//Load data from files
			ItemTable.Initialize(); //Initialize item table data from file

			SceneManager.sceneLoaded += OnSceneLoaded; //Add 

			debugMode = false;
		}
	}

    private void Start()
    {
		customCam.SetFocus(player.gameObject);
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

    #region Loading Methods
	public static void SetRoomInfo(RoomInfo roomInfo)
    {
		currentRoom = roomInfo;
    }

	//Called whenever a scene is loaded
	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{

	}

	const float LOADING_TRANS_DURATION = 1.2f; //Length in seconds of loading screen transition
	const float ANIMATION_DELAY = 0.45f; //How many seconds to wait before playing transition animation

	//Loads the next room and plays the transition animation
	public IEnumerator LoadNextRoom(string scene_name, Transform base_trans, Vector3 spawn_pos)
	{
		//Start loading the next scene asynchronously
		//float percentDone;

		//Move the player towards the center of the loading zone
		Vector3 exitDirection = base_trans.position - player.transform.position;
		exitDirection.Normalize();
		player.SetLoading(exitDirection);
		StartCoroutine(player.MoveInDirection(exitDirection, 1.2f, ANIMATION_DELAY + LOADING_TRANS_DURATION));

		//Save while loading the next screen and show the saving icon
		StartCoroutine(SaveAndShowIcon());

		//Start playing the loading screen animation
		yield return new WaitForSeconds(ANIMATION_DELAY);
		uiManager.LockLoadingScreen();
		StartCoroutine(uiManager.LoadingScreenIn(LOADING_TRANS_DURATION));

		//Wait for the loading screen animation before loading the next scene
		while (uiManager.loadingScreenPlaying)
			yield return null;

		//Start loading the next scene
		/*AsyncOperation op = SceneManager.LoadSceneAsync(scene_name);

		while (!op.isDone)
		{
			percentDone = op.progress;
			yield return null;
		}*/
		SceneManager.LoadScene(scene_name);

		//Now that the scene is loaded, place the Player and camera at the given position
		player.transform.position = spawn_pos;
		mainCamera.transform.position = spawn_pos;
		player.SetDoneLoading();

		//Hide loading screen with animation
		uiManager.LockLoadingScreen();
		StartCoroutine(uiManager.LoadingScreenOut(LOADING_TRANS_DURATION));
	}

	public void LoadSave(uint save_number = 0)
    {
		//Load save file
		saveData = BoogalooGame.Save.ReadSaveFile(new BoogalooGame.SaveMetadata(save_number));
	}

	IEnumerator SaveData()
    {
		//Copy relevant values to save file
		TimeAffectedObject.SaveAllTimeAffectedObjects();

		//Save the clock's state
		clock.CopyToSave(saveData);

		//Write save file
		saveData.WriteSaveFile();

		yield return null;
	}

	IEnumerator SaveAndShowIcon()
    {
		uiManager.ShowSavingIcon();
		StartCoroutine(SaveData());
		uiManager.HideSavingIcon();
		yield return null;
    }

	//Saves the game
	public void SaveGame(bool show_save_icon = true)
    {
		if (show_save_icon)
			StartCoroutine(SaveAndShowIcon());
		else
			StartCoroutine(SaveData());

	}

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
	public static void SetPlayer(PlayerController player) { GameplayManager.player = player; }
	public static void SetMainCamera(Camera camera) 
	{ 
		mainCamera = camera;
		customCam = mainCamera.GetComponent<LookingCamera>();
		uiManager.SetRenderCamera(camera);
		debugManager.SetRenderCamera(camera);
	}

	//Returns a random integer between 0 and the max
	public static ulong GetRandomInt(ulong max)
    {
		//Do some silly BS
		lastRand += clock.time.minute + (uint)saveData.money * (clock.time.hour + (frameTimer % 1024)) + 2;
		lastRand %= int.MaxValue;

		return lastRand % max;
    }
	#endregion

	//****Debug mode****
	bool debugMode = false;
	void ToggleDebugMode() { debugMode = !debugMode; }
}
