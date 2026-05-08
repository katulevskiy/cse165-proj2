using UnityEngine;

public class RunTimer : MonoBehaviour
{
	public TrackManager track;
	public Drone drone;
	public float startCountdownSeconds = 3f;

	public enum Phase { Countdown, Running, Finished }
	public Phase CurrentPhase { get; private set; }
	public float ElapsedSeconds { get; private set; }
	public float CountdownRemaining { get; private set; }

	void Start()
	{
		StartInitialCountdown();
	}

	void Update()
	{
		if (CurrentPhase == Phase.Countdown)
		{
			CountdownRemaining -= Time.deltaTime;
			if (CountdownRemaining <= 0f)
			{
				CountdownRemaining = 0f;
				StartRun();
			}
		}
		else if (CurrentPhase == Phase.Running)
		{
			ElapsedSeconds += Time.deltaTime;
			if (track != null && track.Finished)
			{
				CurrentPhase = Phase.Finished;
				if (track != null) track.IsControlEnabled = false;
			}
		}
	}

	void StartInitialCountdown()
	{
		CurrentPhase = Phase.Countdown;
		CountdownRemaining = startCountdownSeconds;
		ElapsedSeconds = 0f;
		if (track != null) track.IsControlEnabled = false;
	}

	void StartRun()
	{
		CurrentPhase = Phase.Running;
		if (track != null) track.IsControlEnabled = true;
	}

	public string FormatElapsed()
	{
		int m = (int)(ElapsedSeconds / 60f);
		float s = ElapsedSeconds - m * 60f;
		return $"{m:D2}:{s:00.000}";
	}
}
