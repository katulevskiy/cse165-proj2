using UnityEngine;
using UnityEngine.UI;

public class FlightVisualizer : MonoBehaviour
{
	public FlightController flight;
	public Transform head;
	public Font font;

	[Header("PinchThrottle visuals")]
	public Color anchorColor = new Color(0.2f, 0.8f, 1f, 0.9f);
	public Color vectorColor = new Color(1f, 0.85f, 0.2f, 1f);
	public float anchorSphereRadius = 0.02f;
	public float vectorLineWidth = 0.006f;

	[Header("PalmDirection visuals")]
	public Color palmRayColor = new Color(0.4f, 1f, 0.6f, 1f);
	public float palmRayBaseLength = 0.25f;
	public float palmRayMaxLength = 0.8f;

	[Header("Pinch label (mode 2)")]
	public float labelDistanceFromHand = 0.12f;
	public Vector2 labelCanvasSize = new Vector2(0.18f, 0.06f);

	GameObject anchorSphere;
	LineRenderer pinchVector;
	LineRenderer palmRay;
	GameObject palmRayHead;
	Canvas pinchLabelCanvas;
	Text pinchLabelText;

	void Start()
	{
		if (head == null && Camera.main != null) head = Camera.main.transform;
		BuildVisuals();
	}

	void LateUpdate()
	{
		if (flight == null) { HideAll(); return; }

		bool pinchMode = flight.CurrentMode == FlightController.Mode.PinchThrottle;
		bool palmMode = flight.CurrentMode == FlightController.Mode.PalmDirection;

		bool showPinch = pinchMode && flight.PinchEngaged && flight.RightHandTracked;
		anchorSphere.SetActive(showPinch);
		pinchVector.enabled = showPinch;
		if (showPinch)
		{
			Vector3 a = flight.AnchorWorldPosition;
			Vector3 h = flight.HandWorldPosition;
			anchorSphere.transform.position = a;
			anchorSphere.transform.localScale = Vector3.one * (anchorSphereRadius * 2f);

			pinchVector.SetPosition(0, a);
			pinchVector.SetPosition(1, h);
			float t = Mathf.Clamp01(flight.NormalizedThrottle);
			Color c = Color.Lerp(new Color(1f, 1f, 1f, 0.7f), vectorColor, t);
			pinchVector.startColor = c;
			pinchVector.endColor = c;
			pinchVector.startWidth = vectorLineWidth;
			pinchVector.endWidth = vectorLineWidth * (1f + 1.5f * t);
		}

		bool showPalm = palmMode && flight.RightHandTracked && flight.LastMiddlePinch > 0.01f;
		palmRay.enabled = showPalm;
		palmRayHead.SetActive(showPalm);
		pinchLabelCanvas.gameObject.SetActive(showPalm);
		if (showPalm)
		{
			float pinch = Mathf.Clamp01(flight.LastMiddlePinch);
			Vector3 origin = flight.HandWorldPosition;
			Vector3 dir = flight.LastPalmForwardWorld.normalized;
			float length = Mathf.Lerp(palmRayBaseLength, palmRayMaxLength, pinch);
			Vector3 tip = origin + dir * length;

			palmRay.SetPosition(0, origin);
			palmRay.SetPosition(1, tip);
			Color c = palmRayColor;
			c.a = Mathf.Lerp(0.5f, 1f, pinch);
			palmRay.startColor = c;
			palmRay.endColor = c;
			palmRay.startWidth = 0.006f + 0.012f * pinch;
			palmRay.endWidth = 0.002f;

			palmRayHead.transform.position = tip;
			palmRayHead.transform.localScale = Vector3.one * (0.018f + 0.025f * pinch);

			Vector3 labelPos = origin + Vector3.up * labelDistanceFromHand;
			pinchLabelCanvas.transform.position = labelPos;
			if (head != null)
			{
				Vector3 toHead = head.position - labelPos;
				if (toHead.sqrMagnitude > 0.0001f)
					pinchLabelCanvas.transform.rotation = Quaternion.LookRotation(-toHead.normalized, Vector3.up);
			}
			pinchLabelText.text = $"THROTTLE {Mathf.RoundToInt(pinch * 100)}%";
			pinchLabelText.color = c;
		}
	}

	void HideAll()
	{
		if (anchorSphere != null) anchorSphere.SetActive(false);
		if (pinchVector != null) pinchVector.enabled = false;
		if (palmRay != null) palmRay.enabled = false;
		if (palmRayHead != null) palmRayHead.SetActive(false);
		if (pinchLabelCanvas != null) pinchLabelCanvas.gameObject.SetActive(false);
	}

	void BuildVisuals()
	{
		anchorSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		anchorSphere.name = "PinchAnchorMarker";
		anchorSphere.transform.SetParent(transform, false);
		Destroy(anchorSphere.GetComponent<Collider>());
		var sMat = new Material(Shader.Find("Sprites/Default"));
		sMat.color = anchorColor;
		anchorSphere.GetComponent<MeshRenderer>().material = sMat;
		anchorSphere.SetActive(false);

		pinchVector = MakeLine("PinchVectorLine");

		palmRay = MakeLine("PalmRayLine");

		palmRayHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		palmRayHead.name = "PalmRayHead";
		palmRayHead.transform.SetParent(transform, false);
		Destroy(palmRayHead.GetComponent<Collider>());
		var hMat = new Material(Shader.Find("Sprites/Default"));
		hMat.color = palmRayColor;
		palmRayHead.GetComponent<MeshRenderer>().material = hMat;
		palmRayHead.SetActive(false);

		BuildPinchLabel();
	}

	LineRenderer MakeLine(string n)
	{
		var go = new GameObject(n);
		go.transform.SetParent(transform, false);
		var lr = go.AddComponent<LineRenderer>();
		lr.material = new Material(Shader.Find("Sprites/Default"));
		lr.useWorldSpace = true;
		lr.positionCount = 2;
		lr.startWidth = vectorLineWidth;
		lr.endWidth = vectorLineWidth;
		lr.numCapVertices = 4;
		lr.enabled = false;
		return lr;
	}

	void BuildPinchLabel()
	{
		var go = new GameObject("PinchLabelCanvas");
		go.transform.SetParent(transform, false);
		pinchLabelCanvas = go.AddComponent<Canvas>();
		pinchLabelCanvas.renderMode = RenderMode.WorldSpace;
		pinchLabelCanvas.sortingOrder = 90;
		go.AddComponent<CanvasScaler>();

		var rt = go.GetComponent<RectTransform>();
		rt.sizeDelta = new Vector2(600f, 200f);
		float worldPerUnit = labelCanvasSize.x / rt.sizeDelta.x;
		rt.localScale = Vector3.one * worldPerUnit;

		if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

		var tGO = new GameObject("Text");
		tGO.transform.SetParent(go.transform, false);
		pinchLabelText = tGO.AddComponent<Text>();
		pinchLabelText.text = "THROTTLE 0%";
		pinchLabelText.font = font;
		pinchLabelText.fontSize = 110;
		pinchLabelText.fontStyle = FontStyle.Bold;
		pinchLabelText.alignment = TextAnchor.MiddleCenter;
		pinchLabelText.horizontalOverflow = HorizontalWrapMode.Overflow;
		pinchLabelText.verticalOverflow = VerticalWrapMode.Overflow;
		pinchLabelText.color = palmRayColor;
		var trt = tGO.GetComponent<RectTransform>();
		trt.sizeDelta = rt.sizeDelta;
		trt.anchoredPosition = Vector2.zero;

		go.SetActive(false);
	}
}
