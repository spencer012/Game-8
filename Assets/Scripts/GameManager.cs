using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable =  ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks {

	public static GameManager gameManager;
	public static int spawnLocation = 0;

	public GameObject car;
	GameObject playerCar;

	public Transform spawns, checkpoints, players;
	Dictionary<GameObject, int> ch;

	public Text alert, position, lap;

	int currentCheckpoint = 0, currentLap = 0;

	public int numOfLaps = 2;

	void Start() {
		DefaultPool d = (DefaultPool)(PhotonNetwork.PrefabPool);
		if (!d.ResourceCache.ContainsKey("car")) {
			d.ResourceCache.Add("car", car);
		}

		gameManager = this;

		Transform spawn = spawns.GetChild(spawnLocation);

		playerCar = PhotonNetwork.Instantiate("car", spawn.position, spawn.rotation);
		playerCar.tag = "LocalPlayer";
		playerCar.transform.GetChild(0).gameObject.tag = "LocalPlayer";

		WheelDrive w = playerCar.GetComponent<WheelDrive>();
		Camera.main.GetComponent<DriftCamera>().SetTarget(w.lookAtTarget, w.positionTarget, w.sideView);

		ch = new Dictionary<GameObject, int>();
		for (int i = 0; i < checkpoints.childCount; i++) {
			ch.Add(checkpoints.GetChild(i).gameObject, i);
		}

		PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { MultiProperties.LAP, 0 }, { MultiProperties.CHECKPOINT, 0 } });


		lap.text = "Lap " + currentLap + "/" + numOfLaps;
	}

	public void CarEnter(GameObject c) {
		int num = ch[c];

		//print(num + " " + currentCheckpoint + ":" + currentLap);

		if (num - 1 == currentCheckpoint) {
			currentCheckpoint++;
			PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { MultiProperties.CHECKPOINT, currentCheckpoint } });
		}
		else if (num == 0 && currentCheckpoint == ch.Count - 1 && currentLap == numOfLaps - 1) {
			Finish();
		}
		else if (num == 0 && currentCheckpoint == ch.Count - 1) {
			currentCheckpoint = 0;
			currentLap++;
			PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { MultiProperties.LAP, currentLap }, { MultiProperties.CHECKPOINT, 0 } });
		}

		lap.text = "Lap " + currentLap + "/" + numOfLaps;
	}
	void Finish() {
		playerCar.tag = "Player";
		playerCar.transform.GetChild(0).gameObject.tag = "Player";
	}


	void Update() {
		if (Input.GetKeyDown(KeyCode.R) && !resetCooldown) {
			ResetCar();
		}
	}

	bool resetCooldown = false;
	void ResetCar() {
		Transform t = checkpoints.GetChild(currentCheckpoint);
		playerCar.transform.position = t.position;
		playerCar.transform.rotation = t.rotation;
		Rigidbody r = playerCar.GetComponent<Rigidbody>();
		r.velocity = Vector3.zero;
		r.angularVelocity = Vector3.zero;

		resetCooldown = true;
		Invoke("ResetCooldownReset", 1);
	}
	void ResetCooldownReset() {
		resetCooldown = false;
	}

	public void Exit() {
		PhotonNetwork.LeaveRoom(false);
		SceneManager.LoadScene(1);
	}
}
