﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;


/// <summary>
/// Miscelaneous helper classes that will not be attached to objects in game
/// </summary>

namespace BoogalooGame
{

	[System.Serializable] public class Song
	{
		public AudioClip clip; //AudioClip to play
		public float volume, loopPoint; //Volume and the time in seconds at which to restart the song when looping
		public bool isLooping;
		public string name;

		//-----------------Constructors---------------

		public Song(AudioClip clip, float volume, float loop_point, bool isLooping = true)
		{
			this.clip = clip;
			this.volume = volume;
			this.loopPoint = loop_point;
		}
		public Song(AudioClip clip, float volume) : this(clip, volume, 0.0f, false) { }
		public Song(AudioClip clip, float volume, float loop_point, bool isLooping, string name) : this(clip, volume, loop_point, isLooping) { this.name = name; }
		public Song(AudioClip clip) : this(clip, 0.5f) { }
	}

	/// <summary> Class containing all of the flags for game evenets, such as unlocks </summary>
	[System.Serializable] public class Flags
	{
		public enum GameFlag { 
			GAMESTART,
			FLAGCOUNT
		}

		[XmlArray("flag_arr")] [XmlArrayItem("flag")] bool[] flags;

		public void SetFlag(GameFlag flag_to_set) { flags[(int)flag_to_set] = true;  }
		public bool GetFlag(GameFlag flag_to_get) { return flags[(int)flag_to_get];  }

		Flags() 
		{ 
			this.flags = new bool[(int)GameFlag.FLAGCOUNT];
			for (int i = 0; i < (int)GameFlag.FLAGCOUNT; i++)
				flags[i] = false;
		}
	}

	/// <summary> Less data that we'll keep track of seperately so we don't have to read all of the data all at once. Used to display data on the file select screen. </summary>
	public class SaveMetadata
	{
		[XmlAttribute("time")] public double totalTime;

		public int fileNumber;

		//**************Reading and writing***************
		public static SaveMetadata ReadMetadata(int file_number)
		{
			if (File.Exists(Application.persistentDataPath + "/savemeta" + file_number + ".save"))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(SaveMetadata));
				FileStream stream = new FileStream(Application.persistentDataPath + "/savemeta" + file_number + ".boogaloo", FileMode.Open);
				return ((SaveMetadata)serializer.Deserialize(stream));
			}
			else //If there isn't yet a save file, make one
			{
				SaveMetadata metadata = new SaveMetadata(file_number);
				metadata.WriteMetadata();
				return metadata;
			}
		}

		public void WriteMetadata()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(SaveMetadata));
			FileStream stream = new FileStream(Application.persistentDataPath + "/savemeta" + fileNumber + ".boogaloo", FileMode.Create);
			serializer.Serialize(stream, this); //Write all of the save data to the file
			stream.Close();
		}

		public override string ToString()
		{
			return "\nTotal Time: " + totalTime.ToString();
		}

		//******************Constructor******************

		/// <summary> Used to create new save meta data </summary>
		/// <param name="file_number"></param>
		SaveMetadata(int file_number)
		{
			totalTime = 0.0f;
			fileNumber = file_number;
		}

		public SaveMetadata():this(0) { }
	}

	[System.Serializable] public class Save
	{
		[XmlElement("game_flags")] public Flags gameFlags;
		[XmlElement("hour")] public uint hour;
		[XmlElement("minute")] public uint minute;
		[XmlElement("time_passage")] public uint timeSpeed;
		public SaveMetadata metadata;
		//----------------------------Methods---------------------------

		#region Reading and writing files
		/// <summary> Reads the save file for the give save slot, and returns a Save object. If there is no save data, return null. </summary>
		public static Save ReadSaveFile(SaveMetadata metadata)
		{
			if (File.Exists(Application.persistentDataPath + "/level_info" + metadata.fileNumber + ".save"))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(Save));
				FileStream stream = new FileStream(Application.persistentDataPath + "/level_info" + metadata.fileNumber + ".boogaloo", FileMode.Open);
				return ((Save)serializer.Deserialize(stream));
			}
			else //If there isn't yet a save file, return null
				return null;
		}

		/// <summary> Writes the contents of the level array to the save file specified by the current slot at fileNumber. Also writes the metadata. </summary>
		public void WriteSaveFile()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(Save));
			FileStream stream = new FileStream(Application.persistentDataPath + "/level_info" + metadata.fileNumber + ".boogaloo", FileMode.Create);
			serializer.Serialize(stream, this); //Write all of the save data to the file
			stream.Close();

			metadata.WriteMetadata();
		}

		#endregion

		#region Metadata editing

		/// <summary> Sets the metadata values to the passed parameters. </summary>
		/// <param name="total_time">Total time in milliseconds that the player has played the game on this save file. </param>
		/// <param name="total_coins"> Total coins on this save file. </param>
		/// <param name="last_scene"> Last scene that the player was in. Used when reloading on death or loading the game.</param>
		public void UpdateMetadata(double total_time, int total_coins, string last_scene)
		{
			metadata.totalTime = total_time;
		}

		public void AddGameTime(double add_time) { metadata.totalTime += add_time;  }

		#endregion

		#region Flag methods

		public void SetFlag(Flags.GameFlag flag) { gameFlags.SetFlag(flag);  }

		public bool GetFlag(Flags.GameFlag flag) { return gameFlags.GetFlag(flag); }

		#endregion

		#region Helper methods

		#endregion

		//-------------------------Constructors---------------------
		/// <summary>
		/// Creates a new save object, but does not read anything from any files
		/// </summary>
		/// <param name="metadata"></param>
		public Save(SaveMetadata metadata) 
		{ 
			this.metadata = metadata;
		}

		public Save() { metadata = new SaveMetadata(); }

	}
	[System.Serializable] public class Resolution
	{
		[XmlAttribute("width")] public int width { get; private set; }
		[XmlAttribute("height")] public int height { get; private set; }

		public Resolution(int width, int height) { this.width = width; this.height = height; }
		public override string ToString() { return this.width + " x " + this.height; }

		//Available resolutions
		public static Resolution[] resolutions;
		public static void InitializeResolutions()
		{
			resolutions = new Resolution[5];
			resolutions[0] = new Resolution(1152, 648);
			resolutions[1] = new Resolution(1280, 720);
			resolutions[2] = new Resolution(1366, 768);
			resolutions[3] = new Resolution(1600, 900);
			resolutions[4] = new Resolution(1900, 1080);
		}

	}

	[System.Serializable] public class Settings
	{
		[XmlAttribute("resolution")] public int resolutionChoice;
		[XmlAttribute("fullscreen")] public bool isFullscreen;
		[XmlArray("inputs")] public string[] inputs;
		[XmlAttribute("music_volume")] public float musicVolume;
		[XmlAttribute("sfx_volume")] public float sfxVolume;

		//---------------------------------Methods-------------------------------
		/// <summary>
		/// Reads the settings file and returns it as a Settings object. If there is no settings file, returns null
		/// </summary>
		public static Settings ReadSettings()
		{
			if (File.Exists(Application.persistentDataPath + "/settings"))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(Settings));
				FileStream stream = new FileStream(Application.persistentDataPath + "/settings", FileMode.Open);
				return ((Settings)serializer.Deserialize(stream));
			}
			else //If there isn't yet a settings file, return null
				return null;
		}

		/// <summary>
		/// Writes the given Settings object out to the settings file
		/// </summary>
		public void WriteSettings()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(Settings));
			FileStream stream = new FileStream(Application.persistentDataPath + "/settings", FileMode.Create);
			serializer.Serialize(stream, this); //Write all of the settings data to the file
			stream.Close();
		}
		public Resolution GetResolution() { return Resolution.resolutions[resolutionChoice]; }

		public override string ToString()
		{
			return "Resolution: " + Resolution.resolutions[resolutionChoice].ToString() +
					"\nFullscreen? " + isFullscreen +
					"\nMusic Volume: " + musicVolume.ToString() +
					"\nSFX Volume: " + sfxVolume.ToString();
		}

		//-----------------------Constructors-------------------------------
		public Settings()
		{
			resolutionChoice = 4;
			isFullscreen = true;
			musicVolume = 0.5f;
			sfxVolume = 0.5f;

			Resolution.InitializeResolutions();
		}
	}
	[System.Serializable] class AnimationImage : System.IComparable<AnimationImage>
	{
		public Sprite sprite;
		public Color color;
		public Vector3 position;
		public float delayTime;
		public AnimationClip animation;
		[System.NonSerialized] public Image image;

		//Operator overloads for sorting
		/// <summary> Compares the delayTimes of the two IntroImages. If the source is greater, returns 1, if less, returns -1, and if the same, returns 0 </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(AnimationImage other) { if (this.delayTime > other.delayTime) return 1; else if (this.delayTime < other.delayTime) return -1; else return 0; }
	}

	#region Misc Methods

	/// <summary> Class containing various helper methods for the game. </summary>
	public static class RadHare
	{
		public static void BubbleSort<T>(List<T> list) where T : System.IComparable<T>
		{
			bool sorted = false;
			T temp;

			for (int i = 0; !sorted && i < list.Count; i++)
			{
				sorted = true;
				for (int j = i; j < list.Count - 1; j++)
				{
					if (list[j].CompareTo(list[j + 1]) > 0) //If left value is bigger than right value, swap
					{
						temp = list[j];
						list[j] = list[j + 1];
						list[j + 1] = temp;
						sorted = false;
					}
				}
			}
		}

		/// <summary> Converts the timer in seconds to a string in the form "MM:SS" </summary>
		/// <param name="timer"></param>
		/// <returns></returns>
		public static string ConvertToTimeString(int timer)
		{
			string text = "";
			if (timer / 60 < 10)
				text += "0";
			text += (timer / 60) + ":";
			if (timer % 60 < 10)
				text += "0";
			text += (timer % 60);

			return text;
		}

		/// <summary> Converts the timer in seconds to a string in the form "MM:SS:mm" </summary>
		public static string ConvertToTimeString(double timer)
		{
			int seconds = (int)timer;

			string text = "";
			if (seconds / 60 < 10)
				text += "0";
			text += (seconds / 60) + ":";
			if (seconds % 60 < 10)
				text += "0";
			text += (seconds % 60) + ":";
			if (timer - seconds < 0.1f)
				text += "0";
			text += (100.0f * Math.Round((float)(timer - seconds), 2)).ToString();

			return text;
		}

	}
	#endregion
}
