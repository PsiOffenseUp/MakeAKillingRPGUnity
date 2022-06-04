using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#region TextEffect Metadata class and enum
public class TimeSpan
{
	public int start { get; private set; }
	public int end { get; private set; }
	public TimeSpan(int start, int end) { this.start = start; this.end = end; }
}

#region Text Effects
public enum TextEffectType { WAVY, RAINBOW, SPIN, MAX}

//Base class for TextEffects that occur over some time span
public abstract class TextEffect
{
	public TextEffectType type { get; private set; }
	public TimeSpan span { get; private set; }

	protected TextEffect(TextEffectType type, int start, int end) { this.type = type; this.span = new TimeSpan(start, end); }
}

#endregion

#region Triggers
public enum TriggerType { WAIT, CHANGE_SPEED, MAX }
//Base class for text effects that are events and activate at one static time
public abstract class TextTrigger
{
	public TriggerType type { get; private set; }
	public TextTrigger(TriggerType type) { this.type = type;}
}

//Effect tag for waiting some number of seconds before continuing text
public class WaitEffect : TextTrigger
{
	public float waitTime { get; private set; }

	public WaitEffect() : base(TriggerType.WAIT) { this.waitTime = 1.0f; }
	public WaitEffect(float wait_time) : base(TriggerType.WAIT) { this.waitTime = wait_time; }
}
#endregion

public class TextEffectsMetadata
{
	TextEffect[][] textEffects; //An array that holds lists of the times at which effects are active
	Dictionary<TriggerType, Dictionary<int, TextTrigger>> textTriggers; //TODO Rewrite this for faster lookup with [][]
	readonly int maxEffects;
	readonly int maxTriggers;

	/// <summary> Sets all the new effects from the new text mesh pro object </summary>
	public void SetNewEffects(TMP_TextInfo text_info)
    {
		int i;
		string linkId;
		float tempFloat;
		int tempInt;

		//Set all of the Lists to be empty
		//TODO: Finish clearing textEffects
		for (i = 0; i < maxTriggers; i++)
			textTriggers[(TriggerType)i].Clear();

		//Go through all of the custom text effects from our text info and add them to our effect and trigger lists
		for(i = 0; i < text_info.linkCount; i++)
        {
			linkId = text_info.linkInfo[i].GetLinkID();
			if(linkId.StartsWith("wait")) //Wait before typing
            {
				float.TryParse(linkId.Split('_')[1], out tempFloat);
				textTriggers[TriggerType.WAIT][text_info.linkInfo[i].linkTextfirstCharacterIndex] = new WaitEffect(tempFloat);
            }
			else if(linkId.StartsWith("chspd")) //Change speed
            {

            }
        }
    }

	/// <summary>
	/// Tells if the effects is active at the given index
	/// </summary>
	/// <param name="type"></param>
	/// <param name="index"></param>
	/// <returns></returns>
	public TextEffect GetEffect(TextEffectType type, int index)
    {
		return textEffects[(int)type][index];
    }

	public TextTrigger GetTrigger(TriggerType type, int index)
    {
		try { return textTriggers[type][index]; }
		catch { return null; }
    }

	public TextEffectsMetadata()
    {
		int i;
		//Initialize the effect data structure and variables
		maxEffects = (int)TextEffectType.MAX;

		//Do the same for triggers
		maxTriggers = (int)TriggerType.MAX;
		textTriggers = new Dictionary<TriggerType, Dictionary<int, TextTrigger>>();
		for (i = 0; i < maxTriggers; i++)
			textTriggers[(TriggerType)i] = new Dictionary<int, TextTrigger>();
	}
}

#endregion

public class DialogueManager : MonoBehaviour
{

	//----------------------Member variables--------------------
	[SerializeField] TMP_Text bodyText, nameText;
	[SerializeField] GameObject textBoxParent;
	[SerializeField] AudioClip talkingSoundEffect;

	Queue<BoogalooGame.Dialogue> queue; //Queue of dialogues being ready to display
	int currentCharacter, i;
	float invSpeed;
	float targetTime, currentTime;
	public bool isActive { get; private set; }
	bool isTyping; //Bools for whether the dialogue is being displayed, the text is typing
	BoogalooGame.Dialogue currentDialogue;
	TextEffectsMetadata tEM;

	const float defaultSpeed = 0.07f; //How many seconds should be between typing letters by default

    //--------------------------Methods-----------------------
    #region Unity overrides
    // Start is called before the first frame update
    void Awake()
    {
		queue = new Queue<BoogalooGame.Dialogue>();
		isActive = isTyping = false;
		currentCharacter = 0;
		currentTime = targetTime = 0.0f;
		currentDialogue = null;
		textBoxParent.SetActive(false);
		tEM = new TextEffectsMetadata();
		invSpeed = defaultSpeed;
	}

    // Update is called once per frame
    void Update()
    {
		if (isActive)
		{
			if (ControlManager.JumpPressed()) //If the confirm button is pressed
			{
				if (isTyping) //If button is pressed while text is typing, skip to the end of the text
				{
					bodyText.maxVisibleCharacters = bodyText.textInfo.characterCount;
					currentCharacter = bodyText.textInfo.characterCount - 1;
					isTyping = false;
					//marker.SetActive(true);
				}
				else
				{
					try { GetNextDialogue(); } 
					catch { currentDialogue = null; } //If nothing is in the queue, set the current dialogue to null

					isTyping = true;
				}
			}
			else if (isTyping) //Else type the text normally
            {
				currentTime += Time.deltaTime;

				//If we've overshot the time for the target character, advance characters until we reach it
				while (targetTime < currentTime)
                {
					currentCharacter++;
					FindNextTargetTime();
				}

				bodyText.maxVisibleCharacters = currentCharacter;
				isTyping = currentCharacter <= bodyText.textInfo.characterCount; //Update whether we are typing or not
			}
		}

		if (currentDialogue == null && isActive)
			Deactivate();
    }
    #endregion

    #region Methods for other class to call
    public void Activate() 
	{
		isActive = true;
		isTyping = true;
		textBoxParent.SetActive(true);

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
		textBoxParent.SetActive(false);
		currentDialogue = null;
		GameplayManager.Unpause(); //Unpause the game when we are out of dialogue
	}
	public void EnqueueDialogue(BoogalooGame.Dialogue dialogue) 
	{ 
		queue.Enqueue(dialogue);
		if(!isActive) 
			Activate(); 
	}
	public void EnqueueDialogue(BoogalooGame.Dialogue[] dialogue) 
	{
		for (int i = 0; i < dialogue.Length; i++)
			queue.Enqueue(dialogue[i]);

		if (!isActive)
			Activate();
	}
	public void ChangeSpeed(float new_speed) { invSpeed = 1.0f / new_speed; }
	public void ChangeSpeed(int new_speed) { invSpeed = 1.0f / new_speed; }
	#endregion

	public void GetNextDialogue()
	{
		currentDialogue = queue.Dequeue(); //Try to get the next dialogue

		if (currentDialogue != null)
		{
			bodyText.text = currentDialogue.plainText;
			nameText.text = currentDialogue.name;
			bodyText.maxVisibleCharacters = 0;
			bodyText.ForceMeshUpdate(); //Update the fields based on the new text
			tEM.SetNewEffects(bodyText.textInfo);
			targetTime = 0.0f;
			FindNextTargetTime();
		}
		currentCharacter = 0;
	}

	public void FindNextTargetTime()
    {
		targetTime += invSpeed; //By default, time between characters will be invSpeed away

		//Check if there is a pause/wait at the current character

		WaitEffect waitEffect = (WaitEffect)tEM.GetTrigger(TriggerType.WAIT, currentCharacter);
		if (waitEffect != null)
			targetTime += waitEffect.waitTime;
    }

	#region Text Effects

	void DisplayText()
    {
		bodyText.ForceMeshUpdate(); //Force the text mesh to be updated for effects
		TMP_TextInfo textInfo = bodyText.textInfo;
    }




    #endregion

}
