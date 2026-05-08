using UnityEngine;

public class CameraMount : MonoBehaviour
{
	public Transform drone;
	public ViewModeController viewMode;
	public GameObject cockpit;
	public GameObject droneVisual;

	public float thirdPersonDistance = 5f;
	public float thirdPersonHeight = 1.8f;
	public float thirdPersonFollowSmooth = 8f;

	Vector3 smoothedRigPos;
	bool initialized;
	bool rigOrientationInitialized;

	void LateUpdate()
	{
		if (drone == null || viewMode == null) return;

		if (!rigOrientationInitialized)
		{
			Vector3 fwd = drone.forward;
			fwd.y = 0f;
			if (fwd.sqrMagnitude > 0.0001f)
				transform.rotation = Quaternion.LookRotation(fwd.normalized, Vector3.up);
			rigOrientationInitialized = true;
		}

		switch (viewMode.CurrentMode)
		{
			case ViewModeController.Mode.FirstPersonNoCockpit:
				transform.position = drone.position;
				if (cockpit != null) cockpit.SetActive(false);
				if (droneVisual != null) droneVisual.SetActive(false);
				break;
			case ViewModeController.Mode.FirstPersonCockpit:
				transform.position = drone.position;
				if (cockpit != null) cockpit.SetActive(true);
				if (droneVisual != null) droneVisual.SetActive(false);
				break;
			case ViewModeController.Mode.ThirdPersonBehind:
				Vector3 forward = drone.forward;
				forward.y = 0f;
				if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
				forward.Normalize();
				Vector3 target = drone.position - forward * thirdPersonDistance + Vector3.up * thirdPersonHeight;
				if (!initialized) { smoothedRigPos = target; initialized = true; }
				smoothedRigPos = Vector3.Lerp(smoothedRigPos, target, thirdPersonFollowSmooth * Time.deltaTime);
				transform.position = smoothedRigPos;
				transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
				if (cockpit != null) cockpit.SetActive(false);
				if (droneVisual != null) droneVisual.SetActive(true);
				break;
		}
	}

	void OnEnable()
	{
		initialized = false;
	}
}
