using System.Collections.Generic;
using UnityEngine;

public class CubeStateExtractor : MonoBehaviour
{
    public CubeManager cubeManager;

    public string GetCubeStateString()
    {
        Dictionary<string, char> faceLetterMap = new()
        {
            { "Face_Up", 'U' },
            { "Face_Right", 'R' },
            { "Face_Forward", 'F' },
            { "Face_Down", 'D' },
            { "Face_Left", 'L' },
            { "Face_Back", 'B' },
        };

        string[] faceNames = { "Face_Up", "Face_Right", "Face_Forward", "Face_Down", "Face_Left", "Face_Back" };
        string result = "";

        foreach (string faceName in faceNames)
        {
            List<(Vector2, char)> facelets = new();

            foreach (Transform cubelet in cubeManager.GetCubeletsForFace(faceName))
            {
                Transform face = GetMatchingFace(cubelet, faceName);
                if (face == null) continue;

                Renderer renderer = face.GetComponent<Renderer>();
                if (renderer == null || renderer.sharedMaterial == null) continue;

                string colorName = renderer.sharedMaterial.name.Replace(" (Instance)", "");
                char colorChar = ColorNameToChar(colorName, faceLetterMap[faceName]);

                // Project facelet into 2D for consistent sorting
                Vector3 pos = face.localPosition;
                Vector2 flat = faceName switch
                {
                    "Face_Up" or "Face_Down" => new Vector2(pos.x, -pos.z),
                    "Face_Left" or "Face_Right" => new Vector2(pos.z, -pos.y),
                    "Face_Forward" or "Face_Back" => new Vector2(pos.x, -pos.y),
                    _ => Vector2.zero
                };

                facelets.Add((flat, colorChar));
            }

            // Sort in row-major (top-left to bottom-right)
            facelets.Sort((a, b) =>
            {
                int rowComp = a.Item1.y.CompareTo(b.Item1.y);
                return rowComp != 0 ? rowComp : a.Item1.x.CompareTo(b.Item1.x);
            });

            foreach (var (_, ch) in facelets)
                result += ch;
        }

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

    private char ColorNameToChar(string colorName, char fallback)
    {
        return colorName switch
        {
            "White" => 'U',
            "Yellow" => 'D',
            "Green" => 'F',
            "Blue" => 'B',
            "Red" => 'R',
            "Orange" => 'L',
            _ => fallback
        };
    }
}
