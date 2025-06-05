using UnityEngine;

public class CubeManager : MonoBehaviour
{
    public GameObject cubeletPrefab;

    private const int SIZE = 3;
    private GameObject[,,] cubelets = new GameObject[SIZE, SIZE, SIZE];

    void Start()
    {
        CreateCube();
    }

    void CreateCube()
    {
        float spacing = 1.05f; // Slight space between cubelets

        for (int x = 0; x < SIZE; x++)
        {
            for (int y = 0; y < SIZE; y++)
            {
                for (int z = 0; z < SIZE; z++)
                {
                    Vector3Int coords = new Vector3Int(x, y, z);
                    Vector3 position = new Vector3(
                        (x - 1) * spacing,
                        (y - 1) * spacing,
                        (z - 1) * spacing
                    );

                    GameObject cubelet = Instantiate(cubeletPrefab, position, Quaternion.identity, transform);
                    cubelet.GetComponent<Cubelet>().Initialize(coords);
                    cubelets[x, y, z] = cubelet;
                }
            }
        }
    }
}