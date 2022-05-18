using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	[SerializeField] Canvas uiCanvas;
	const int infoDuration = 420; //Number of frames that info (health, lives, collectibles) stays on screen
	const float panSpeed = 5.6f; //Speed at which UI will pan into its correct location

    // Start is called before the first frame update
    void Awake()
    {
	}

	void Start() //Disable all of the objects after they had a chance to be awake
	{

	}

    // Update is called once per frame
    void Update()
    {
	}

	public void SetRenderCamera(Camera camera)
    {
		uiCanvas.worldCamera = camera;
    }
}
