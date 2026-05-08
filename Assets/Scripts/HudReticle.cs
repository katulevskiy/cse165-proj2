using UnityEngine;
using UnityEngine.UI;

public class HudReticle : MonoBehaviour
{
	public TrackManager track;
	public RunTimer runTimer;
	public Drone drone;
	public Transform head;
	public float canvasDistance = 1.2f;
	public Vector2 canvasSize = new Vector2(0.6f, 0.6f);
	public float edgeMargin = 0.04f;
	public float onAxisAngleDeg = 4f;
	public Color offAxisColor = new Color(1f, 0.85f, 0.2f, 0.9f);
	public Color onAxisColor = new Color(0.2f, 1f, 0.4f, 1f);
	public Color behindColor = new Color(1f, 0.4f, 0.4f, 0.95f);
	public Color crashColor = new Color(1f, 0.25f, 0.25f, 1f);
	public Font font;

	Canvas canvas;
	RectTransform canvasRect;
	RectTransform chevronRect;
	Text chevronText;
	Text distanceText;
	Text timerText;
	Text bigCenterText;
	float pulseT;

	void Start()
	{
		if (head == null && Camera.main != null) head = Camera.main.transform;
		BuildCanvas();
	}

	void LateUpdate()
	{
		if (head == null || track == null || canvas == null) return;

		Transform t = canvas.transform;
		t.position = head.position + head.forward * canvasDistance;
		t.rotation = head.rotation;

		UpdateTimerAndBigText();

		if (!track.HasActive)
		{
			chevronText.text = string.Empty;
			distanceText.text = string.Empty;
			return;
		}

		Vector3 cp = track.ActivePosition;
		Vector3 toCp = cp - head.position;
		float dist = toCp.magnitude;
		Vector3 local = head.InverseTransformDirection(toCp.normalized);

		float halfW = canvasSize.x * 0.5f - edgeMargin;
		float halfH = canvasSize.y * 0.5f - edgeMargin;
		bool behind = local.z <= 0.01f;

		float angle = Vector3.Angle(head.forward, toCp);
		bool onAxis = !behind && angle <= onAxisAngleDeg;

		Vector2 pos;
		float rotZ;
		if (behind)
		{
			float bx = Mathf.Sign(local.x) * halfW;
			float by = -halfH;
			pos = new Vector2(bx, by);
			rotZ = local.x >= 0f ? 90f : -90f;
		}
		else if (onAxis)
		{
			pos = Vector2.zero;
			rotZ = 0f;
		}
		else
		{
			Vector2 dir = new Vector2(local.x, local.y);
			float dirMag = Mathf.Max(0.0001f, dir.magnitude);
			Vector2 n = dir / dirMag;
			float clampX = Mathf.Abs(n.x) > 0.0001f ? halfW / Mathf.Abs(n.x) : float.PositiveInfinity;
			float clampY = Mathf.Abs(n.y) > 0.0001f ? halfH / Mathf.Abs(n.y) : float.PositiveInfinity;
			float r = Mathf.Min(clampX, clampY);
			pos = n * r;
			rotZ = Mathf.Atan2(-n.x, n.y) * Mathf.Rad2Deg;
		}

		chevronRect.anchoredPosition = pos / Mathf.Max(0.0001f, canvasRect.localScale.x);
		chevronRect.localRotation = Quaternion.Euler(0f, 0f, rotZ);

		Color c = behind ? behindColor : (onAxis ? onAxisColor : offAxisColor);
		if (onAxis)
		{
			pulseT += Time.deltaTime * 4f;
			float pulse = 0.85f + 0.15f * Mathf.Sin(pulseT);
			c.a *= pulse;
			chevronRect.localScale = Vector3.one * (1f + 0.1f * Mathf.Sin(pulseT));
			chevronText.text = "●";
		}
		else
		{
			chevronRect.localScale = Vector3.one;
			chevronText.text = "▲";
		}
		chevronText.color = c;

		distanceText.text = $"P {track.ActiveIndex + 1}/{track.TotalCheckpoints}    {dist:F0} m";
		distanceText.color = c;
	}

	void BuildCanvas()
	{
		var go = new GameObject("HudReticleCanvas");
		go.transform.SetParent(transform, false);

		canvas = go.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.WorldSpace;
		canvas.sortingOrder = 100;
		go.AddComponent<CanvasScaler>();
		go.AddComponent<GraphicRaycaster>();

		canvasRect = go.GetComponent<RectTransform>();
		canvasRect.sizeDelta = new Vector2(1000f, 1000f);
		float worldPerUnit = canvasSize.x / canvasRect.sizeDelta.x;
		canvasRect.localScale = Vector3.one * worldPerUnit;

		if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

		var chevronGO = new GameObject("Chevron");
		chevronGO.transform.SetParent(go.transform, false);
		chevronText = chevronGO.AddComponent<Text>();
		chevronText.text = "▲";
		chevronText.font = font;
		chevronText.fontSize = 140;
		chevronText.alignment = TextAnchor.MiddleCenter;
		chevronText.horizontalOverflow = HorizontalWrapMode.Overflow;
		chevronText.verticalOverflow = VerticalWrapMode.Overflow;
		chevronText.color = offAxisColor;
		chevronRect = chevronGO.GetComponent<RectTransform>();
		chevronRect.sizeDelta = new Vector2(220f, 220f);
		chevronRect.anchoredPosition = Vector2.zero;

		var distGO = new GameObject("Distance");
		distGO.transform.SetParent(go.transform, false);
		distanceText = distGO.AddComponent<Text>();
		distanceText.font = font;
		distanceText.fontSize = 90;
		distanceText.fontStyle = FontStyle.Bold;
		distanceText.alignment = TextAnchor.MiddleCenter;
		distanceText.horizontalOverflow = HorizontalWrapMode.Overflow;
		distanceText.verticalOverflow = VerticalWrapMode.Overflow;
		distanceText.color = offAxisColor;
		var distRect = distGO.GetComponent<RectTransform>();
		distRect.sizeDelta = new Vector2(800f, 140f);
		distRect.anchoredPosition = new Vector2(0f, -440f);

		var timerGO = new GameObject("Timer");
		timerGO.transform.SetParent(go.transform, false);
		timerText = timerGO.AddComponent<Text>();
		timerText.font = font;
		timerText.fontSize = 110;
		timerText.fontStyle = FontStyle.Bold;
		timerText.alignment = TextAnchor.MiddleCenter;
		timerText.horizontalOverflow = HorizontalWrapMode.Overflow;
		timerText.verticalOverflow = VerticalWrapMode.Overflow;
		timerText.color = onAxisColor;
		var timerRect = timerGO.GetComponent<RectTransform>();
		timerRect.sizeDelta = new Vector2(900f, 160f);
		timerRect.anchoredPosition = new Vector2(0f, 410f);

		var bigGO = new GameObject("BigCenter");
		bigGO.transform.SetParent(go.transform, false);
		bigCenterText = bigGO.AddComponent<Text>();
		bigCenterText.font = font;
		bigCenterText.fontSize = 220;
		bigCenterText.alignment = TextAnchor.MiddleCenter;
		bigCenterText.horizontalOverflow = HorizontalWrapMode.Overflow;
		bigCenterText.verticalOverflow = VerticalWrapMode.Overflow;
		bigCenterText.color = new Color(1f, 1f, 1f, 0f);
		bigCenterText.fontStyle = FontStyle.Bold;
		var bigRect = bigGO.GetComponent<RectTransform>();
		bigRect.sizeDelta = new Vector2(800f, 400f);
		bigRect.anchoredPosition = Vector2.zero;
	}

	void UpdateTimerAndBigText()
	{
		if (timerText == null || bigCenterText == null) return;

		bool crashed = drone != null && drone.IsCrashed;
		if (runTimer != null)
		{
			if (runTimer.CurrentPhase == RunTimer.Phase.Countdown)
			{
				int n = Mathf.CeilToInt(runTimer.CountdownRemaining);
				bigCenterText.text = n > 0 ? n.ToString() : "GO!";
				bigCenterText.color = new Color(0.2f, 1f, 0.4f, 0.95f);
				timerText.text = "00:00.000";
				timerText.color = onAxisColor;
				return;
			}
			else if (runTimer.CurrentPhase == RunTimer.Phase.Finished)
			{
				bigCenterText.text = "FINISH";
				bigCenterText.color = onAxisColor;
				timerText.text = "TIME  " + runTimer.FormatElapsed();
				timerText.color = onAxisColor;
				return;
			}
			timerText.text = runTimer.FormatElapsed();
			timerText.color = onAxisColor;
		}

		if (crashed)
		{
			float r = drone.CrashTimeRemaining;
			bigCenterText.text = $"CRASH  {Mathf.Ceil(r)}";
			bigCenterText.color = crashColor;
		}
		else
		{
			bigCenterText.text = string.Empty;
			bigCenterText.color = new Color(1f, 1f, 1f, 0f);
		}
	}
}
