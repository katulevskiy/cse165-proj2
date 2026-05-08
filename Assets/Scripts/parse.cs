using System.Collections.Generic;
using UnityEngine;

public class parse : MonoBehaviour
{
	public TextAsset file;

	public List<Vector3> ParseFile()
	{
		float ScaleFactor = 1.0f / 39.37f;
		List<Vector3> positions = new List<Vector3>();
		string content = file.text;
		string[] lines = content.Split('\n');
		for (int i = 0; i < lines.Length; i++)
		{
			if (string.IsNullOrWhiteSpace(lines[i])) continue;
			string[] coords = lines[i].Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
			Vector3 pos = new Vector3(float.Parse(coords[0]), float.Parse(coords[1]), float.Parse(coords[2]));
			positions.Add(pos * ScaleFactor);
		}
		return positions;
	}
}
