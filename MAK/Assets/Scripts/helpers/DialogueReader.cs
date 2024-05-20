using BoogalooGame;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//Helper class designed to read dialogue for a character from a file
public static class DialogueReader
{
    #region ----------------- Internal Classes -----------------
    //Class for information about text Triggers for reading them from files and converting them to dialogue
    class TriggerInformation
    {
        public string symbol { get; protected set; } //Symbol for recognizing this trigger from the file
        public string translation { get; protected set; } //Text to translate symbol to for actual game text
        public int paramCount { get; protected set; } //How many parameters this command takes
        public bool isEffect { get; protected set; } //Whether the symbol is an effect or just a trigger (with no end tag)
        public bool isCustom { get; protected set; } //Whether the symbol is a custom link

        //Constructor
        public TriggerInformation(string sy, string tr, int pc, bool ef, bool ic = false)
        {
            symbol = sy;
            translation = tr;
            paramCount = pc;
            isEffect = ef;
            isCustom = ic;
        }
    }
    #endregion

    #region ----------------- Trigger Information Data Structures -----------------
    static readonly Dictionary<string, TriggerInformation> triggerDictionary = new Dictionary<string, TriggerInformation>()
    {
        ["wa"] = new TriggerInformation("wa", "wait", 1, false, true), //Wait
        ["co"] = new TriggerInformation("co", "color", 1, true), //Color
        ["si"] = new TriggerInformation("si", "size", 1, true), //Size
        ["sp"] = new TriggerInformation("sp", "spin", 0, true, true), //Spin
        ["wv"] = new TriggerInformation("wv", "wave", 1, true, true), //Wave
        ["cs"] = new TriggerInformation("cs", "chspd", 1, false, true), //Change Speed
        ["rs"] = new TriggerInformation("rs", "resspd", 0, false, true), //Reset Speed
        ["rb"] = new TriggerInformation("rb", "rainbow", 0, true, true), //Rainbow
        ["su"] = new TriggerInformation("su", "sup", 0, true, false), //Superscript
        ["sb"] = new TriggerInformation("sb", "sub", 0, true, false), //Subscript
        ["st"] = new TriggerInformation("st", "sprite", 1, true, false), //Sprite
        ["sn"] = new TriggerInformation("sn", "", 0, true, true), //Set Name (Special)
        ["cc"] = new TriggerInformation("cc", "", 1, true, true), //Change Name Color (Special)
        ["ss"] = new TriggerInformation("s", "", 1, true, true), //Set Sprite (Special)
        ["ed"] = new TriggerInformation("ed", "", 0, false, true), //End Dialogue (Special)
    };

    #endregion

    #region Tag Conversions
    //Converts the given trigger info into a start tag
    private static string GetAsDialogueTag(TriggerInformation trigger, List<string> arg_list = null)
    {
        string tag = "<"; //Start bracket

        //Custom tags using links and custom param delimiter
        if(trigger.isCustom)
        {
            //Name of the tag
            tag += "link=\"" + trigger.translation;
            //Arguments
            for (int i = 0; i < trigger.paramCount; i++)
                tag += "_" + arg_list[i];

            //Ending quote only if custom
            tag += "\"";
        }
        else
        {
            //Name and =
            tag += trigger.translation;
            if(trigger.paramCount > 0)
                tag += "=";
            for (int i = 0; i < trigger.paramCount; i++)
                tag += arg_list[i];
        }
    
        tag += ">"; //End bracket

        //If this is not an effect, but a trigger, needs an end tag immediately
        if(!trigger.isEffect)
            tag += GetEndTag(trigger);

        return tag;
    }

    //Converts the given trigger into an end tag
    private static string GetEndTag(TriggerInformation trigger)
    {
        //For customs, use link end tag, else use the translated symbol for Unity's built in tags
        return trigger.isCustom ? "</link>" : "</" + trigger.translation + ">";
    }
    #endregion

    #region ----------------- Dialogue Reading Methods -----------------
    //Reads all of the Dialogue from a file and 
    public static Conversation ReadDialogueFromFile(string file_name, string convo_name = "Conversation")
    {
        Conversation conversation = new Conversation(convo_name);
        StreamReader reader = new StreamReader(file_name); //Open the file

        //Variables for reading things
        string param = ""; //Used to hold params
        string talkName = "Unknown"; //Name of character currently talking
        string dialogueText = "";
        string triggerTag = "";
        int nameColor = 0;
        int spriteIndex = 0;
        bool isEndTag;
        List<string> parameters;

        //Parse through the whole file
        char currentChar;
        while(reader.Peek() >= 0) //While not at end of file
        {
            currentChar = (char)reader.Read(); //Read the next character
            if (currentChar == '<') //If the current character is the start of a tag, read the whole tag
            {
                triggerTag = "";
                currentChar = (char)reader.Read(); //Read the next character to figure out if this is a start or end tag
                isEndTag = currentChar == '/'; //Figure out whether this is an end tag
                if (!isEndTag)//If it's not an end tag, add the read char to the tag name and read the next char
                {
                    triggerTag += currentChar;
                    triggerTag += (char)reader.Read();
                }
                else //Read two chars
                {
                    triggerTag += (char)reader.Read();
                    triggerTag += (char)reader.Read();
                }

                //Read any parameters
                parameters = new List<string>();
                param = "";
                while (reader.Peek() != '>')
                {
                    currentChar = (char)reader.Read();
                    if (currentChar == ',') //If a comma is read, then add the param and get ready to start the next one
                    {
                        parameters.Add(param); //Add the param
                        param = "";
                    }
                    else
                        param += currentChar;
                }
                parameters.Add(param); //Add the last param
                currentChar = (char)reader.Read(); //Read the '>' char

                //Finally, generate the tag for the dialogue

                //Handle special tags that don't get added into the dialogue
                if(triggerTag.Equals("sn")) //Set the name of the character talking
                {
                    talkName = "";
                    //Read the name until the end tag is hit
                    while (reader.Peek() != '<')
                        talkName += (char)reader.Read();

                    //Read the rest of the </sn> end tag
                    for (int i = 0; i < 5; i++)
                        currentChar = (char)reader.Read(); 
                }
                else if (triggerTag.Equals("cc")) //Change color for the name
                {
                    //Try to parse the name color and fallback to 0 on error
                    if (!int.TryParse(parameters[0], out nameColor)) nameColor = 0;
                }
                else if(triggerTag.Equals("ss")) //Set sprite
                {
                    //Try to parse the sprite number and fallback to 0 on error
                    if (!int.TryParse(parameters[0], out spriteIndex)) spriteIndex = 0;
                }
                else if (triggerTag.Equals("ed")) //End Dialogue ends the current dialogue and should be added to the conversation
                {
                    //If the person has no name, use the Dialogue constructor for no name, else use normal one
                    conversation.AddDialogue((talkName.Length == 0) ?
                        new Dialogue(dialogueText) : new Dialogue(dialogueText, talkName, nameColor));
                    dialogueText = ""; //Reset dialogue text afetr this one has been added
                } 
                else
                    dialogueText += isEndTag ? 
                        GetEndTag(triggerDictionary[triggerTag]) : GetAsDialogueTag(triggerDictionary[triggerTag], parameters);
            }
            else if (currentChar == '\\') //Handle special characters like in-game line feeds
            {
                currentChar = (char)reader.Read();
                if (currentChar == 'n')
                    dialogueText += "\n";
            }
            else if(currentChar != '\n' && currentChar != '\r')//Else if not reading a tag, add character normally
                dialogueText += currentChar;
        }

        return conversation;
    }

    public static List<Conversation> ReadAllDialogueFromDirectory(string directory)
    {
        List<Conversation> allDialogues = new List<Conversation>();

        return allDialogues;
    }
    #endregion
}
