using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace BoogalooGame
{
	#region Subdialogue tokens
	public class SubDialogue { }

	public class DialogueText : SubDialogue { public string text { get; private set; } public DialogueText(string text) { this.text = text; } } //The string that this object holds.
	public class SpriteTag : SubDialogue { public string index; public SpriteTag(string index) { this.index = index; } } //The string that this object holds.

	public class Tag : SubDialogue { public bool openTag; public Tag(bool is_open) { this.openTag = is_open; } } //Whether this object is an opening tag or a closing tag.
	public class BoldTag : Tag { public BoldTag(bool is_open) : base(is_open) { } }
	public class ItalicTag : Tag { public ItalicTag(bool is_open) : base(is_open) { } }
	public class ColorTag : Tag { public string colorString; public ColorTag(bool is_open) : base(is_open) { } public ColorTag(bool is_open, string color_string) : this(is_open) { colorString = color_string; } }
	public class SizeTag : Tag { public string size; public SizeTag(bool is_open) : base(is_open) { } public SizeTag(bool is_open, string size) : this(is_open) { this.size = size; } }
	public class SpeedTag : Tag { public int speed; public SpeedTag(bool is_open) : base(is_open) { } public SpeedTag(bool is_open, int speed) : this(is_open) { this.speed = speed; } }
	#endregion

	///Dialogue class for each message box
	[System.Serializable]
	public class Dialogue
	{
		public string plainText;
		public int speed;
		public Sprite portrait;
		public enum FontSize { SMALL, NORMAL, BIG };
		public FontSize defaultFontSize;


		bool usingColor, isBold, isItalic, diffSize;
		public bool isDone { get; private set; }
		List<SubDialogue> subdialogues;
		string currentString;
		int currentSubdialogue;
		int currentPosition;

		public int GetFontSize()
		{
			switch (defaultFontSize)
			{
				case FontSize.SMALL:
					return 48;
				case FontSize.NORMAL:
					return 64;
				case FontSize.BIG:
					return 80;
				default:
					return 64;
			}
		}

		public Dialogue() { plainText = "..."; speed = 1; this.defaultFontSize = FontSize.NORMAL; }
		public Dialogue(string text) { this.plainText = text; speed = 1; this.defaultFontSize = FontSize.NORMAL; }
		public Dialogue(string text, int speed) { this.plainText = text; this.speed = speed; this.defaultFontSize = FontSize.NORMAL; }
		public Dialogue(string text, int speed, Sprite portrait_sprite, FontSize font_size) { this.plainText = text; this.speed = speed; this.portrait = portrait_sprite; this.defaultFontSize = font_size; }

		/// <summary>
		/// Parses the Dialogue's plain text string, separating it so it correctly splits on color changes. Use GetString to get a character with color changing included.
		/// </summary>
		public void ParsePlainText()
		{
			//Initalize variables for after the parsing is done
			usingColor = isBold = isItalic = isDone = diffSize = false;
			currentString = "";
			currentSubdialogue = currentPosition = 0;

			//Initalize variables before parsing
			subdialogues = new List<SubDialogue>();
			string tempText = ""; //Holds the current text or tag being read
			bool isOpenTag; //Whether the tag being read is an open or closing tag

			//Go through all of the plain text and attempt to parse any color, bold, or italic tag tokens out
			for (int i = 0; i < plainText.Length; i++)
			{
				if (plainText[i] != '<') //If this is not a tag, keep reading
					tempText += plainText[i];
				else //If we are about to start reading a tag
				{
					if (plainText[i + 1] == '/') //Check the next character to see if this is a closing tag
					{
						isOpenTag = false;
						i++; //Go to the next character
					}
					else
						isOpenTag = true;

					//If there was a string being read, add it to the list
					if (tempText != "")
						subdialogues.Add(new DialogueText(tempText));
					tempText = "";
					i++; //Go to the next character to being reading the tag name

					//Read the rest of the tag
					while (plainText[i] != '>') //While we are not at the end of tag, read into tempText
					{
						tempText += plainText[i];
						i++;
					}

					//Check what the tag was
					if (tempText == "b") //If this is a bold tag, add it
						subdialogues.Add(new BoldTag(isOpenTag));
					else if (tempText == "i") //If this is an italic tag, add it
						subdialogues.Add(new ItalicTag(isOpenTag));
					else if (tempText.StartsWith("color")) //If this is a color tag
					{
						if (isOpenTag)
							subdialogues.Add(new ColorTag(true, tempText.Substring(6, 7))); //Add everything after the "color=" part
						else
							subdialogues.Add(new ColorTag(false, ""));
					}
					else if (tempText.StartsWith("size")) //If this is a font size tag
					{
						if (isOpenTag)
							subdialogues.Add(new SizeTag(true, tempText.Substring(5, tempText.Length - 5))); //Add everything after the "size=" part
						else
							subdialogues.Add(new SizeTag(false, ""));
					}
					else if (tempText.StartsWith("speed"))
					{
						if (isOpenTag)
							subdialogues.Add(new SpeedTag(true, System.Int32.Parse(tempText.Substring(6, tempText.Length - 6))));
						else
							subdialogues.Add(new SpeedTag(false, 0));
					}
					else if (tempText.StartsWith("sprite")) //If this is a sprite tag
						subdialogues.Add(new SpriteTag(tempText.Substring(7, tempText.Length - 7)));

					tempText = ""; //Reset tempText
				}
			}

			//Add the last string if there was one
			if (tempText != "")
				subdialogues.Add(new DialogueText(tempText));

		}

		/// <summary>
		/// Gets the next string for the next character with the correct tags placed in
		/// </summary>
		/// <returns></returns>
		public string GetNextString()
		{
			if (currentSubdialogue >= subdialogues.Count) //If we're at the end of the subdialogue list, set a flag saying so
			{
				isDone = true;
				return currentString;
			}

			while (!(subdialogues[currentSubdialogue] is DialogueText)) //While we're not at a DialogueText to read from
			{
				currentPosition = 0;

				//If we encounter any tags while moving over, set attributes accordingly
				if (subdialogues[currentSubdialogue] is BoldTag)
				{
					isBold = ((BoldTag)subdialogues[currentSubdialogue]).openTag;

					if (!isBold) //If this is an ending tag, add it to the currentString
						currentString += "</b>";
					else //Otherwise, add in an opening tag
						currentString += "<b>";
				}
				else if (subdialogues[currentSubdialogue] is ItalicTag)
				{
					isItalic = ((ItalicTag)subdialogues[currentSubdialogue]).openTag;

					if (!isItalic)
						currentString += "</i>";
					else
						currentString += "<i>";
				}
				else if (subdialogues[currentSubdialogue] is ColorTag)
				{
					usingColor = ((ColorTag)subdialogues[currentSubdialogue]).openTag;

					if (!usingColor)
						currentString += "</color>";
					else
						currentString += "<color=" + ((ColorTag)subdialogues[currentSubdialogue]).colorString + ">";
				}
				else if (subdialogues[currentSubdialogue] is SizeTag)
				{
					diffSize = ((SizeTag)subdialogues[currentSubdialogue]).openTag;

					if (!diffSize)
						currentString += "</size>";
					else
						currentString += "<size=" + ((SizeTag)subdialogues[currentSubdialogue]).size + ">";
				}
				else if (subdialogues[currentSubdialogue] is SpeedTag)
				{
					if (((SpeedTag)subdialogues[currentSubdialogue]).openTag)
						GameplayManager.dialogueManager.ChangeSpeed(((SpeedTag)subdialogues[currentSubdialogue]).speed);
					else
						GameplayManager.dialogueManager.ChangeSpeed(this.speed);
				}
				else if (subdialogues[currentSubdialogue] is SpriteTag)
					currentString += "</sprite=" + ((SpriteTag)subdialogues[currentSubdialogue]).index + ">";

				//Go to the next subdialogue
				currentSubdialogue++;

				if (currentSubdialogue >= subdialogues.Count) //If we're at the end of the subdialogue list, set a flag saying so
				{
					isDone = true;
					return currentString;
				}
			}

			//If we are on text, start copying it over
			//Add the next character to the current string
			currentString += ((DialogueText)subdialogues[currentSubdialogue]).text[currentPosition];
			currentPosition++;

			if (currentPosition >= ((DialogueText)subdialogues[currentSubdialogue]).text.Length) //If this is the end of this text block, go to the next subdialogue choice
				currentSubdialogue++;

			//Add on any closing tags that we will temporarily need
			string tempClosingTags = "";
			if (isBold)
				tempClosingTags += "</b>";
			if (isItalic)
				tempClosingTags += "</i>";
			if (usingColor)
				tempClosingTags += "</color>";
			if (diffSize)
				tempClosingTags += "</size>";

			return currentString + tempClosingTags;
		}

	}

	[System.Serializable]
	public class CutsceneEvent
	{
		public Dialogue[] dialogue;
		public Animation animation;
		public float hSpeed, vSpeed;
		public bool waitForDialogueClear;
		public int moveFrames;

		public CutsceneEvent(float hspeed, float vspeed, int time)
		{
			hSpeed = hspeed;
			vSpeed = vspeed;
			moveFrames = time;
		}
	}
}
