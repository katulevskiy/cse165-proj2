using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Drone : MonoBehaviour
{
	public TrackManager track;
	public Transform terrainRoot;
	public float crashLockoutSeconds = 3f;

	public bool IsCrashed { get; private set; }
	public float CrashTimeRemaining { get; private set; }

	void Awake()
	{
		var rb = GetComponent<Rigidbody>();
		rb.isKinematic = true;
		rb.useGravity = false;
		var col = GetComponent<Collider>();
		col.isTrigger = true;
	}

	void OnTriggerEnter(Collider other)
	{
		if (IsCrashed || track == null) return;
		if (terrainRoot != null && !other.transform.IsChildOf(terrainRoot)) return;
		StartCoroutine(HandleCrash());
	}

	IEnumerator HandleCrash()
	{
		IsCrashed = true;
		track.IsControlEnabled = false;
		Debug.Log($"Drone crashed into terrain. Respawning at checkpoint {track.LastClearedIndex + 1}.");

		var rb = GetComponent<Rigidbody>();
		transform.position = track.LastClearedPosition;
		if (rb != null) rb.position = track.LastClearedPosition;
		FaceNextCheckpoint();

		CrashTimeRemaining = crashLockoutSeconds;
		while (CrashTimeRemaining > 0f)
		{
			CrashTimeRemaining -= Time.deltaTime;
			yield return null;
		}
		CrashTimeRemaining = 0f;

		IsCrashed = false;
		track.IsControlEnabled = true;
		Debug.Log("Drone control restored.");
	}

	void FaceNextCheckpoint()
	{
		if (track == null || !track.HasActive) return;
		Vector3 to = track.ActivePosition - transform.position;
		to.y = 0f;
		if (to.sqrMagnitude > 0.0001f)
		{
			var q = Quaternion.LookRotation(to.normalized, Vector3.up);
			transform.rotation = q;
			var rb = GetComponent<Rigidbody>();
			if (rb != null) rb.rotation = q;
		}
	}
}
