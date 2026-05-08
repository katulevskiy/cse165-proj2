using UnityEngine;
using UnityEngine.InputSystem;

public class ViewModeController : MonoBehaviour
{
	public enum Mode { FirstPersonNoCockpit, FirstPersonCockpit, ThirdPersonBehind }

	public Mode CurrentMode { get; private set; } = Mode.FirstPersonNoCockpit;
	public System.Action<Mode> OnModeChanged;

	public OVRHand leftHand;
	public float gestureHoldSeconds = 0.5f;
	public bool keyboardFallback = true;

	float pinchHoldTime;
	bool alreadyTriggeredThisHold;

	void Update()
	{
		bool cycleTriggered = false;

		if (leftHand != null && leftHand.IsTracked)
		{
			bool middle = leftHand.GetFingerIsPinching(OVRHand.HandFinger.Middle);
			bool index = leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
			bool pinch = middle && !index;
			if (pinch)
			{
				pinchHoldTime += Time.deltaTime;
				if (!alreadyTriggeredThisHold && pinchHoldTime >= gestureHoldSeconds)
				{
					cycleTriggered = true;
					alreadyTriggeredThisHold = true;
				}
			}
			else
			{
				pinchHoldTime = 0f;
				alreadyTriggeredThisHold = false;
			}
		}

		if (keyboardFallback && Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame) cycleTriggered = true;

		if (cycleTriggered) Cycle();
	}

	public void Cycle()
	{
		int next = ((int)CurrentMode + 1) % System.Enum.GetValues(typeof(Mode)).Length;
		SetMode((Mode)next);
	}

	public void SetMode(Mode m)
	{
		CurrentMode = m;
		OnModeChanged?.Invoke(m);
		Debug.Log($"View mode: {m}");
	}
}
