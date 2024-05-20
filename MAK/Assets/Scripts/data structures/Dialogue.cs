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
		public string plainText { private set; get; }
		public string name { private set; get; }
		public bool useName { private set; get; }
		public int nameColor { private set; get; }
		public int spriteIndex { private set; get; }

		public Dialogue() { plainText = "..."; name = "Unknown"; useName = false; nameColor = 0; }
		public Dialogue(string text, string name, int name_color = 0, int sprite_index = 0) 
		{
			this.plainText = text; 
			this.name = name; 
			useName = true;
			this.nameColor = name_color;
			this.spriteIndex = sprite_index;
		}

		public Dialogue(string text)
        {
			this.plainText = text;
			this.name = "";
			useName = false;
			this.nameColor = 0;
			this.spriteIndex = 0;
		}

	}

	[System.Serializable]
	public class Conversation
    {
		public string name { private set; get; }
		public List<Dialogue> dialogues;
		public void AddDialogue(Dialogue dialogue) { dialogues.Add(dialogue); }

		public Conversation() { dialogues = new List<Dialogue>(); name = "unknown"; }
		public Conversation(string name) { dialogues = new List<Dialogue>(); this.name = name; }
    }
}
