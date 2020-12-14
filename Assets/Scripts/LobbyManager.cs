using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class LobbyManager : MonoBehaviourPunCallbacks {

	public static LobbyManager lobbyManager;

	public GameObject connecting, buttons,
		main, lobby;

	public PlayerLobby[] playerLobbies;

	public Button startB, readyB;

	void Start() {
		lobbyManager = this;
		if (!PhotonNetwork.IsConnected) {
			PhotonNetwork.LocalPlayer.NickName = "Player " + Random.Range(1000, 10000);
			PhotonNetwork.ConnectUsingSettings();
			PhotonNetwork.AutomaticallySyncScene = true;

			buttons.SetActive(false);
			connecting.SetActive(true);
		}
		else if (PhotonNetwork.InRoom) {
			OnJoinedRoom();
		}
		else {
			buttons.SetActive(true);
			connecting.SetActive(false);
		}

		main.SetActive(true);
		lobby.SetActive(false);

		readyB.onClick.AddListener(ReadyUp);
		startB.onClick.AddListener(StartGame);
	}

	public override void OnConnectedToMaster() {
		buttons.SetActive(true);
		connecting.SetActive(false);
		//this.SetActivePanel(SelectionPanel.name);
	}
	public override void OnDisconnected(DisconnectCause cause) {
		buttons.SetActive(false);
		connecting.SetActive(true);
	}


	public void JoinRandomGame() {
		PhotonNetwork.JoinRandomRoom();
		buttons.SetActive(false);
		connecting.SetActive(true);
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
		SceneManager.LoadScene(1);
	}
	public void ReadyUp() {
		Hashtable props = new Hashtable { { MultiProperties.READY, true } };
		PhotonNetwork.LocalPlayer.SetCustomProperties(props);
		RefreshPlayerList();
		readyB.interactable = false;
	}
	public void StartGame() {
		PhotonNetwork.CurrentRoom.IsOpen = false;
		PhotonNetwork.CurrentRoom.IsVisible = false;

		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { MultiProperties.NUM_OF_PLAYERS, PhotonNetwork.CurrentRoom.PlayerCount } });

		PhotonNetwork.LoadLevel(2);
	}

	public override void OnJoinedRoom() {
		ChangeLocalScene(1);

		Hashtable props = new Hashtable { { MultiProperties.READY, false } };
		PhotonNetwork.LocalPlayer.SetCustomProperties(props);

		RefreshPlayerList();
	}
	void RefreshPlayerList() {
		//print("--------------------");

		Player[] playerList = PhotonNetwork.PlayerList;
		for (int i = 0; i < 8; i++) {
			if (i < playerList.Length) {
				playerLobbies[i].gameObject.SetActive(true);
				playerLobbies[i].Display(playerList[i].NickName, (bool)GetValueOrDefault(playerList[i].CustomProperties, MultiProperties.READY, false));

				//foreach (System.Collections.DictionaryEntry entry in playerList[i].CustomProperties) {
					//print(entry.Key + ":" + entry.Value + "\t" + (bool)GetValueOrDefault(playerList[i].CustomProperties, MultiProperties.READY, false));
				//}
				if (playerList[i].IsLocal) {
					GameManager.spawnLocation = i;
				}

			}
			else {
				playerLobbies[i].gameObject.SetActive(false);
			}
		}
		startB.interactable = CheckPlayersReady();
		//print("--------------------");

	}

	private static object GetValueOrDefault(Hashtable h, string key, object d) {
		object r;
		if (h.TryGetValue(key, out r)) {
			return r;
		}
		else {
			return d;
		}

	}

	public override void OnPlayerEnteredRoom(Player newPlayer) {
		Player[] playerList = PhotonNetwork.PlayerList;
		int i = playerList.Length - 1;
		playerLobbies[i].gameObject.SetActive(true);
		playerLobbies[i].Display(playerList[i].NickName, (bool)GetValueOrDefault(playerList[i].CustomProperties, MultiProperties.READY, false));
	}
	public override void OnPlayerLeftRoom(Player otherPlayer) {
		RefreshPlayerList();
	}
	public override void OnMasterClientSwitched(Player newMasterClient) {
		if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber) {
			startB.interactable = CheckPlayersReady();
		}
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) {
		RefreshPlayerList();
	}

	private bool CheckPlayersReady() {
		if (!PhotonNetwork.IsMasterClient) {
			return false;
		}

		foreach (Player p in PhotonNetwork.PlayerList) {
			object isPlayerReady;
			if (p.CustomProperties.TryGetValue(MultiProperties.READY, out isPlayerReady)) {
				if (!(bool)isPlayerReady) {
					return false;
				}
			}
			else {
				return false;
			}
		}

		return true;
	}

	public override void OnCreateRoomFailed(short returnCode, string message) {
		print(message);
		buttons.SetActive(true);
		connecting.SetActive(false);
	}

	public override void OnJoinRoomFailed(short returnCode, string message) {
		print(message);
		buttons.SetActive(true);
		connecting.SetActive(false);
	}

	private void ChangeLocalScene(int i) {
		main.SetActive(false);
		lobby.SetActive(false);
		switch (i) {
			case 0:
				main.SetActive(true);
				break;
			case 1:
				lobby.SetActive(true);
				break;
			default: break;
		}
	}
}