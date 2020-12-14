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
	Dictionary<int, Transform> hc;

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
		hc = new Dictionary<int, Transform>();
		for (int i = 0; i < checkpoints.childCount; i++) {
			ch.Add(checkpoints.GetChild(i).gameObject, i);
			hc.Add(i, checkpoints.GetChild(i));
		}

		PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { MultiProperties.LAP, 0 }, { MultiProperties.CHECKPOINT, 0 } });

		lap.text = "Lap " + currentLap + "/" + numOfLaps;

		cars = new Dictionary<Transform, Photon.Realtime.Player>();
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
		position.text = "Pos " + 0 + "/" + PhotonNetwork.CurrentRoom.PlayerCount;
	}
	void Finish() {
		playerCar.tag = "Player";
		playerCar.transform.GetChild(0).gameObject.tag = "Player";

		alert.gameObject.SetActive(true);
		alert.text = "Finished!";

		PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { MultiProperties.FINISHED, true } });

		bool allFinished = true;

		foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerListOthers) {
			if (!p.CustomProperties.ContainsKey(MultiProperties.FINISHED)) {
				allFinished = false;
			}
		}

		if (allFinished) {
			Invoke("Return", 1);
		}
	}
	void Return() {
		PhotonNetwork.LoadLevel(1);
	}


	void Update() {
		if (Input.GetKeyDown(KeyCode.R) && !resetCooldown) {
			ResetCar();
		}
	}

	Dictionary<Transform, Photon.Realtime.Player> cars;
	public void RegisterCar(Transform g) {
		cars.Add(g, g.gameObject.GetComponent<PhotonView>().Owner);
	}
	public void UnRegisterCar(Transform g) {
		cars.Remove(g);
	}

	void FixedUpdate() {
		if (playerCar != null) {
			bool succeed = true;
			object check, lp;
			succeed &= PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(MultiProperties.CHECKPOINT, out check);
			succeed &= PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(MultiProperties.LAP, out lp);
			if (!succeed)
				return;

			float myPos = ((int)check + 1) * ((int)lp + 1) * 1000000 - Vector3.Distance(playerCar.transform.position, hc[mod((int)check + 1, ch.Count)].position);
			int pos = 0;
			print(myPos + " my");

			foreach (Transform t in players) {
				if (t == playerCar.transform)
					continue;

				succeed &= cars[t].CustomProperties.TryGetValue(MultiProperties.CHECKPOINT, out check);
				succeed &= cars[t].CustomProperties.TryGetValue(MultiProperties.LAP, out lp);
				if (!succeed)
					return;

				if (((int)check + 1) * ((int)lp + 1) * 1000000 - Vector3.Distance(t.position, hc[mod((int)check + 1, ch.Count)].position) > myPos) {
					pos++;
				}
				print(((int)check + 1) * ((int)lp + 1) * 1000000 - Vector3.Distance(t.position, hc[mod((int)check + 1, ch.Count)].position));
			}

			position.text = "Pos " + (pos + 1) + "/" + PhotonNetwork.CurrentRoom.PlayerCount;
		}
	}
	int mod(int x, int m) {
		return (x % m + m) % m;
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
