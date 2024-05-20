using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BoogalooGame;

public class NPC : InteractableObject
{
    STATE storedState; //State before they started talking
    static readonly Vector3 talkOffset = new Vector3(2.5f, 1.0f, -11.0f);
    static readonly Vector3 talkFocusOffset = new Vector3(3.5f, -2.3f, 0.0f);

    protected override void Awake()
    {
        availableConversations = new Dictionary<string, Conversation>();

        LoadConversation("testing/sample.txt", "sample"); //DEBUG

        base.Awake();
    }

    protected override void Start()
    {
    }

    #region Dialogue-Related Members
    Dictionary<string, Conversation> availableConversations; //Conversations available to be triggered

    //Loads a Conversation from the given file with the given conersation_name as a key to look up later
    protected void LoadConversation(string file_name, string conversation_name)
    {
        availableConversations[conversation_name] = DialogueReader.ReadDialogueFromFile("Assets/Resources/text/" + file_name, conversation_name);
    }

    //Starts the conversation with the given key. Conversation must have been loaded first using LoadConversation
    protected void MakeConversationActive(string conversation_name)
    {
        try
        {
            GameplayManager.dialogueManager.AddConversation(availableConversations[conversation_name]);
        }
        catch
        {
            Debug.Log("Could not find the conversation: " + conversation_name);
        }
    }

    #endregion

    #region Events related to talking
    /// <summary>
    /// Called when the player first initiates starting talking with this NPC. Use this to queue up proper dialogue
    /// </summary>
    protected virtual void OnStartTalk()
    {
        //If already talking, don't worry about it
        if (state.current == STATE.TALKING)
            return;

        storedState = state.current;
        state.Transition(STATE.TALKING);
        GameplayManager.dialogueManager.SetTalkingNPC(this); //Mark this NPC as the one talking

        //Update where the camera is looking
        GameplayManager.customCam.SetFocus(this.gameObject);
        GameplayManager.customCam.SetFocusOffset(talkFocusOffset);
        GameplayManager.customCam.SetOffset(talkOffset);
        GameplayManager.customCam.ImmediatelyGoToOffet();

        MakeConversationActive("sample"); //DEBUG
    }

    public virtual void OnEndTalk()
    {
        state.Transition(storedState, 4); //Restore the state from before they were talking

        //Update where the camera is looking
        GameplayManager.customCam.SetFocus(GameplayManager.player.gameObject);
        GameplayManager.customCam.UseDefaultOffsets();
        GameplayManager.customCam.ImmediatelyGoToOffet();
    }
    #endregion

    #region Interaction Overrides


    protected override void OnPlayerInteract()
    {
        base.OnPlayerInteract();
        GameplayManager.player.OnTalk();
        OnStartTalk();
    }
    protected override void OnPlayerFinishInteract()
    {
        base.OnPlayerFinishInteract();
        OnEndTalk();
    }

    #endregion
}
