using UnityEngine;

public class Cubelet : MonoBehaviour
{
    public Vector3Int Coordinates { get; private set; }

    public void Initialize(Vector3Int coords)
    {
        Coordinates = coords;
    }
}
