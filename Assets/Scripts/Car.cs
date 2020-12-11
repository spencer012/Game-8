using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour {

	PhotonView photonView;
	Rigidbody rigidbody;

	public bool cameraFocus = false;
	public float cameraAngle = 25, cameraBack = 20, cameraUp = 10;

	public List<AxleInfo> axleInfos;
	public float maxMotorTorque, maxSteeringAngle, centerOfMass;

	void Start() {
		photonView = GetComponent<PhotonView>();
		rigidbody = GetComponent<Rigidbody>();

		rigidbody.centerOfMass = Vector3.down * centerOfMass;
	}

	void Update() {
		if (photonView.IsMine || true) {
			float motor = maxMotorTorque * Input.GetAxis("Vertical");
			float steering = maxSteeringAngle * Input.GetAxis("Horizontal");

			foreach (AxleInfo axleInfo in axleInfos) {
				if (axleInfo.steering) {
					axleInfo.leftWheel.steerAngle = steering;
					axleInfo.rightWheel.steerAngle = steering;
				}
				if (axleInfo.motor) {
					axleInfo.leftWheel.motorTorque = motor;
					axleInfo.rightWheel.motorTorque = motor;
				}
			}

			rigidbody.AddForce(transform.up * -1 * rigidbody.velocity.magnitude, ForceMode.Force);

			//rigidbody.AddForce(transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * forwardsForce * 100);
			//rigidbody.AddTorque(transform.up * Input.GetAxis("Horizontal") * Time.deltaTime * turningForce * 100 * (rigidbody.velocity.magnitude / 100));

			//Debug.Log(transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * forwardsForce * 100 + " : " + transform.up * Input.GetAxis("Horizontal") * Time.deltaTime * turningForce * 100);
		}

		if (cameraFocus) {
			Camera.main.transform.parent = transform;
			Camera.main.transform.localPosition = new Vector3(0, cameraUp, -cameraBack);
			Camera.main.transform.localRotation = Quaternion.Euler(cameraAngle, 0, 0);

			//Camera.main.transform.position = transform.position - (transform.forward * 30) + (transform.up * 30);
			//Camera.main.transform.rotation = Quaternion.Euler(25, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
		}
	}

	void FixedUpdate() {
	}
}

[System.Serializable]
public class AxleInfo {
	public WheelCollider leftWheel;
	public WheelCollider rightWheel;
	public bool motor; // is this wheel attached to motor?
	public bool steering; // does this wheel apply steer angle?
}