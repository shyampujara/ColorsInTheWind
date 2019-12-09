// ﻿using UnityEngine;
// using System.Collections.Generic;
//
// public class CameraLogic : MonoBehaviour {
//
//     private Transform m_currentTarget;
//     private float m_distance = 2f;
//     private float m_height = 1;
//     private float m_lookAtAroundAngle = 180;
//
//     [SerializeField] private List<Transform> m_targets;
//     private int m_currentIndex;
//
// 	private void Start () {
//         if(m_targets.Count > 0)
//         {
//             m_currentIndex = 0;
//             m_currentTarget = m_targets[m_currentIndex];
//         }
// 	}
//
//     private void SwitchTarget(int step)
//     {
//         if(m_targets.Count == 0) { return; }
//         m_currentIndex+=step;
//         if (m_currentIndex > m_targets.Count-1) { m_currentIndex = 0; }
//         if (m_currentIndex < 0) { m_currentIndex = m_targets.Count - 1; }
//         m_currentTarget = m_targets[m_currentIndex];
//     }
//
//     public void NextTarget() { SwitchTarget(1); }
//     public void PreviousTarget() { SwitchTarget(-1); }
//
//     private void Update () {
//         if (m_targets.Count == 0) { return; }
//     }
//
//     private void LateUpdate()
//     {
//         if(m_currentTarget == null) { return; }
//
//         float targetHeight = m_currentTarget.position.y + m_height;
//         float currentRotationAngle = m_lookAtAroundAngle;
//
//         Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);
//
//         Vector3 position = m_currentTarget.position;
//         position -= currentRotation * Vector3.forward * m_distance;
//         position.y = targetHeight;
//
//         transform.position = position;
//         transform.LookAt(m_currentTarget.position + new Vector3(0, m_height, 0));
//     }
// }


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Makes the camera follow the player

public class CameraLogic : MonoBehaviour {

	public Transform target;	// Target to follow (player)

	public Vector3 offset;			// Offset from the player
	public float zoomSpeed = 4f;	// How quickly we zoom
	public float minZoom = 5f;		// Min zoom amount
	public float maxZoom = 15f;		// Max zoom amount

	public float pitch = 2f;		// Pitch up the camera to look at head

	public float yawSpeed = 100f;	// How quickly we rotate

	// In these variables we store input from Update
	private float currentZoom = 10f;
	private float currentYaw = 0f;

	private int up = 0;

	void Update ()
	{
		// Adjust our zoom based on the scrollwheel
		currentZoom -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
		currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

		// Adjust our camera's rotation around the player
		currentYaw -= Input.GetAxis("Horizontal") * yawSpeed * Time.deltaTime;
	}

	void LateUpdate ()
	{
		// Set our cameras position based on offset and zoom
		transform.position = target.position - offset * currentZoom + new Vector3(0, up, 0);
		// Look at the player's head
		transform.LookAt(target.position + Vector3.up * pitch);

		// Rotate around the player
		transform.RotateAround(target.position, Vector3.up, currentYaw);

		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			up += 1;
		}
		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			up -= 1;
		}
	}

  public void PreviousTarget() {}
  public void NextTarget() {}


}
