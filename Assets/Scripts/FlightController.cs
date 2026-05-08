using UnityEngine;
using UnityEngine.InputSystem;

public class FlightController : MonoBehaviour
{
	public Transform drone;
	public Transform cameraRig;
	public TrackManager track;
	public OVRHand rightHand;
	public Transform rightHandAnchor;
	public OVRHand leftHand;

	public float maxSpeedMps = 25f;
	public float deadZone = 0.04f;
	public float fullSpeedAtMeters = 0.35f;
	public float engageHoldSeconds = 0.25f;
	public float modeToggleHoldSeconds = 0.5f;
	public bool keyboardFallback = true;
	public float keyboardSpeedMps = 12f;

	public enum Mode { PinchThrottle, PalmDirection }
	public Mode flightMode = Mode.PinchThrottle;

	bool pinchEngaged;
	float pinchHoldTime;
	Vector3 anchorRigLocal;
	float toggleHoldTime;
	bool toggleAlreadyTriggered;

	// for hud
	public bool PinchEngaged => pinchEngaged;
	public bool RightHandTracked { get; private set; }
	public Vector3 AnchorWorldPosition { get; private set; }
	public Vector3 HandWorldPosition { get; private set; }
	public Vector3 LastVelocityRigLocal { get; private set; }
	public Vector3 LastVelocityWorld { get; private set; }
	public float LastMiddlePinch { get; private set; }
	public Vector3 LastPalmForwardWorld { get; private set; }
	public bool KeyboardDroveLastFrame { get; private set; }
	public float NormalizedThrottle { get; private set; } // 0..1 of maxSpeed
	public Mode CurrentMode => flightMode;

	void Update()
	{
		if (drone == null || track == null) return;

		HandleFlightModeToggle();

		RightHandTracked = rightHand != null && rightHand.IsTracked;
		if (rightHandAnchor != null)
			HandWorldPosition = rightHandAnchor.position;
		if (cameraRig != null && pinchEngaged)
			AnchorWorldPosition = cameraRig.TransformPoint(anchorRigLocal);
		LastPalmForwardWorld = rightHandAnchor != null ? -rightHandAnchor.up : Vector3.forward;

		if (!track.IsControlEnabled)
		{
			pinchEngaged = false;
			pinchHoldTime = 0f;
			LastVelocityRigLocal = Vector3.zero;
			LastVelocityWorld = Vector3.zero;
			NormalizedThrottle = 0f;
			LastMiddlePinch = 0f;
			KeyboardDroveLastFrame = false;
			return;
		}

		Vector3 velocityRigLocal = Vector3.zero;
		bool handDrove = false;

		if (RightHandTracked && rightHandAnchor != null && cameraRig != null)
		{
			if (flightMode == Mode.PinchThrottle)
				velocityRigLocal = ComputePinchThrottleVelocity(out handDrove);
			else
				velocityRigLocal = ComputePalmDirectionVelocity(out handDrove);
		}
		else
		{
			LastMiddlePinch = 0f;
		}

		KeyboardDroveLastFrame = false;
		if (!handDrove && keyboardFallback)
		{
			velocityRigLocal = ComputeKeyboardVelocity();
			KeyboardDroveLastFrame = velocityRigLocal.sqrMagnitude > 0.0001f;
		}

		LastVelocityRigLocal = velocityRigLocal;
		NormalizedThrottle = Mathf.Clamp01(velocityRigLocal.magnitude / Mathf.Max(0.01f, maxSpeedMps));

		Transform frame = cameraRig != null ? cameraRig : transform;
		Vector3 velocityWorld = frame.TransformDirection(velocityRigLocal);
		LastVelocityWorld = velocityWorld;
		var rb = drone.GetComponent<Rigidbody>();

		if (velocityWorld.sqrMagnitude > 0.0001f)
		{
			Vector3 newPos = drone.position + velocityWorld * Time.deltaTime;
			drone.position = newPos;
			if (rb != null) rb.position = newPos;
		}

		Vector3 horiz = velocityWorld; horiz.y = 0f;
		if (horiz.sqrMagnitude > 0.04f)
		{
			Quaternion target = Quaternion.LookRotation(horiz.normalized, Vector3.up);
			Quaternion newRot = Quaternion.RotateTowards(drone.rotation, target, 180f * Time.deltaTime);
			drone.rotation = newRot;
			if (rb != null) rb.rotation = newRot;
		}
	}

	Vector3 ComputePinchThrottleVelocity(out bool active)
	{
		active = false;
		bool indexPinch = rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
		if (!indexPinch)
		{
			pinchEngaged = false;
			pinchHoldTime = 0f;
			return Vector3.zero;
		}

		pinchHoldTime += Time.deltaTime;
		if (!pinchEngaged && pinchHoldTime < engageHoldSeconds) return Vector3.zero;

		Vector3 handRigLocal = cameraRig.InverseTransformPoint(rightHandAnchor.position);
		if (!pinchEngaged)
		{
			anchorRigLocal = handRigLocal;
			pinchEngaged = true;
		}

		Vector3 offset = handRigLocal - anchorRigLocal;
		if (offset.magnitude < deadZone) return Vector3.zero;

		float mag = Mathf.Clamp01(offset.magnitude / fullSpeedAtMeters);
		Vector3 dir = offset.normalized;
		active = true;
		return dir * mag * maxSpeedMps;
	}

	Vector3 ComputePalmDirectionVelocity(out bool active)
	{
		active = false;
		float middlePinch = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
		LastMiddlePinch = middlePinch;
		if (middlePinch < 0.1f) return Vector3.zero;

		Vector3 palmForwardWorld = -rightHandAnchor.up;
		Vector3 palmForwardRigLocal = cameraRig.InverseTransformDirection(palmForwardWorld);
		active = true;
		return palmForwardRigLocal * (middlePinch * maxSpeedMps);
	}

	Vector3 ComputeKeyboardVelocity()
	{
		var kb = Keyboard.current;
		if (kb == null) return Vector3.zero;
		float x = 0f, y = 0f, z = 0f;
		if (kb.wKey.isPressed) z += 1f;
		if (kb.sKey.isPressed) z -= 1f;
		if (kb.aKey.isPressed) x -= 1f;
		if (kb.dKey.isPressed) x += 1f;
		if (kb.qKey.isPressed) y -= 1f;
		if (kb.eKey.isPressed) y += 1f;
		Vector3 v = new Vector3(x, y, z);
		if (v.sqrMagnitude > 1f) v.Normalize();
		return v * keyboardSpeedMps;
	}

	public void ToggleFlightMode()
	{
		flightMode = flightMode == Mode.PinchThrottle ? Mode.PalmDirection : Mode.PinchThrottle;
		Debug.Log($"Flight mode: {flightMode}");
	}

	void HandleFlightModeToggle()
	{
		bool triggered = false;

		if (leftHand != null && leftHand.IsTracked)
		{
			bool ring = leftHand.GetFingerIsPinching(OVRHand.HandFinger.Ring);
			bool index = leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
			bool middle = leftHand.GetFingerIsPinching(OVRHand.HandFinger.Middle);
			bool gesture = ring && !index && !middle;
			if (gesture)
			{
				toggleHoldTime += Time.deltaTime;
				if (!toggleAlreadyTriggered && toggleHoldTime >= modeToggleHoldSeconds)
				{
					triggered = true;
					toggleAlreadyTriggered = true;
				}
			}
			else
			{
				toggleHoldTime = 0f;
				toggleAlreadyTriggered = false;
			}
		}

		if (keyboardFallback && Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
			triggered = true;

		if (triggered) ToggleFlightMode();
	}
}
