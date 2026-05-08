using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WorldBeacon : MonoBehaviour
{
	public TrackManager track;
	public float beamHeight = 200f;
	public float beamRadius = 0.6f;
	public Color beamColor = new Color(0.2f, 1f, 0.4f, 0.55f);

	public float fadeStartDistance = 30f;
	public float fadeEndDistance = 100f;
	public float minAlphaAtClose = 0.05f;

	Renderer rend;
	MaterialPropertyBlock mpb;
	Transform drone;

	void Awake()
	{
		rend = GetComponent<Renderer>();
		mpb = new MaterialPropertyBlock();
	}

	void Start()
	{
		if (track != null) drone = track.drone;
		transform.localScale = new Vector3(beamRadius * 2f, beamHeight * 0.5f, beamRadius * 2f);
	}

	void LateUpdate()
	{
		if (track == null || !track.HasActive)
		{
			rend.enabled = false;
			return;
		}
		rend.enabled = true;

		Vector3 cp = track.ActivePosition;
		transform.position = cp + Vector3.up * (beamHeight * 0.5f);

		float a = beamColor.a;
		if (drone != null && fadeEndDistance > fadeStartDistance)
		{
			float dist = Vector3.Distance(drone.position, cp);
			float k = Mathf.InverseLerp(fadeStartDistance, fadeEndDistance, dist);
			a = Mathf.Lerp(minAlphaAtClose, beamColor.a, k);
		}

		var c = beamColor; c.a = a;
		rend.GetPropertyBlock(mpb);
		mpb.SetColor("_Color", c);
		mpb.SetColor("_EmissionColor", new Color(c.r * 1.2f, c.g * 1.2f, c.b * 1.2f) * a);
		rend.SetPropertyBlock(mpb);
	}
}
