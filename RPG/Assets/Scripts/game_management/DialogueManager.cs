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

public class WaveEffect : TextEffect
{
	public float waviness { get; private set; }
	public WaveEffect(int start, int end, float waviness) : base(TextEffectType.WAVY, start, end)
    {
		this.waviness = waviness;
    }
}

public class SpinEffect : TextEffect
{
	public SpinEffect(int start, int end) : base(TextEffectType.SPIN, start, end) { }
}

#endregion

#region Triggers
public enum TriggerType { WAIT, CHANGE_SPEED, RESET_SPEED, MAX }
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

public class ChangeSpeedTrigger : TextTrigger
{
	public float newSpeed { get; private set; }

	public ChangeSpeedTrigger() : base (TriggerType.CHANGE_SPEED) { this.newSpeed = 4.0f; }
	public ChangeSpeedTrigger(float new_speed) : base (TriggerType.CHANGE_SPEED) { this.newSpeed = new_speed; }
}

public class ResetSpeedTrigger : TextTrigger
{
	public ResetSpeedTrigger() : base(TriggerType.RESET_SPEED) { }
}
#endregion

public class TextEffectsMetadata
{
	TextEffect[,] textEffects; //An array that holds lists of the times at which effects are active
	Dictionary<TriggerType, Dictionary<int, TextTrigger>> textTriggers; //TODO Rewrite this for faster lookup with [][]
	readonly int maxEffects;
	readonly int maxTriggers;

	/// <summary> Sets all the new effects from the new text mesh pro object </summary>
	public void SetNewEffects(TMP_TextInfo text_info)
    {
		int i, j, startChar, endChar;
		string linkId;
		float tempFloat; //For reading float values from tags
		int tempInt, castEffect;

		//Set all of the Lists to be empty
		//TODO: Finish clearing textEffects
		for (i = 0; i < maxTriggers; i++)
			textTriggers[(TriggerType)i].Clear();

		//Reallocate the text effects array
		textEffects = new TextEffect[maxEffects, text_info.characterCount];

		//Go through all of the custom text effects from our text info and add them to our effect and trigger lists
		for(i = 0; i < text_info.linkCount; i++)
        {
			linkId = text_info.linkInfo[i].GetLinkID();
			startChar = text_info.linkInfo[i].linkTextfirstCharacterIndex;
			endChar = startChar + text_info.linkInfo[i].linkTextLength;
			if (linkId.StartsWith("wait")) //Wait before typing
            {
				float.TryParse(linkId.Split('_')[1], out tempFloat);
				textTriggers[TriggerType.WAIT][startChar] = new WaitEffect(tempFloat);
            }
			else if(linkId.StartsWith("chspd")) //Change speed
            {
				float.TryParse(linkId.Split('_')[1], out tempFloat);
				textTriggers[TriggerType.CHANGE_SPEED][startChar] = new ChangeSpeedTrigger(tempFloat);
			}
			else if(linkId.StartsWith("resspd"))
            {
				textTriggers[TriggerType.RESET_SPEED][startChar] = new ResetSpeedTrigger();
			}
			else if (linkId.StartsWith("wave")) //Wave effect
            {
				float.TryParse(linkId.Split('_')[1], out tempFloat);
				WaveEffect we = new WaveEffect(startChar, endChar, tempFloat);
				castEffect = (int)TextEffectType.WAVY;
				for (j = startChar; j < endChar; j++)
					textEffects[castEffect, j] = we;

			}
			else if(linkId.StartsWith("spin"))
            {
				SpinEffect se = new SpinEffect(startChar, endChar);
				castEffect = (int)TextEffectType.SPIN;
				for (j = startChar; j < endChar; j++)
					textEffects[castEffect, j] = se;
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
		return textEffects[(int)type, index];
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
	NPC talkNPC;

	//########### Text effect constants ###########
	const float defaultSpeed = 0.058f; //How many seconds should be between typing letters by default
	const float timeMult = 2.6f;
	readonly Vector2 waveAmp = new Vector2(0.05f, 2.2f); //Base amplitude for x and y on wavy effect

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
			if (ControlManager.AttackPressed()) //If the confirm button is pressed
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
				}
			}
			else if (isTyping) //Else type the text normally
            {
				currentTime += Time.deltaTime;

				//If we've overshot the time for the target character, advance characters until we reach it
				while (targetTime < currentTime && currentCharacter < bodyText.textInfo.characterCount)
                {
					currentCharacter++;
					FindNextTargetTime();
				}

				//If the text was advanced by some amount, play a sound effect
				if (bodyText.maxVisibleCharacters != currentCharacter)
					GameplayManager.audioPlayer.PlaySFX(talkingSoundEffect);

				bodyText.maxVisibleCharacters = currentCharacter;
				isTyping = currentCharacter < bodyText.textInfo.characterCount; //Update whether we are typing or not
			}
			ApplyTextEffects(); //Apply any active text effects
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
		GameplayManager.player.EndTalk(); //Let the player know dialogue is done
		FinishTalkingNPC(); //Let the NPC know the talk is done
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
	public void ResetSpeed() { invSpeed = defaultSpeed; }
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
			isTyping = true;
			currentCharacter = 0;
			currentTime = targetTime = 0.0f;
			ResetSpeed(); //Reset talking speed
			FindNextTargetTime();
		}
	}

	public void FindNextTargetTime()
    {
		TextTrigger tempTrigger;

		//Check if the speed needs to be updated
		tempTrigger = tEM.GetTrigger(TriggerType.CHANGE_SPEED, currentCharacter);
		if (tempTrigger != null)
			ChangeSpeed(((ChangeSpeedTrigger)tempTrigger).newSpeed);
		else //Check for speed reset
        {
			tempTrigger = tEM.GetTrigger(TriggerType.RESET_SPEED, currentCharacter);
			if (tempTrigger != null)
				ResetSpeed();
		}

		targetTime += invSpeed; //By default, time between characters will be invSpeed away

		//Check if there is a pause/wait at the current character
		tempTrigger = tEM.GetTrigger(TriggerType.WAIT, currentCharacter);
		if (tempTrigger != null)
			targetTime += ((WaitEffect)tempTrigger).waitTime;

    }

	#region Text Effects

	void ApplyTextEffects()
    {
		bodyText.ForceMeshUpdate(); //Force the text mesh to be updated for effects
		Mesh mesh = bodyText.mesh;
		Vector3[] verts = mesh.vertices;
		TMP_TextInfo textInfo = bodyText.textInfo;
		TMP_CharacterInfo charInfo;

		TextEffect tempEffect; //Temp reference to relevant effect
		float tempFloat; //Temp variables to grab values without needing to cast as often
		int tempInt, startVertIndex;
		Vector3 tempVector;

		//Check for text effects and apply up to the current character
		for (int i = 0; i < currentCharacter && i < textInfo.characterCount; i++)
        {
			charInfo = textInfo.characterInfo[i];

			//TODO: Find out why the first char's vert index is randomly in the middle? This is a jank fix
			if (charInfo.vertexIndex == 0 && i != 0) 
				continue;

			//****** Check what effects apply to each character ******

			//Wave effect
			tempEffect = tEM.GetEffect(TextEffectType.WAVY, i);
			if (tempEffect != null)
            {
				tempFloat = ((WaveEffect)tempEffect).waviness;
				tempVector = tempFloat*waveAmp; //Get waviness in both directions
				startVertIndex = charInfo.vertexIndex;

				//Debug.Log("i =" + i + "Char# " + charInfo.index + " with Start vert index = " + startVertIndex);
				//Apply to all 4 vertices
				tempFloat = Mathf.Sin(timeMult * Time.time + mesh.vertices[startVertIndex].x);
				verts[startVertIndex] += new Vector3(tempVector.x * tempFloat, tempVector.y * tempFloat, 0);

				tempFloat = Mathf.Sin(timeMult * Time.time + mesh.vertices[startVertIndex + 1].x);
				verts[startVertIndex + 1] += new Vector3(tempVector.x * tempFloat, tempVector.y * tempFloat, 0);

				tempFloat = Mathf.Sin(timeMult * Time.time + mesh.vertices[startVertIndex + 2].x);
				verts[startVertIndex + 2] += new Vector3(tempVector.x * tempFloat, tempVector.y * tempFloat, 0);

				tempFloat = Mathf.Sin(timeMult * Time.time + mesh.vertices[startVertIndex + 3].x);
				verts[startVertIndex + 3] += new Vector3(tempVector.x * tempFloat, tempVector.y * tempFloat, 0);
			}

			//Spin effect
			tempEffect = tEM.GetEffect(TextEffectType.SPIN, i);
			if (tempEffect != null)
			{
				startVertIndex = charInfo.vertexIndex;
				tempVector = new Vector3(5.8f*Mathf.Cos(timeMult *Time.time + mesh.vertices[startVertIndex].x),
					7.0f*Mathf.Sin(timeMult * Time.time + mesh.vertices[startVertIndex].x), 0);

				verts[startVertIndex] += tempVector;
				verts[startVertIndex + 1] += tempVector;
				verts[startVertIndex + 2] += tempVector;
				verts[startVertIndex + 3] += tempVector;
			}
		}

		//Update the mesh and vertices
		mesh.vertices = verts;
		bodyText.canvasRenderer.SetMesh(mesh);
    }

	#endregion

	#region Talking Events

	public void SetTalkingNPC(NPC active_npc)
    {
		talkNPC = active_npc;
    }

	void FinishTalkingNPC()
    {
		talkNPC.OnEndTalk();
    }

	#endregion

}
