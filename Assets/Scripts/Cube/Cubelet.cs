using UnityEngine;

public class Cubelet : MonoBehaviour
{
    public Vector3Int coordinates; // Logical position in the 3D array

    public void Initialize(Vector3Int coords)
	{
		coordinates = coords;
		gameObject.name = $"Cubelet_{coords.x}_{coords.y}_{coords.z}";
	
		GetComponent<FaceColorAssigner>().AssignColors(coords);
	}
}