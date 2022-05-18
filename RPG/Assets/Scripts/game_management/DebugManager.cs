using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
	[SerializeField] Canvas debugCanvas;
	[SerializeField] TMPro.TextMeshProUGUI velocityText, positionText, stateText, framesText, actionText, directionText;
	[SerializeField] UnityEngine.UI.Image jumpButton, runButton, startButton, selectButton, attackButton, otherButton, controlStick, controlStickBack;

	Vector2 controlStickPosition; //Center position for the control stick
	Vector2 movement;
	const float controlStickRadius = 10.0f;
	PlayerController playerController;

	// Start is called before the first frame update
	void Start()
    {
		movement = Vector2.zero;
		playerController = GameplayManager.player.GetComponent<PlayerController>();
	}

	// Update is called once per frame
	void Update()
	{
		try
		{
			directionText.text = playerController.GetDirection().ToString();
			stateText.text = playerController.GetState().ToString();
			actionText.text = playerController.GetAction().ToString();
			/*
			velocityText.text = "Velocity: " + Object.player.GetVelocity().ToString();
			positionText.text = "Position: " + Object.player.transform.position.ToString();
			stateText.text = "State: " + Object.player.state.ToString();
			framesText.text = "Frames since launch: " + GameplayManager.frameTimer;
			actionText.text = "Action: " + Object.player.actionState.ToString();

			SetCollisionColor(leftCol, Object.player.collisionLeft);
			SetCollisionColor(rightCol, Object.player.collisionRight);
			SetCollisionColor(upCol, Object.player.collisionAbove);
			SetCollisionColor(downCol, Object.player.isGrounded);*/

			SetButtonColor(jumpButton, ControlManager.JumpDown(), Color.green);
			SetButtonColor(runButton, ControlManager.BackDown(), Color.blue);
			SetButtonColor(attackButton, ControlManager.AttackDown(), Color.red);
			SetButtonColor(otherButton, ControlManager.TargetDown(), Color.yellow);
			SetButtonColor(startButton, ControlManager.StartPressed(), Color.black);
			SetButtonColor(selectButton, ControlManager.SelectPressed(), Color.black);

			movement = ControlManager.GetMovement();
			controlStickPosition = controlStickBack.transform.position;
			controlStick.transform.position = controlStickPosition + movement.normalized * controlStickRadius;
			if (movement.x == 0.0f && movement.y == 0.0f)
				controlStick.color = Color.white;
			else
				controlStick.color = Color.magenta;
		}
		catch { }
	}

	void SetCollisionColor(UnityEngine.UI.Image image, bool isCollision)
	{
		if (isCollision)
			image.color = Color.red;
		else
			image.color = Color.white;
	}

	void SetButtonColor(UnityEngine.UI.Image image, bool isOn, Color color)
	{
		if (isOn)
			image.color = color;
		else
			image.color = Color.gray;
	}

	public void Activate()
	{
		debugCanvas.gameObject.SetActive(true);
	}

	public void Deactivate()
	{
		debugCanvas.gameObject.SetActive(false);
		this.enabled = false;
	}
	
	public void SetRenderCamera(Camera camera)
    {
		debugCanvas.worldCamera = camera;
    }

	/// <summary> Creates a debug point in the game with the given color. Expires after the lifetime frames pass </summary>
	/// <param name="position"></param>
	/// <param name="color"></param>
	public void CreateDebugPoint(Vector3 position, Color color, ulong lifetime = 1)
	{
		GameObject debugPoint = Instantiate(Resources.Load<GameObject>("prefabs/debug/Point"));
		debugPoint.transform.position = position;
		debugPoint.GetComponent<SpriteRenderer>().color = color;

		StartCoroutine(DestroyAfterFrames(debugPoint, lifetime));
	}

	/// <summary> Destroys the GameObject after the amount of frames given passes </summary>
	/// <param name="obj"></param>
	/// <param name="frames"></param>
	/// <returns></returns>
	static IEnumerator DestroyAfterFrames(GameObject obj, ulong frames)
	{
		while (frames > 1)
		{
			frames--;
			yield return null;
		}

		Destroy(obj);
	}
}
