using UnityEngine;

[ExecuteAlways]
public class DroneVisualBuilder : MonoBehaviour
{
	public Color bodyColor = new Color(0.2f, 0.2f, 0.25f, 1f);
	public Color accentColor = new Color(1f, 0.4f, 0.1f, 1f);
	public bool buildOnEnable = true;
	public bool rotateRotors = true;
	public float rotorSpinDeg = 720f;

	GameObject[] rotors;

	void OnEnable()
	{
		if (buildOnEnable && transform.childCount == 0) Build();
	}

	void Update()
	{
		if (!rotateRotors || rotors == null) return;
		for (int i = 0; i < rotors.Length; i++)
		{
			if (rotors[i] == null) continue;
			float dir = (i % 2 == 0) ? 1f : -1f;
			rotors[i].transform.Rotate(0f, rotorSpinDeg * dir * Time.deltaTime, 0f, Space.Self);
		}
	}

	public void Build()
	{
		ClearChildren();

		var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
		body.name = "Body";
		body.transform.SetParent(transform, false);
		body.transform.localScale = new Vector3(0.55f, 0.12f, 0.55f);
		var rendBody = body.GetComponent<Renderer>();
		rendBody.sharedMaterial = MakeMaterial(bodyColor, "Drone_Body");
		DestroyImmediate(body.GetComponent<Collider>());

		var nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
		nose.name = "Nose";
		nose.transform.SetParent(transform, false);
		nose.transform.localPosition = new Vector3(0f, 0f, 0.35f);
		nose.transform.localScale = new Vector3(0.12f, 0.1f, 0.18f);
		nose.GetComponent<Renderer>().sharedMaterial = MakeMaterial(accentColor, "Drone_Nose");
		DestroyImmediate(nose.GetComponent<Collider>());

		Vector3[] armPositions = {
			new Vector3( 0.45f, 0f,  0.35f),
			new Vector3(-0.45f, 0f,  0.35f),
			new Vector3( 0.45f, 0f, -0.35f),
			new Vector3(-0.45f, 0f, -0.35f)
		};
		rotors = new GameObject[4];
		for (int i = 0; i < 4; i++)
		{
			var arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
			arm.name = $"Arm_{i}";
			arm.transform.SetParent(transform, false);
			arm.transform.localPosition = armPositions[i] * 0.5f;
			arm.transform.localScale = new Vector3(0.06f, 0.05f, 0.06f);
			Vector3 dir = armPositions[i].normalized;
			arm.transform.localRotation = Quaternion.LookRotation(dir);
			arm.transform.localScale = new Vector3(0.08f, 0.05f, armPositions[i].magnitude);
			arm.GetComponent<Renderer>().sharedMaterial = MakeMaterial(bodyColor, "Drone_Arm");
			DestroyImmediate(arm.GetComponent<Collider>());

			var rotor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			rotor.name = $"Rotor_{i}";
			rotor.transform.SetParent(transform, false);
			rotor.transform.localPosition = armPositions[i];
			rotor.transform.localScale = new Vector3(0.22f, 0.012f, 0.22f);
			rotor.GetComponent<Renderer>().sharedMaterial = MakeMaterial(accentColor * 0.7f, "Drone_Rotor");
			DestroyImmediate(rotor.GetComponent<Collider>());
			rotors[i] = rotor;
		}
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

	static Material MakeMaterial(Color c, string n)
	{
		var s = Shader.Find("Standard");
		var m = new Material(s) { name = n };
		m.color = c;
		m.SetFloat("_Glossiness", 0.4f);
		return m;
	}
}
