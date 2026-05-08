using UnityEngine;
using UnityEngine.UI;

public class FlightHud : MonoBehaviour
{
	public FlightController flight;
	public Transform head;
	public Font font;

	[Header("Placement (head-relative)")]
	public float canvasDistance = 0.55f;
	public Vector2 canvasSize = new Vector2(0.42f, 0.25f);
	[Tooltip("Bottom-left offset from the head's forward direction, in metres at the canvas distance.")]
	public Vector2 bottomLeftOffset = new Vector2(-0.42f, -0.40f);

	[Header("Font sizes")]
	public int modeFontSize = 90;
	public int stateFontSize = 70;
	public int controlsFontSize = 56;

	[Header("Colors")]
	public Color modeColor = new Color(0.4f, 1f, 0.6f, 0.95f);
	public Color textColor = new Color(1f, 1f, 1f, 0.85f);
	public Color engagedColor = new Color(1f, 0.9f, 0.2f, 1f);

	Canvas canvas;
	RectTransform canvasRect;
	Text modeText;
	Text controlsText;
	Text stateText;

	void Start()
	{
		if (head == null && Camera.main != null) head = Camera.main.transform;
		BuildCanvas();
	}

	void LateUpdate()
	{
		if (canvas == null || head == null) return;

		// bottom left canv
		Vector3 fwd = head.position + head.forward * canvasDistance;
		Vector3 pos = fwd + head.right * bottomLeftOffset.x + head.up * bottomLeftOffset.y;
		canvas.transform.position = pos;
		canvas.transform.rotation = head.rotation;

		UpdateContent();
	}

	void UpdateContent()
	{
		if (flight == null) return;

		string modeName = flight.CurrentMode == FlightController.Mode.PinchThrottle
			? "PINCH THROTTLE"
			: "PALM DIRECTION";
		modeText.text = $"MODE: {modeName}";
		modeText.color = modeColor;

		// state row
		if (flight.CurrentMode == FlightController.Mode.PinchThrottle)
		{
			if (!flight.RightHandTracked)
				stateText.text = flight.KeyboardDroveLastFrame
					? "Keyboard drive  |  throttle " + Mathf.RoundToInt(flight.NormalizedThrottle * 100) + "%"
					: "Right hand: NOT TRACKED";
			else if (flight.PinchEngaged)
				stateText.text = $"ENGAGED  ●  throttle {Mathf.RoundToInt(flight.NormalizedThrottle * 100)}%";
			else
				stateText.text = "Idle — pinch index ≥ 0.25s to engage";
		}
		else
		{
			if (!flight.RightHandTracked)
				stateText.text = flight.KeyboardDroveLastFrame
					? "Keyboard drive  |  throttle " + Mathf.RoundToInt(flight.NormalizedThrottle * 100) + "%"
					: "Right hand: NOT TRACKED";
			else
			{
				int pct = Mathf.RoundToInt(flight.LastMiddlePinch * 100);
				stateText.text = $"Middle pinch {pct}%  →  fly along palm";
			}
		}
		stateText.color = flight.PinchEngaged || flight.LastMiddlePinch > 0.1f
			? engagedColor : textColor;

		// cheatsheet
		controlsText.text =
			"<b>R hand</b>: index pinch = throttle  |  middle pinch = palm fly\n" +
			"<b>L hand</b>: hold ring = toggle mode  |  hold middle = cycle view\n" +
			"<b>Keys</b>: WASDQE = drive  |  M = mode  |  V = view";
		controlsText.color = textColor;
	}

	void BuildCanvas()
	{
		var go = new GameObject("FlightHudCanvas");
		go.transform.SetParent(transform, false);

		canvas = go.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.WorldSpace;
		canvas.sortingOrder = 95;
		go.AddComponent<CanvasScaler>();
		go.AddComponent<GraphicRaycaster>();

		canvasRect = go.GetComponent<RectTransform>();
		canvasRect.sizeDelta = new Vector2(1100f, 820f);
		float worldPerUnit = canvasSize.x / canvasRect.sizeDelta.x;
		canvasRect.localScale = Vector3.one * worldPerUnit;
		canvasRect.pivot = new Vector2(0f, 0f);

		if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

		// Background panel (semi-transparent)
		var bgGO = new GameObject("Bg");
		bgGO.transform.SetParent(go.transform, false);
		var bg = bgGO.AddComponent<Image>();
		bg.color = new Color(0f, 0f, 0f, 0.45f);
		var bgRect = bgGO.GetComponent<RectTransform>();
		bgRect.anchorMin = new Vector2(0f, 0f);
		bgRect.anchorMax = new Vector2(1f, 1f);
		bgRect.offsetMin = Vector2.zero;
		bgRect.offsetMax = Vector2.zero;

		modeText = AddText(go.transform, "ModeLabel", "MODE: PINCH THROTTLE", modeFontSize, FontStyle.Bold,
			new Vector2(20f, -20f), new Vector2(1060f, 110f),
			TextAnchor.UpperLeft, new Vector2(0f, 1f));
		modeText.horizontalOverflow = HorizontalWrapMode.Overflow; 

		stateText = AddText(go.transform, "StateLabel", "Idle", stateFontSize, FontStyle.Normal,
			new Vector2(20f, -150f), new Vector2(1060f, 90f),
			TextAnchor.UpperLeft, new Vector2(0f, 1f));
		stateText.horizontalOverflow = HorizontalWrapMode.Overflow;

		controlsText = AddText(go.transform, "Controls", "", controlsFontSize, FontStyle.Normal,
			new Vector2(20f, -260f), new Vector2(1060f, 540f),
			TextAnchor.UpperLeft, new Vector2(0f, 1f));
		controlsText.supportRichText = true;
	}

	Text AddText(Transform parent, string name, string content, int size, FontStyle style,
				Vector2 anchoredPos, Vector2 sizeDelta,
				TextAnchor align, Vector2 anchor)
	{
		var go = new GameObject(name);
		go.transform.SetParent(parent, false);
		var t = go.AddComponent<Text>();
		t.text = content;
		t.font = font;
		t.fontSize = size;
		t.fontStyle = style;
		t.alignment = align;
		t.color = textColor;
		t.horizontalOverflow = HorizontalWrapMode.Wrap;
		t.verticalOverflow = VerticalWrapMode.Overflow;
		var rt = go.GetComponent<RectTransform>();
		rt.anchorMin = anchor;
		rt.anchorMax = anchor;
		rt.pivot = anchor;
		rt.sizeDelta = sizeDelta;
		rt.anchoredPosition = anchoredPos;
		return t;
	}
}
