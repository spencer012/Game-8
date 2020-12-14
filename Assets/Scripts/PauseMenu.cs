using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour {

	public KeyCode pauseKey = KeyCode.Escape;

	private bool on = false;
	public GameObject toggle;

	void Start() {
		toggle.SetActive(false);
	}
	public void Toggle() {
		on = !on;
		toggle.SetActive(on);
	}

	void Update() {
		if (Input.GetKeyDown(pauseKey)) {
			Toggle();
		}
	}

	public void Exit() {
		GameManager.gameManager.Exit();
	}
}
