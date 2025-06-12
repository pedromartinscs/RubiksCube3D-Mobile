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

    private bool isRotating = false;

    void Start() => CreateCube();

	private struct Move
	{
		public string faceName;
		public string direction;
	
		public Move(string faceName, string direction)
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
    }

    public void RotateFace(string faceName, string direction, bool recordMove = true)
    {
        if (isRotating) return;

        List<Transform> faceCubelets = GetCubeletsForFace(faceName);
        if (faceCubelets == null || faceCubelets.Count == 0) return;

        GameObject pivot = new GameObject("RotationPivot");
        pivot.transform.position = GetFaceCenter(faceCubelets);
        pivot.transform.rotation = Quaternion.identity;

        Vector3 rotationAxis = GetRotationAxis(faceName, direction);
        float angle = 90f;

        StartCoroutine(AnimateRotation(pivot.transform, faceCubelets, rotationAxis, angle));
		
		if (recordMove)
		{
			moveHistory.Push(new Move(faceName, direction));
		}
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

    private Vector3 GetRotationAxis(string faceName, string direction)
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

        bool reverse = direction == "Right" || direction == "Down";
        return reverse ? -axis : axis;
    }

    private Vector3 GetFaceCenter(List<Transform> faceCubelets)
    {
        Vector3 sum = Vector3.zero;
        foreach (var c in faceCubelets) sum += c.position;
        return sum / faceCubelets.Count;
    }

    private IEnumerator AnimateRotation(Transform pivot, List<Transform> cubeletsToRotate, Vector3 axis, float angle)
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

        isRotating = false;

        if (IsCubeSolved())
        {
            Debug.Log("🎉 Cube is solved!");
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
		string opposite = GetOppositeDirection(lastMove.direction);
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
	
	private string GetOppositeDirection(string direction)
	{
		return direction switch
		{
			"Left" => "Right",
			"Right" => "Left",
			"Up" => "Down",
			"Down" => "Up",
			_ => direction
		};
	}
}
