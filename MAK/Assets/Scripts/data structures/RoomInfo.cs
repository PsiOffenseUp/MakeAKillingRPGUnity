using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class RoomInfo
{
    public string name { get; private set; }
    public Color color { get; private set; }
    public Scene scene { get; private set;}

    public void GetScene() 
    { 
        scene = SceneManager.GetActiveScene();
    }
}
