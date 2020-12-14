using UnityEngine;
using System;
using Photon.Pun;

[Serializable]
public enum DriveType {
	RearWheelDrive,
	FrontWheelDrive,
	AllWheelDrive
}

public class WheelDrive : MonoBehaviour, IPunObservable {
	[Tooltip("Maximum steering angle of the wheels")]
	public int maxAngle = 30;
	[Tooltip("Maximum torque applied to the driving wheels")]
	public int maxTorque = 300;
	[Tooltip("Maximum brake torque applied to the driving wheels")]
	public int brakeTorque = 30000;
	[Tooltip("If you need the visual wheels to be attached automatically, drag the wheel shape here.")]
	public GameObject wheelShape;

	[Tooltip("The vehicle's speed when the physics engine can use different amount of sub-steps (in m/s).")]
	public float criticalSpeed = 5f;
	[Tooltip("Simulation sub-steps when the speed is above critical.")]
	public int stepsBelow = 5;
	[Tooltip("Simulation sub-steps when the speed is below critical.")]
	public int stepsAbove = 1;

	[Tooltip("The vehicle's drive type: rear-wheels drive, front-wheels drive or all-wheels drive.")]
	public DriveType driveType;

	private WheelCollider[] m_Wheels;

	PhotonView photonView;

	public Transform lookAtTarget, positionTarget, sideView;

	// Find all the WheelColliders down in the hierarchy.
	void Start() {
		m_Wheels = GetComponentsInChildren<WheelCollider>();

		for (int i = 0; i < m_Wheels.Length; ++i) {
			var wheel = m_Wheels[i];

			// Create wheel shapes only when needed.
			if (wheelShape != null) {
				var ws = Instantiate(wheelShape);
				ws.transform.parent = wheel.transform;
			}
		}

		photonView = GetComponent<PhotonView>();
	}

	void OnEnable() {
		transform.parent = GameManager.gameManager.players;
	}

	// This is a really simple approach to updating wheels.
	// We simulate a rear wheel drive car and assume that the car is perfectly symmetric at local zero.
	// This helps us to figure our which wheels are front ones and which are rear.
	int angle = 0, torque = 0, handBrake = 0;
	void Update() {
		m_Wheels[0].ConfigureVehicleSubsteps(criticalSpeed, stepsBelow, stepsAbove);

		if (photonView.IsMine && tag == "LocalPlayer") {
			angle = maxAngle * (int)Input.GetAxis("Horizontal");
			torque = maxTorque * (int)Input.GetAxis("Vertical");

			handBrake = Input.GetKey(KeyCode.X) ? brakeTorque : 0;
		}

		foreach (WheelCollider wheel in m_Wheels) {
			// A simple car where front wheels steer while rear ones drive.
			if (wheel.transform.localPosition.z > 0)
				wheel.steerAngle = angle;

			if (wheel.transform.localPosition.z < 0) {
				wheel.brakeTorque = handBrake;
			}

			if (wheel.transform.localPosition.z < 0 && driveType != DriveType.FrontWheelDrive) {
				wheel.motorTorque = torque;
			}

			if (wheel.transform.localPosition.z >= 0 && driveType != DriveType.RearWheelDrive) {
				wheel.motorTorque = torque;
			}

			// Update visual wheels if any.
			if (wheelShape) {
				Quaternion q;
				Vector3 p;
				wheel.GetWorldPose(out p, out q);

				// Assume that the only child of the wheelcollider is the wheel shape.
				Transform shapeTransform = wheel.transform.GetChild(0);

				if (wheel.name == "a0l" || wheel.name == "a1l" || wheel.name == "a2l") {
					shapeTransform.rotation = q * Quaternion.Euler(0, 180, 0);
					shapeTransform.position = p;
				}
				else {
					shapeTransform.position = p;
					shapeTransform.rotation = q;
				}
			}
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		if (stream.IsWriting) {
			stream.SendNext(angle);
			stream.SendNext(torque);
			stream.SendNext(handBrake);
		}
		else {
			angle = (int)stream.ReceiveNext();
			torque = (int)stream.ReceiveNext();
			handBrake = (int)stream.ReceiveNext();
		}
	}
}
