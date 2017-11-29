using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GravitySwitcher))]
[RequireComponent(typeof(ObstacleSpawner))]
public class MultiplayerManager : Game {

    public string playerPrefabName;

    public Color localPlayerColour;

    GravitySwitcher gravity;
    ObstacleSpawner spawner;

    protected override void Awake() {
        base.Awake();

        Time.timeScale = 0;

        gravity = GetComponent<GravitySwitcher>();
        spawner = GetComponent<ObstacleSpawner>();

        gravity.enabled = false;
        spawner.enabled = false;

        PhotonNetwork.ConnectUsingSettings("Gravity Rebound " + Application.version);
    }

    void OnJoinedLobby() {
        Debug.Log("Joined Lobby");
        PhotonNetwork.JoinRandomRoom();
    }

    void OnPhotonRandomJoinFailed(object[] codeAndMsg) {
        Debug.Log("Creating Room");
        PhotonNetwork.CreateRoom("");
    }

    void OnJoinedRoom() {
        Debug.Log("Joined Room");

        NetworkPlayer player = PhotonNetwork.Instantiate(playerPrefabName, Vector3.zero, Quaternion.identity, 0).GetComponent<NetworkPlayer>();

        player.enabled = true;
        player.gameObject.layer = 0;

        player.SetColour(localPlayerColour);

        gravity.target = spawner.target = player;
        CameraManager.main.target = player.transform;

        Time.timeScale = 1;

        gravity.enabled = true;
        spawner.enabled = true;
    }

    void OnGUI() {
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
    }
}
