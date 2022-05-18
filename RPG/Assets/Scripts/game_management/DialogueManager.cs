using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{

	//----------------------Member variables--------------------
	[SerializeField] TMPro.TextMeshProUGUI currentText;
	[SerializeField] AudioClip talkingSoundEffect;

	Queue<BoogalooGame.Dialogue> queue; //Queue of dialogues being ready to display
	int currentCharacter, i, readCharacters;
	ulong speed;
	public bool isActive { get; private set; }
	bool isTyping; //Bools for whether the dialogue is being displayed, the text is typing
	BoogalooGame.Dialogue currentDialogue;

	//--------------------------Methods-----------------------
	// Start is called before the first frame update
	void Awake()
    {
		queue = new Queue<BoogalooGame.Dialogue>();
		isActive = isTyping = false;
		currentCharacter = readCharacters = 0;
		currentDialogue = null;
    }

    // Update is called once per frame
    void Update()
    {
		if (isActive)
		{
			if (ControlManager.AttackPressed()) //If the confirm button is pressed
			{
				if (isTyping) //If button is pressed while text is typing, skip to the end of the text
				{
					currentText.text = currentDialogue.plainText;
					isTyping = false;
					//marker.SetActive(true);
				}
				else
				{
					try 
					{
						GetNextDialogue();
					} 
					catch { currentDialogue = null; } //If nothing is in the queue, set the current dialogue to null

					isTyping = true;
				}
			}
		}
		else
		{
			try
			{
				if (queue.Count != 0)
					Activate();
			}
			catch { }
		}

		if (currentDialogue == null && isActive)
			Deactivate();

		if (isTyping)
		{
			if (GameplayManager.frameTimer % speed == 0)
			{
				if (GameplayManager.frameTimer % (2*(ulong)currentDialogue.speed) == 0)
					GameplayManager.audioPlayer.PlaySFX(talkingSoundEffect); //Play a sound effect for talking

				currentCharacter++; //Go to the next character
			}

			if (readCharacters < currentCharacter)
			{
				readCharacters++;
				currentText.text = currentDialogue.GetNextString(); //Update the string being displayed

				if (currentDialogue.isDone)
					isTyping = false;
			}
		}
    }

	public void Activate() 
	{
		isActive = true;
		isTyping = true;
		currentText.text = "";
		try 
		{
			GetNextDialogue();
		}
		catch { currentDialogue = null; }
	}
	public void Deactivate() 
	{ 
		isActive = false;
		isTyping = false;
		currentDialogue = null;
		GameplayManager.Unpause(); //Unpause the game when we are out of dialogue
	}
	public void EnqueueDialogue(BoogalooGame.Dialogue dialogue) { queue.Enqueue(dialogue); }
	public void EnqueueDialogue(BoogalooGame.Dialogue[] dialogue) 
	{
		for (int i = 0; i < dialogue.Length; i++)
			queue.Enqueue(dialogue[i]); 
	}

	public void GetNextDialogue()
	{
		currentDialogue = queue.Dequeue(); //Try to get the next dialogue
		currentText.fontSize = currentDialogue.GetFontSize(); //Copy over the font size
		currentDialogue.ParsePlainText(); //Parse the dialogue for any tokens
		speed = (ulong)currentDialogue.speed;

		currentCharacter = 0;
		readCharacters = 0;
	}

	public void ChangeSpeed(int new_speed) { speed = (ulong)new_speed; }
	public void ChangeSpeed(ulong new_speed) { speed = new_speed; }

	//DEBUG Write some functions to queue up dialogue
}
