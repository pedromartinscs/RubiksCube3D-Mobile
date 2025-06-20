using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CubeManager : MonoBehaviour
{
    public GameObject cubeletPrefab;
    private const int SIZE = 3;
    private GameObject[,,] cubelets = new GameObject[SIZE, SIZE, SIZE];
    private const float cubeletSpacing = 1.05f;
    private const float faceTolerance = 0.01f;
	private Stack<Move> moveHistory = new Stack<Move>();
	private char[] logicalCube = new char[54];

    private bool isRotating = false;

    void Start() => CreateCube();
	
	public char[] GetLogicalCube() => (char[])logicalCube.Clone();
	
	void Awake()
	{
		var asset = Resources.Load<TextAsset>("TwistSlicePruneTable");
		Kociemba.TwistSlicePrune.LoadFromTextAsset(asset);
	}

	private struct Move
	{
		public string faceName;
		public bool direction;
	
		public Move(string faceName, bool direction)
		{
			this.faceName = faceName;
			this.direction = direction;
		}
	}

    void CreateCube()
    {
        for (int x = 0; x < SIZE; x++)
        {
            for (int y = 0; y < SIZE; y++)
            {
                for (int z = 0; z < SIZE; z++)
                {
                    Vector3Int coords = new Vector3Int(x, y, z);
                    Vector3 position = new Vector3(
                        (x - 1) * cubeletSpacing,
                        (y - 1) * cubeletSpacing,
                        (z - 1) * cubeletSpacing
                    );

                    GameObject cubelet = Instantiate(cubeletPrefab, position, Quaternion.identity, transform);
                    cubelet.name = $"Cubelet_{x}{y}{z}";

                    cubelet.GetComponent<Cubelet>()?.Initialize(coords);
                    cubelet.GetComponent<FaceColorAssigner>()?.ApplyColors();

                    cubelets[x, y, z] = cubelet;
                }
            }
        }
		
		string initial = "UUUUUUUUURRRRRRRRRFFFFFFFFFDDDDDDDDDLLLLLLLLLBBBBBBBBB";
        logicalCube = initial.ToCharArray();
    }
	
	public void RotateLogicalCubeFace(string face, bool clockwise)
    {
        switch (face)
        {
            case "Face_Up": RotateU(clockwise); break;
            case "Face_Down": RotateD(clockwise); break;
            case "Face_Right": RotateL(clockwise); break;    //Right and Left faces are switched the in the detection, so for the logical cube I corrected it
            case "Face_Left": RotateR(clockwise); break;     //Right and Left faces are switched the in the detection, so for the logical cube I corrected it
            case "Face_Forward": RotateF(clockwise); break;
            case "Face_Back": RotateB(clockwise); break;
        }
    }
	
	string[] GetEdgePairs(string cubeState)
	{
		int[][] edgeIndices = new int[][]
		{
			new int[] { 5, 10 },  // UR
			new int[] { 7, 19 },  // UF
			new int[] { 3, 37 },  // UL
			new int[] { 1, 46 },  // UB
			new int[] { 32, 16 }, // DR
			new int[] { 28, 25 }, // DF
			new int[] { 30, 43 }, // DL
			new int[] { 34, 52 }, // DB
			new int[] { 23, 12 }, // FR
			new int[] { 21, 41 }, // FL
			new int[] { 39, 50 }, // BL
			new int[] { 14, 48 }  // BR
		};
	
		List<string> results = new();
		foreach (var pair in edgeIndices)
		{
			results.Add($"{cubeState[pair[0]]}{cubeState[pair[1]]}");
		}
		return results.ToArray();
	}
	
	private void RotateU(bool cw) => RotateFacelet(new int[] {
		0, 1, 2, 3, 4, 5, 6, 7, 8,          // U face
		36, 37, 38,  // L
		45, 46, 47,  // B
		9, 10, 11,   // R
		18, 19, 20   // F
	}, cw);

    private void RotateD(bool cw) => RotateFacelet(new int[] {
		27, 28, 29, 30, 31, 32, 33, 34, 35, // D face
		24, 25, 26,  // F
		15, 16, 17,  // R
		51, 52, 53,  // B
		42, 43, 44   // L
	}, cw);

    private void RotateF(bool cw) => RotateFacelet(new int[] {
		18, 19, 20, 21, 22, 23, 24, 25, 26, // F face
		44, 41, 38,  // L
		29, 28, 27,  // D
		9, 12, 15,   // R
		6, 7, 8      // U
	}, cw);

    private void RotateB(bool cw) => RotateFacelet(new int[] {
        45, 46, 47, 48, 49, 50, 51, 52, 53, // B face
        2, 1, 0,     // U
        36, 39, 42,  // L 
        33, 34, 35,  // D
        17, 14, 11   // R
    }, cw);

    private void RotateL(bool cw) => RotateFacelet(new int[] {
		36, 37, 38, 39, 40, 41, 42, 43, 44, // L face
		0, 3, 6,     // U
		18, 21, 24,  // F
		27, 30, 33,  // D
		53, 50, 47   // B
	}, cw);

    private void RotateR(bool cw) => RotateFacelet(new int[] {
		9, 10, 11, 12, 13, 14, 15, 16, 17,  // R face
		8, 5, 2,    // U
		45, 48, 51, // B
		35, 32, 29, // D
		26, 23, 20  // F
	}, cw);
	
	private void RotateFacelet(int[] indices, bool clockwise)
	{
		char[] copy = (char[])logicalCube.Clone();
	
		// Rotate face itself (standard 3x3 rotation)
		int[] face = indices[..9];
		for (int i = 0; i < 9; i++)
		{
			int fromIndex = !clockwise
				? face[(6 - 3 * (i % 3) + i / 3)]
				: face[(3 * (i % 3) + 2 - i / 3)];
	
			logicalCube[face[i]] = copy[fromIndex];
		}
		
		//Get groups of adjacent facelets (4 groups of 3)
		int[] firstFace = indices[9..12];
		int[] secondFace = indices[12..15];
		int[] thirdFace = indices[15..18];
		int[] fourthFace = indices[18..21];
	
		for (int i = 0; i < 3; i++)
		{
			logicalCube[firstFace[i]] = !clockwise ? copy[secondFace[i]] : copy[fourthFace[i]];
			logicalCube[secondFace[i]] = !clockwise ? copy[thirdFace[i]] : copy[firstFace[i]];
			logicalCube[thirdFace[i]] = !clockwise ? copy[fourthFace[i]] : copy[secondFace[i]];
			logicalCube[fourthFace[i]] = !clockwise ? copy[firstFace[i]] : copy[thirdFace[i]];
		}
	
		//Debug.Log("✅ New logical state: " + new string(logicalCube));
	}

    public void RotateFace(string faceName, bool direction, bool recordMove = true)
    {
        if (isRotating) return;

        List<Transform> faceCubelets = GetCubeletsForFace(faceName);
        if (faceCubelets == null || faceCubelets.Count == 0) return;

        GameObject pivot = new GameObject("RotationPivot");
        pivot.transform.position = GetFaceCenter(faceCubelets);
        pivot.transform.rotation = Quaternion.identity;

        Vector3 rotationAxis = GetRotationAxis(faceName, direction);
        float angle = 90f;

        StartCoroutine(AnimateRotation(pivot.transform, faceCubelets, rotationAxis, angle, faceName, direction));
		
		if (recordMove)
		{
			moveHistory.Push(new Move(faceName, direction));
		}
    }
	
	private bool IsClockwiseRotation(string faceName, string direction)
	{
		return (faceName, direction) switch
		{
			// Horizontal rotation of top and bottom face:
			// Clockwise for U is visually Left → true
			("Face_Up", "Left") => true,
			("Face_Up", "Right") => false,
	
			// Clockwise for D is visually Right → true
			("Face_Down", "Right") => true,
			("Face_Down", "Left") => false,
	
			// Left face: clockwise is visually Down
			("Face_Left", "Down") => true,
			("Face_Left", "Up") => false,
	
			// Right face: clockwise is visually Up
			("Face_Right", "Up") => true,
			("Face_Right", "Down") => false,
	
			// Front face: clockwise is visually Right
			("Face_Forward", "Right") => true,
			("Face_Forward", "Left") => false,
	
			// Back face: clockwise is visually Left (inverted view)
			("Face_Back", "Left") => true,
			("Face_Back", "Right") => false,
	
			_ => true // default fallback
		};
	}

    public List<Transform> GetCubeletsForFace(string faceName)
    {
        List<Transform> faceCubelets = new List<Transform>();

        float half = cubeletSpacing;
        float topY = half, bottomY = -half;
        float leftX = -half, rightX = half;
        float frontZ = half, backZ = -half;

        foreach (var cubelet in Object.FindObjectsByType<Cubelet>(FindObjectsSortMode.None))
        {
            Vector3 pos = cubelet.transform.position;
            bool match = faceName switch
            {
                "Face_Up" => Mathf.Abs(pos.y - topY) < faceTolerance,
                "Face_Down" => Mathf.Abs(pos.y - bottomY) < faceTolerance,
                "Face_Left" => Mathf.Abs(pos.x - leftX) < faceTolerance,
                "Face_Right" => Mathf.Abs(pos.x - rightX) < faceTolerance,
                "Face_Forward" => Mathf.Abs(pos.z - frontZ) < faceTolerance,
                "Face_Back" => Mathf.Abs(pos.z - backZ) < faceTolerance,
                _ => false
            };

            if (match)
                faceCubelets.Add(cubelet.transform);
        }

        return faceCubelets;
    }

    private Vector3 GetRotationAxis(string faceName, bool direction)
    {
        Vector3 axis = faceName switch
        {
            "Face_Up" => Vector3.up,
            "Face_Down" => Vector3.down,
            "Face_Left" => Vector3.left,
            "Face_Right" => Vector3.right,
            "Face_Forward" => Vector3.forward,
            "Face_Back" => Vector3.back,
            _ => Vector3.zero
        };

        return direction ? axis : -axis;
    }

    private Vector3 GetFaceCenter(List<Transform> faceCubelets)
    {
        Vector3 sum = Vector3.zero;
        foreach (var c in faceCubelets) sum += c.position;
        return sum / faceCubelets.Count;
    }

    private IEnumerator AnimateRotation(Transform pivot, List<Transform> cubeletsToRotate, Vector3 axis, float angle, string faceName, bool direction)
    {
        isRotating = true;

        float duration = 0.3f;
        float timeElapsed = 0f;
        float currentAngle = 0f;

        foreach (var cubelet in cubeletsToRotate)
            cubelet.SetParent(pivot);

        while (timeElapsed < duration)
        {
            float step = (angle / duration) * Time.deltaTime;
            pivot.transform.Rotate(axis, step, Space.World);
            currentAngle += step;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        pivot.transform.Rotate(axis, angle - currentAngle, Space.World);

        foreach (var cubelet in cubeletsToRotate)
        {
            cubelet.SetParent(null);
            Vector3 p = cubelet.position;
            cubelet.position = new Vector3(
                Mathf.Round(p.x / cubeletSpacing) * cubeletSpacing,
                Mathf.Round(p.y / cubeletSpacing) * cubeletSpacing,
                Mathf.Round(p.z / cubeletSpacing) * cubeletSpacing
            );
        }

        Destroy(pivot.gameObject);
        RebuildCubeletGrid();
		
		RotateLogicalCubeFace(faceName, direction);
		
		Debug.Log(new string(logicalCube));
		
		string[] edges = GetEdgePairs(new string(logicalCube));
		Debug.Log("🧩 Edge pairs: " + string.Join(", ", edges));
        isRotating = false;

        if (IsCubeSolved())
        {
            Debug.Log("🎉 Cube is solved!");
			string cube = new string(logicalCube);
			if (cube != "UUUUUUUUURRRRRRRRRFFFFFFFFFDDDDDDDDDLLLLLLLLLBBBBBBBBB"){
				Debug.Log("But the string is different...");
			}
        }
    }

    private void RebuildCubeletGrid()
    {
        cubelets = new GameObject[SIZE, SIZE, SIZE];
        foreach (var cubelet in Object.FindObjectsByType<Cubelet>(FindObjectsSortMode.None))
        {
            Vector3Int coords = cubelet.Coordinates;
            if (IsValidCoords(coords))
            {
                cubelets[coords.x, coords.y, coords.z] = cubelet.gameObject;
            }
        }
    }

    public bool IsCubeSolved()
    {
        return IsFaceUniform("Face_Up") &&
               IsFaceUniform("Face_Down") &&
               IsFaceUniform("Face_Left") &&
               IsFaceUniform("Face_Right") &&
               IsFaceUniform("Face_Forward") &&
               IsFaceUniform("Face_Back");
    }

    private bool IsFaceUniform(string faceName)
    {
        float target = cubeletSpacing;
        float tolerance = faceTolerance;

        GameObject centerCubelet = null;
        foreach (var cubelet in Object.FindObjectsByType<Cubelet>(FindObjectsSortMode.None))
        {
            Vector3 pos = cubelet.transform.position;
            bool isCenter = faceName switch
            {
                "Face_Up" => Mathf.Abs(pos.y - target) < tolerance && Mathf.Abs(pos.x) < tolerance && Mathf.Abs(pos.z) < tolerance,
                "Face_Down" => Mathf.Abs(pos.y + target) < tolerance && Mathf.Abs(pos.x) < tolerance && Mathf.Abs(pos.z) < tolerance,
                "Face_Left" => Mathf.Abs(pos.x + target) < tolerance && Mathf.Abs(pos.y) < tolerance && Mathf.Abs(pos.z) < tolerance,
                "Face_Right" => Mathf.Abs(pos.x - target) < tolerance && Mathf.Abs(pos.y) < tolerance && Mathf.Abs(pos.z) < tolerance,
                "Face_Forward" => Mathf.Abs(pos.z - target) < tolerance && Mathf.Abs(pos.x) < tolerance && Mathf.Abs(pos.y) < tolerance,
                "Face_Back" => Mathf.Abs(pos.z + target) < tolerance && Mathf.Abs(pos.x) < tolerance && Mathf.Abs(pos.y) < tolerance,
                _ => false
            };
            if (isCenter)
            {
                centerCubelet = cubelet.gameObject;
                break;
            }
        }

        if (centerCubelet == null)
            return false;

        Transform centerFace = null;
        foreach (Transform face in centerCubelet.transform)
        {
            Vector3 dir = face.forward;
            bool match = faceName switch
            {
                "Face_Up" => Vector3.Dot(dir, Vector3.up) > 0.9f,
                "Face_Down" => Vector3.Dot(dir, Vector3.down) > 0.9f,
                "Face_Left" => Vector3.Dot(dir, Vector3.left) > 0.9f,
                "Face_Right" => Vector3.Dot(dir, Vector3.right) > 0.9f,
                "Face_Forward" => Vector3.Dot(dir, Vector3.forward) > 0.9f,
                "Face_Back" => Vector3.Dot(dir, Vector3.back) > 0.9f,
                _ => false
            };

            if (match)
            {
                centerFace = face;
                break;
            }
        }

        if (centerFace == null)
            return false;

        var centerRenderer = centerFace.GetComponent<MeshRenderer>();
        if (centerRenderer == null)
            return false;

        string referenceMat = centerRenderer.sharedMaterial.name;

        foreach (var cubelet in Object.FindObjectsByType<Cubelet>(FindObjectsSortMode.None))
        {
            Vector3 pos = cubelet.transform.position;
            bool isOnFace = faceName switch
            {
                "Face_Up" => Mathf.Abs(pos.y - target) < tolerance,
                "Face_Down" => Mathf.Abs(pos.y + target) < tolerance,
                "Face_Left" => Mathf.Abs(pos.x + target) < tolerance,
                "Face_Right" => Mathf.Abs(pos.x - target) < tolerance,
                "Face_Forward" => Mathf.Abs(pos.z - target) < tolerance,
                "Face_Back" => Mathf.Abs(pos.z + target) < tolerance,
                _ => false
            };

            if (!isOnFace) continue;

            Transform visibleFace = null;
            foreach (Transform face in cubelet.transform)
            {
                Vector3 dir = face.forward;
                bool match = faceName switch
                {
                    "Face_Up" => Vector3.Dot(dir, Vector3.up) > 0.9f,
                    "Face_Down" => Vector3.Dot(dir, Vector3.down) > 0.9f,
                    "Face_Left" => Vector3.Dot(dir, Vector3.left) > 0.9f,
                    "Face_Right" => Vector3.Dot(dir, Vector3.right) > 0.9f,
                    "Face_Forward" => Vector3.Dot(dir, Vector3.forward) > 0.9f,
                    "Face_Back" => Vector3.Dot(dir, Vector3.back) > 0.9f,
                    _ => false
                };

                if (match)
                {
                    visibleFace = face;
                    break;
                }
            }

            if (visibleFace == null)
                return false;

            var renderer = visibleFace.GetComponent<MeshRenderer>();
            if (renderer == null || renderer.sharedMaterial.name != referenceMat)
                return false;
        }

        return true;
    }

    private bool IsValidCoords(Vector3Int c) => c.x >= 0 && c.x < SIZE && c.y >= 0 && c.y < SIZE && c.z >= 0 && c.z < SIZE;
	
	public bool HasUndo() => moveHistory.Count > 0;
	
	public void UndoLastMove()
	{
		if (isRotating || moveHistory.Count == 0)
			return;
		
		Move lastMove = moveHistory.Pop();
		bool opposite = !lastMove.direction;
		RotateFace(lastMove.faceName, opposite, false); // false = don't re-add to stack
	}
	
	public List<Transform> GetAllCubelets()
	{
		Cubelet[] cubelets = Object.FindObjectsByType<Cubelet>(FindObjectsSortMode.None);
		return cubelets.Select(c => c.transform).ToList();
	}
	
	public List<Transform> GetCubeletsFacing(Vector3 direction, float tolerance = 0.9f)
	{
		List<Transform> matching = new();
	
		foreach (Transform cubelet in GetAllCubelets())
		{
			foreach (Transform face in cubelet)
			{
				if (!face.name.StartsWith("Face_")) continue;
	
				Vector3 worldNormal = face.transform.forward;
				float alignment = Vector3.Dot(worldNormal.normalized, direction.normalized);
	
				if (alignment >= tolerance)
				{
					matching.Add(cubelet);
					break; // only need one face to qualify
				}
			}
		}
	
		return matching;
	}
}
