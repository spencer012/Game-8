using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviourPunCallbacks {

	public static LobbyManager lobbyManager;



	void Start() {
		if (lobbyManager == null) {
			lobbyManager = this;
			DontDestroyOnLoad(gameObject);
		}
		else {
			Destroy(gameObject);
		}
		PhotonNetwork.AutomaticallySyncScene = true;
	}

	public override void OnConnectedToMaster() {
		//this.SetActivePanel(SelectionPanel.name);
	}
	public override void OnDisconnected(DisconnectCause cause) {

	}

	void JoinRandomGame() {
		PhotonNetwork.JoinRandomRoom();
	}
	public override void OnJoinRandomFailed(short returnCode, string message) {
		string roomName = "Room " + Random.Range(1000, 10000);

		RoomOptions options = new RoomOptions {
			MaxPlayers = 8,
			PlayerTtl = 10000,
			CustomRoomProperties = new Hashtable { { MultiProperties.LEVEL, 1 } }
		};

		PhotonNetwork.CreateRoom(roomName, options, null);
	}

	public void LeaveGame() {
		PhotonNetwork.LeaveRoom();
	}
	public void StartGame() {
		PhotonNetwork.CurrentRoom.IsOpen = false;
		PhotonNetwork.CurrentRoom.IsVisible = false;

		//PhotonNetwork.LoadLevel(3);
	}

	public override void OnJoinedRoom() {


		Hashtable props = new Hashtable { {MultiProperties.READY, false} };
		PhotonNetwork.LocalPlayer.SetCustomProperties(props);
	}

	public override void OnCreateRoomFailed(short returnCode, string message) {
		print(message);
	}

	public override void OnJoinRoomFailed(short returnCode, string message) {
		print(message);
	}
}


public enum MultiProperties {
	READY,
	LEVEL
}