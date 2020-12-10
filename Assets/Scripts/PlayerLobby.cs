using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobby : MonoBehaviour {
	public Text name;
	public Toggle ready;

	public void Display(string name, bool ready) {
		this.name.text = name;
		this.ready.isOn = ready;
	}
}
