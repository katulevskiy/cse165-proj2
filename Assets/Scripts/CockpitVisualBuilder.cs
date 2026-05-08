using UnityEngine;

[ExecuteAlways]
public class CockpitVisualBuilder : MonoBehaviour
{
	public Color frameColor = new Color(0.15f, 0.15f, 0.18f, 1f);
	public Color dashColor = new Color(0.08f, 0.1f, 0.12f, 1f);
	public Color glassColor = new Color(0.3f, 0.5f, 0.7f, 0.12f);
	public bool buildOnEnable = true;

	void OnEnable()
	{
		if (buildOnEnable && transform.childCount == 0) Build();
	}

	public void Build()
	{
		ClearChildren();

		AddBar(new Vector3(0f, -0.45f, 0.55f), new Vector3(0.9f, 0.06f, 0.06f), frameColor, "Frame_BottomFront");
		AddBar(new Vector3(0f,  0.45f, 0.55f), new Vector3(0.9f, 0.05f, 0.05f), frameColor, "Frame_TopFront");
		AddBar(new Vector3(-0.45f, 0f, 0.55f), new Vector3(0.05f, 0.95f, 0.05f), frameColor, "Frame_Left");
		AddBar(new Vector3( 0.45f, 0f, 0.55f), new Vector3(0.05f, 0.95f, 0.05f), frameColor, "Frame_Right");

		AddBar(new Vector3(-0.45f, -0.45f, 0f), new Vector3(0.05f, 0.05f, 1.1f), frameColor, "Frame_BL_Strut");
		AddBar(new Vector3( 0.45f, -0.45f, 0f), new Vector3(0.05f, 0.05f, 1.1f), frameColor, "Frame_BR_Strut");

		AddBar(new Vector3(0f, -0.55f, 0.35f), new Vector3(0.85f, 0.12f, 0.42f), dashColor, "Dashboard");

		AddBar(new Vector3(-0.18f, -0.5f, 0.5f), new Vector3(0.12f, 0.04f, 0.04f), new Color(0.1f, 1f, 0.4f), "Indicator_Green");
		AddBar(new Vector3( 0.18f, -0.5f, 0.5f), new Vector3(0.12f, 0.04f, 0.04f), new Color(1f, 0.4f, 0.1f), "Indicator_Orange");

		var glass = GameObject.CreatePrimitive(PrimitiveType.Quad);
		glass.name = "Glass";
		glass.transform.SetParent(transform, false);
		glass.transform.localPosition = new Vector3(0f, 0f, 0.549f);
		glass.transform.localScale = new Vector3(0.88f, 0.88f, 1f);
		var s = Shader.Find("Standard");
		var m = new Material(s) { name = "CockpitGlass" };
		m.SetFloat("_Mode", 3);
		m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
		m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
		m.SetInt("_ZWrite", 0);
		m.EnableKeyword("_ALPHABLEND_ON");
		m.renderQueue = 3000;
		m.color = glassColor;
		glass.GetComponent<Renderer>().sharedMaterial = m;
		DestroyImmediate(glass.GetComponent<Collider>());
	}

	void AddBar(Vector3 localPos, Vector3 localScale, Color col, string n)
	{
		var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
		b.name = n;
		b.transform.SetParent(transform, false);
		b.transform.localPosition = localPos;
		b.transform.localScale = localScale;
		var s = Shader.Find("Standard");
		var m = new Material(s) { name = n + "_Mat" };
		m.color = col;
		b.GetComponent<Renderer>().sharedMaterial = m;
		DestroyImmediate(b.GetComponent<Collider>());
	}

	void ClearChildren()
	{
		for (int i = transform.childCount - 1; i >= 0; i--)
		{
			var c = transform.GetChild(i);
			if (Application.isPlaying) Destroy(c.gameObject);
			else DestroyImmediate(c.gameObject);
		}
	}
}
