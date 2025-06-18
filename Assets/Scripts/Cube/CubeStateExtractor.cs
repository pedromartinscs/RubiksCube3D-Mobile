using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public class CubeStateExtractor : MonoBehaviour
{
    public CubeManager cubeManager;

    public string GetCubeStateString()
	{
		string result = "";
		float tolerance = 0.1f;
		
		Dictionary<char, char> faceRemap = new()
		{
			['F'] = 'F',
			['B'] = 'B',
			['L'] = 'R',
			['R'] = 'L',
			['U'] = 'U',
			['D'] = 'D',
		};
	
		var faces = new List<(string faceCode, Func<Vector3, bool>, Vector3, Func<Vector3, IComparable>, Func<Vector3, IComparable>)>
		{
			// U = actual bottom
			("U", pos => Mathf.Abs(pos.y + 1.05f) < tolerance, Vector3.down,    pos => -pos.z, pos => pos.x),
			
			// L = actual right
			("L", pos => Mathf.Abs(pos.x - 1.05f) < tolerance, Vector3.right,   pos => -pos.y, pos => -pos.z),
		
			// F = actual back
			("F", pos => Mathf.Abs(pos.z + 1.05f) < tolerance, Vector3.back,    pos => -pos.y, pos => -pos.x),
		
			// D = actual top
			("D", pos => Mathf.Abs(pos.y - 1.05f) < tolerance, Vector3.up,      pos => pos.z, pos => pos.x),
			
			// R = actual left
			("R", pos => Mathf.Abs(pos.x + 1.05f) < tolerance, Vector3.left,    pos => -pos.y, pos => pos.z),
		
			// B = actual front
			("B", pos => Mathf.Abs(pos.z - 1.05f) < tolerance, Vector3.forward, pos => -pos.y, pos => pos.x),
		};
	
		foreach (var (faceCode, isOnFace, normal, primarySort, secondarySort) in faces)
		{
			List<(Transform facelet, Vector3 pos)> selected = new();
	
			foreach (Transform cubelet in cubeManager.GetAllCubelets())
			{
				if (!isOnFace(cubelet.position)) continue;
	
				Transform matchedFace = null;
				foreach (Transform face in cubelet)
				{
					if (!face.name.StartsWith("Face_")) continue;
					if (Vector3.Dot(face.forward.normalized, normal.normalized) > 0.9f)
					{
						matchedFace = face;
						break;
					}
				}
	
				if (matchedFace != null)
					selected.Add((matchedFace, cubelet.position));
			}
	
			if (selected.Count != 9)
			{
				Debug.LogError($"❌ {faceCode}-face has {selected.Count} facelets instead of 9.");
				return "";
			}
	
			selected = selected
				.OrderBy(f => primarySort(f.pos))
				.ThenBy(f => secondarySort(f.pos))
				.ToList();
	
			foreach (var (facelet, _) in selected)
			{
				var mat = facelet.GetComponent<Renderer>().sharedMaterial;
				string colorName = mat.name.Replace(" (Instance)", "");
				char c = ColorNameToChar(colorName, '?');
				result += c;
			}
		}
	
		string corrected = new string(result.Select(c => faceRemap[c]).ToArray());
		
		Debug.Log("🧩 Final CubeState: " + result);
		var counts = result.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
		foreach (var kv in counts)
			Debug.Log($"🔢 {kv.Key}: {kv.Value}");
	
		return corrected;
	}

    private Transform GetMatchingFace(Transform cubelet, string faceName)
    {
        foreach (Transform face in cubelet)
        {
            if (face.name == faceName)
                return face;
        }
        return null;
    }

    private char ColorNameToChar(string name, char fallback = '?')
	{
		return name.ToLower() switch
		{
			"mat_white" => 'U',
			"mat_yellow" => 'D',
			"mat_red" => 'R',
			"mat_orange" => 'L',
			"mat_blue" => 'B',
			"mat_green" => 'F',
			_ => fallback
		};
	}
}
