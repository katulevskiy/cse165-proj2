using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(parse))]
public class TrackManager : MonoBehaviour
{
	public Transform drone;
	public Transform worldRoot;
	public Material upcomingMaterial;
	public Material activeMaterial;
	public Material completedMaterial;
	public float radiusFeet = 30f;
	public float labelHeightMultiplier = 1.6f;
	public bool shiftWorldToStart = true;

	parse parser;
	List<Vector3> positions;
	Vector3 worldOffset;
	List<GameObject> checkpointObjects = new List<GameObject>();
	int nextIndex;
	float radiusMeters;

	public bool HasActive => positions != null && nextIndex < positions.Count;
	public Vector3 ActivePosition => HasActive ? positions[nextIndex] : Vector3.zero;
	public int ActiveIndex => nextIndex;
	public int TotalCheckpoints => positions?.Count ?? 0;
	public bool Finished => positions != null && nextIndex >= positions.Count;

	int lastClearedIndex = -1;
	public int LastClearedIndex => lastClearedIndex;
	public Vector3 LastClearedPosition => (positions != null && lastClearedIndex >= 0 && lastClearedIndex < positions.Count) ? positions[lastClearedIndex] : (positions != null && positions.Count > 0 ? positions[0] : Vector3.zero);

	public bool IsControlEnabled { get; set; } = true;

	const float FeetToMeters = 0.3048f;

	void Awake()
	{
		parser = GetComponent<parse>();
	}

	void Start()
	{
		radiusMeters = radiusFeet * FeetToMeters;

		if (parser == null || parser.file == null)
		{
			Debug.LogError("TrackManager: parse.file is not assigned.");
			return;
		}

		positions = parser.ParseFile();
		if (positions == null || positions.Count == 0)
		{
			Debug.LogError("TrackManager: track file parsed to zero positions.");
			return;
		}

		if (shiftWorldToStart)
		{
			worldOffset = -positions[0];
			for (int i = 0; i < positions.Count; i++) positions[i] += worldOffset;
			if (worldRoot != null) worldRoot.position += worldOffset;
		}

		SpawnCheckpoints();
		SetActiveCheckpoint(0);
		PositionDroneAtStart();
	}

	void Update()
	{
		if (drone == null || positions == null) return;
		if (nextIndex >= positions.Count) return;

		float d = Vector3.Distance(drone.position, positions[nextIndex]);
		if (d <= radiusMeters)
		{
			OnCheckpointReached(nextIndex);
		}
	}

	void SpawnCheckpoints()
	{
		float diameter = radiusMeters * 2f;
		for (int i = 0; i < positions.Count; i++)
		{
			var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			go.name = $"Checkpoint_{i + 1}";
			go.transform.SetParent(transform, true);
			go.transform.position = positions[i];
			go.transform.localScale = Vector3.one * diameter;

			Destroy(go.GetComponent<Collider>());

			if (upcomingMaterial != null)
				go.GetComponent<Renderer>().sharedMaterial = upcomingMaterial;

			var labelGO = new GameObject("Label");
			labelGO.transform.SetParent(go.transform, false);
			labelGO.transform.localPosition = Vector3.up * labelHeightMultiplier;
			labelGO.transform.localScale = Vector3.one * 0.1f;
			var tm = labelGO.AddComponent<TextMesh>();
			tm.text = (i + 1).ToString();
			tm.fontSize = 80;
			tm.characterSize = 1f;
			tm.alignment = TextAlignment.Center;
			tm.anchor = TextAnchor.MiddleCenter;
			tm.color = Color.white;
			var billboard = labelGO.AddComponent<Billboard>();

			checkpointObjects.Add(go);
		}
	}

	void OnCheckpointReached(int i)
	{
		Debug.Log($"Checkpoint {i + 1} reached.");
		ApplyMaterial(checkpointObjects[i], completedMaterial);
		lastClearedIndex = i;
		nextIndex = i + 1;
		if (nextIndex < positions.Count)
		{
			SetActiveCheckpoint(nextIndex);
		}
		else
		{
			Debug.Log("Training complete - all checkpoints reached.");
		}
	}

	void SetActiveCheckpoint(int i)
	{
		nextIndex = i;
		if (i < checkpointObjects.Count)
			ApplyMaterial(checkpointObjects[i], activeMaterial);
	}

	void ApplyMaterial(GameObject go, Material mat)
	{
		if (mat == null) return;
		var r = go.GetComponent<Renderer>();
		if (r != null) r.sharedMaterial = mat;
	}

	void PositionDroneAtStart()
	{
		if (drone == null || positions.Count == 0) return;
		var rb = drone.GetComponent<Rigidbody>();
		if (!shiftWorldToStart)
		{
			drone.position = positions[0];
			if (rb != null) rb.position = positions[0];
		}
		if (positions.Count >= 2)
		{
			Vector3 toNext = positions[1] - positions[0];
			toNext.y = 0f;
			if (toNext.sqrMagnitude > 0.0001f)
			{
				var q = Quaternion.LookRotation(toNext.normalized, Vector3.up);
				drone.rotation = q;
				if (rb != null) rb.rotation = q;
			}
		}
	}
}
