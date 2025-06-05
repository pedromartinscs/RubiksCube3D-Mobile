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

    private bool isRotating = false;

    void Start() => CreateCube();

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

    public void RotateFace(string faceName, string direction)
    {
        if (isRotating) return;

        Debug.Log($"Rotating {faceName} face to the {direction}");

        List<Transform> faceCubelets = GetCubeletsForFace(faceName);
        if (faceCubelets == null || faceCubelets.Count == 0)
        {
            Debug.LogWarning("❌ Face cubelets not found.");
            return;
        }

        GameObject pivot = new GameObject("RotationPivot");
        pivot.transform.position = GetFaceCenter(faceCubelets);
        pivot.transform.rotation = Quaternion.identity;

        Vector3 rotationAxis = GetRotationAxis(faceName, direction);
        float angle = 90f;

        StartCoroutine(AnimateRotation(pivot.transform, faceCubelets, rotationAxis, angle));
    }

    private List<Transform> GetCubeletsForFace(string faceName)
    {
        List<Transform> faceCubelets = new List<Transform>();

        float half = cubeletSpacing;
        float topY = half, bottomY = -half;
        float leftX = -half, rightX = half;
        float frontZ = half, backZ = -half;

        foreach (var cubelet in FindObjectsOfType<Cubelet>())
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
        Debug.Log("✅ Rotation complete. Grid rebuilt.");
    }

    private void RebuildCubeletGrid()
    {
        cubelets = new GameObject[SIZE, SIZE, SIZE];
        foreach (var cubelet in FindObjectsOfType<Cubelet>())
        {
            Vector3Int coords = cubelet.Coordinates;
            if (IsValidCoords(coords))
            {
                cubelets[coords.x, coords.y, coords.z] = cubelet.gameObject;
                cubelet.GetComponent<FaceColorAssigner>()?.ApplyColors();
            }
        }
    }

    private bool IsValidCoords(Vector3Int c) => c.x >= 0 && c.x < SIZE && c.y >= 0 && c.y < SIZE && c.z >= 0 && c.z < SIZE;

    private int CountFilledCubelets()
    {
        int count = 0;
        for (int x = 0; x < SIZE; x++)
            for (int y = 0; y < SIZE; y++)
                for (int z = 0; z < SIZE; z++)
                    if (cubelets[x, y, z] != null)
                        count++;
        return count;
    }
}
