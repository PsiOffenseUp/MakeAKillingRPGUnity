using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Object to intialize a room by starting a song on level start and displaying room name text
/// </summary>
public class RoomInitializer : MonoBehaviour
{

    [System.Serializable]
    class RoomUIInfo
    {
        public Color outlineColor;
        public Color highlightColor;
        public Color bottomColor;
        public Color topColor;
        public string roomName;
        public Material loadingScreenMat;
    }

    [SerializeField] BoogalooGame.Song song;
    [SerializeField] RoomUIInfo roomUIInfo;

    void Start()
    {
        if (song != null)
            GameplayManager.audioPlayer.PlaySong(song);

        if(roomUIInfo.loadingScreenMat != null)
            GameplayManager.uiManager.SetLoadingScreenAMaterial(roomUIInfo.loadingScreenMat);

        //Set all of the global shader information
        Shader.SetGlobalColor("TopColor", roomUIInfo.topColor);
        Shader.SetGlobalColor("BottomColor", roomUIInfo.bottomColor);
        Shader.SetGlobalColor("LineColor", roomUIInfo.outlineColor);
        Shader.SetGlobalColor("LightingColor", roomUIInfo.highlightColor);

        StartCoroutine(GameplayManager.uiManager.PlayRoomTitleAnimation(roomUIInfo.roomName));
    }
}
