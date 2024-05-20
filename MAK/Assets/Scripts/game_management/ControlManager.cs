using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that reads and handles controls
/// </summary>
public class ControlManager : MonoBehaviour
{
	/// <summary>
	/// Class that represents a single frame worth of inputs from the player
	/// </summary>
	public class InputFrame
	{
		public bool jump, attack, back, start, select, other, leftt, rightt;

		public Vector2 controllerInput;

		public enum InputType { JUMP, ATTACK, BACK, START, SELECT, OTHER, LEFT_TRIG, RIGHT_TRIG };
		public bool GetInput(InputType inputType)
		{
			switch (inputType)
			{
				case InputType.JUMP:
					return jump;
				case InputType.ATTACK:
					return attack;
				case InputType.BACK:
					return back;
				case InputType.START:
					return start;
				case InputType.SELECT:
					return select;
				case InputType.OTHER:
					return other;
				case InputType.LEFT_TRIG:
					return leftt;
				case InputType.RIGHT_TRIG:
					return rightt;
				default:
					return false;
			}
		}
		public InputFrame() { jump = false; attack = false; controllerInput = Vector2.zero; }
	}

	//----------------------------Variables-------------------------
	public const int bufferSize = 12;
	static InputFrame[] inputBuffer = new InputFrame[bufferSize];
	static int currentIndex = 0, previousIndex = bufferSize - 1; //Current index in the buffer, and index of the last frame. Store both to avoid frequent wrap around checking
	const float deadZone = 0.2f;

	//-----------------------------Methods--------------------------

	// Start is called before the first frame update
	void Start()
    {
		//Initialize the input buffer
		for (int i = 0; i < bufferSize; i++)
			inputBuffer[i] = new InputFrame();
	}

    // Update is called once per frame
    public void Update()
    {
		UpdateBuffer();
    }

	//**********Helper methods**************
	/// <summary> Updates the input buffer for this frame. Overwrites anything previously in that spot in the buffer. </summary>
	private void UpdateBuffer()
	{
		//Go to the next spot in the input buffer
		previousIndex = currentIndex;
		currentIndex++;
		currentIndex %= bufferSize; //Make sure the currentIndex never goes out of bounds.

		inputBuffer[currentIndex].jump = Input.GetButton("Jump");
		inputBuffer[currentIndex].attack = Input.GetButton("Attack");
		inputBuffer[currentIndex].back = Input.GetButton("Back");
		inputBuffer[currentIndex].start = Input.GetButton("Start");
		inputBuffer[currentIndex].select = Input.GetButton("Select");
		inputBuffer[currentIndex].other = Input.GetButton("Other");
		inputBuffer[currentIndex].leftt = Input.GetButton("LeftTrigger");
		inputBuffer[currentIndex].rightt = Input.GetButton("RightTrigger");
		inputBuffer[currentIndex].controllerInput.x = Input.GetAxis("Horizontal");
		inputBuffer[currentIndex].controllerInput.y = Input.GetAxis("Vertical");
	}

	//**********Input returning*************
	/// <summary>
	/// Returns the number of frames that jump has been held for. Can be a maximum of bufferSize
	/// </summary>
	public static int GetJumpHeldFrames()
	{
		int count = 0;
		for (count = 0; count < bufferSize; count++)
		{
			if (!inputBuffer[(bufferSize + currentIndex - count) % bufferSize].jump)
				return count;
		}

		return bufferSize;
	}

	//DEBUG This method call could be replaced by a counter in the UpdateBuffer() method, which would reduce run time from O(bufferSize) to O(1) every frame
	/// <summary>
	/// Returns the number of frames that a specific input has been held for. Can have a maximum of bufferSize. There is also GetJumpHeld Frames, which is faster.
	/// </summary>
	public static int GetInputHeldFrames(InputFrame.InputType inputType)
	{
		int count = 0;
		for (count = 0; count < bufferSize; count++)
		{
			if (!inputBuffer[(bufferSize + currentIndex - count) % bufferSize].GetInput(inputType))
				return count;
		}

		return bufferSize;
	}

	/// <summary> Tells how many frames ago the given input was last pressed (Going from not down to down). </summary>
	/// <returns></returns>
	public static int GetLastPress(InputFrame.InputType input)
	{
		int frameToCheck = currentIndex;
		int lastFrame = currentIndex - 1;
		if (lastFrame < 0)
			lastFrame = bufferSize - 1;

		int framesPassed = 0; //Number of frames that have passed since the last time the button was pressed

		while (lastFrame != (currentIndex + 1) % bufferSize) //Keep checking backwards until we hit the next frame.
		{
			if (inputBuffer[frameToCheck].GetInput(input) && !inputBuffer[lastFrame].GetInput(input))
				break;

			framesPassed++; //Increment the number of frames that have passed since the press
			frameToCheck = lastFrame; //Check the last frame versus the frame two back, now
			lastFrame--; //Go to the last frame, wrapping to the back of the buffer if needed
			if (lastFrame < 0)
				lastFrame = bufferSize - 1;
		}

		return framesPassed;
	}

	/// <summary>Returns whether or not this is the first frame on which jump was pressed</summary>
	public static bool JumpPressed(){ return inputBuffer[currentIndex].jump && !inputBuffer[previousIndex].jump; }
	/// <summary>Returns whether or not jump is being pressed on this frame</summary>
	public static bool JumpDown() { return inputBuffer[currentIndex].jump; }
	/// <summary>Returns whether or not this is the first frame on which attack was pressed</summary>
	public static bool AttackPressed() { return inputBuffer[currentIndex].attack && !inputBuffer[previousIndex].attack; }
	/// <summary>Returns whether or not attack is being pressed on this frame</summary>
	public static bool AttackDown() { return inputBuffer[currentIndex].attack; }
	/// <summary>Returns whether or not this is the first frame on which back was pressed</summary>
	public static bool BackPressed() { return inputBuffer[currentIndex].back && !inputBuffer[previousIndex].back; }
	/// <summary>Returns whether or not back is being pressed on this frame</summary>
	public static bool BackDown() { return inputBuffer[currentIndex].back; }
	/// <summary>Returns whether or not other is being pressed on this frame</summary>
	public static bool TargetDown() { return inputBuffer[currentIndex].other; }
	/// <summary>Returns whether or not this is the first frame on which other was pressed</summary>
	public static bool TargetPressed() { return inputBuffer[currentIndex].other && !inputBuffer[previousIndex].other; }
	/// <summary>Returns whether or not this is the first frame on which start was pressed</summary>
	public static bool StartPressed() { return inputBuffer[currentIndex].start && !inputBuffer[previousIndex].start; }
	/// <summary>Returns whether or not this is the first frame on which select was pressed</summary>
	public static bool SelectPressed() { return inputBuffer[currentIndex].select && !inputBuffer[previousIndex].select; }
	/// <summary>Returns whether or not this is the first frame on which the other button (top) was pressed</summary>
	public static bool OtherPressed() { return inputBuffer[currentIndex].other && !inputBuffer[previousIndex].other; }
	/// <summary>Returns whether or not the other button (top) is pressed</summary>
	public static bool OtherDown() { return inputBuffer[currentIndex].other; }
	/// <summary>Returns whether or not this is the first frame on which the left trigger was pressed</summary>
	public static bool LeftTriggerPressed() { return inputBuffer[currentIndex].leftt && !inputBuffer[previousIndex].leftt; }
	/// <summary>Returns whether or not the left trigger is pressed</summary>
	public static bool LeftTriggerDown() { return inputBuffer[currentIndex].leftt; }
	/// <summary>Returns whether or not this is the first frame on which the left trigger was pressed</summary>
	public static bool RightTriggerPressed() { return inputBuffer[currentIndex].rightt && !inputBuffer[previousIndex].rightt; }
	/// <summary>Returns whether or not the left trigger is pressed</summary>
	public static bool RightTriggerDown() { return inputBuffer[currentIndex].rightt; }
	/// <summary>Returns whether or not this is the first frame on which the button for talking was pressed</summary>
	public static bool TalkPressed() { return inputBuffer[currentIndex].rightt && !inputBuffer[previousIndex].rightt; }
	/// <summary>Returns whether or not the button for talking is being pressed on this frame</summary>
	public static bool TalkDown() { return inputBuffer[currentIndex].rightt; }
	/// <summary>Returns whether or not this is the first frame on which up passed the deadzone</summary>
	public static bool UpPressed() { return inputBuffer[currentIndex].controllerInput.y >= deadZone && inputBuffer[previousIndex].controllerInput.y < deadZone; }
	/// <summary>Returns whether or not this is the first frame on which down passed the deadzone</summary>
	public static bool DownPressed() { return inputBuffer[currentIndex].controllerInput.y <= -deadZone && inputBuffer[previousIndex].controllerInput.y > -deadZone; }	
	/// <summary>Returns whether or not this is the first frame on which left passed the deadzone</summary>
	public static bool LeftPressed() { return inputBuffer[currentIndex].controllerInput.x <= -deadZone && inputBuffer[previousIndex].controllerInput.x > -deadZone; }	
	/// <summary>Returns whether or not this is the first frame on which right passed the deadzone</summary>
	public static bool RightPressed() { return inputBuffer[currentIndex].controllerInput.x >= deadZone && inputBuffer[previousIndex].controllerInput.x < deadZone; }
	/// <summary>Returns whether or not down on the controller is past the deadzone</summary>
	public static bool DownDown() { return inputBuffer[currentIndex].controllerInput.y <= -deadZone; }
	/// <summary>Returns whether or not this is the first frame on which left passed the deadzone</summary>
	public static bool LeftDown() { return inputBuffer[currentIndex].controllerInput.x <= -deadZone; }
	/// <summary>Returns whether or not this is the first frame on which right passed the deadzone</summary>
	public static bool RightDown() { return inputBuffer[currentIndex].controllerInput.x >= deadZone; }
	/// <summary>Returns a Vector2 of the input movement for this frame</summary>
	public static Vector2 GetMovement() { return inputBuffer[currentIndex].controllerInput; }
	/// <summary>Returns a Vector2 of the input horizontal movement for this frame</summary>
	public static float GetHorizontalMovement() { return inputBuffer[currentIndex].controllerInput.x; }
	/// <summary>Returns a Vector2 of the input vertical movement for this frame</summary>
	public static float GetVerticalMovement() { return inputBuffer[currentIndex].controllerInput.y; }

}
