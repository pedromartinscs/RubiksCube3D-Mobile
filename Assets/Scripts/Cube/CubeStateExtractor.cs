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
		float tolerance = 0.1f; // Allow for minor floating-point imprecision
	
		var faceInfo = new List<(string name, Vector3 normal, Func<Vector3, bool> isOnFace)>
		{
			("Face_Up", Vector3.up, pos => Mathf.Abs(pos.y - 1.05f) < tolerance),
			("Face_Left", Vector3.left, pos => Mathf.Abs(pos.x + 1.05f) < tolerance),
			("Face_Forward", Vector3.forward, pos => Mathf.Abs(pos.z - 1.05f) < tolerance),
			("Face_Right", Vector3.right, pos => Mathf.Abs(pos.x - 1.05f) < tolerance),
			("Face_Back", Vector3.back, pos => Mathf.Abs(pos.z + 1.05f) < tolerance),
			("Face_Down", Vector3.down, pos => Mathf.Abs(pos.y + 1.05f) < tolerance),
		};
	
		foreach (var (faceName, normal, isOnFace) in faceInfo)
		{
			List<(Transform facelet, Vector3 pos)> selected = new();
	
			foreach (Transform cubelet in cubeManager.GetAllCubelets())
			{
				if (!isOnFace(cubelet.position)) continue;
	
				foreach (Transform child in cubelet)
				{
					if (!child.name.StartsWith("Face_")) continue;
	
					float alignment = Vector3.Dot(child.forward.normalized, normal.normalized);
					if (alignment > 0.99f)
					{
						selected.Add((child, cubelet.position));
						break; // only one facelet per cubelet
					}
				}
			}
	
			if (selected.Count != 9)
			{
				Debug.LogError($"❌ Face {faceName} has {selected.Count} valid facelets instead of 9.");
				return "";
			}
	
			selected = selected
				.OrderBy(f => -f.pos.y)  // top to bottom
				.ThenBy(f => f.pos.x)    // left to right
				.ToList();
	
			foreach (var (facelet, _) in selected)
			{
				var mat = facelet.GetComponent<Renderer>().sharedMaterial;
				string colorName = mat.name.Replace(" (Instance)", "");
				result += ColorNameToChar(colorName, '?');
			}
		}
	
		Debug.Log("🧩 Final CubeState: " + result);
		return result;
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
			"mat_blue" => 'F',
			"mat_green" => 'B',
			_ => fallback
		};
	}
}
