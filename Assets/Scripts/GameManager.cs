using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks {

	public static GameManager gameManager;
	public static int spawnLocation = 0;

	public GameObject car;
	GameObject playerCar;

	public Transform spawns, checkpoints, players;

	void Start() {
		DefaultPool d = (DefaultPool)(PhotonNetwork.PrefabPool);
		if (!d.ResourceCache.ContainsKey("car")) {
			d.ResourceCache.Add("car", car);
		}

		gameManager = this;

		Transform spawn = spawns.GetChild(spawnLocation);

		playerCar = PhotonNetwork.Instantiate("car", spawn.position, spawn.rotation);
		playerCar.tag = "LocalPlayer";

		WheelDrive w = playerCar.GetComponent<WheelDrive>();
		Camera.main.GetComponent<DriftCamera>().SetTarget(w.lookAtTarget, w.positionTarget, w.sideView);
	}

	public void CarEnter(GameObject c) {

	}


	void Update() {

	}
}
