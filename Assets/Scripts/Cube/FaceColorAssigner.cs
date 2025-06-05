using UnityEngine;

public class FaceColorAssigner : MonoBehaviour
{
    public Material upMat, downMat, leftMat, rightMat, frontMat, backMat;

    public void AssignColors(Vector3Int coords)
    {
        foreach (Transform child in transform)
        {
            switch (child.name)
            {
                case "Face_Up":
                    child.gameObject.SetActive(coords.y == 2);
                    if (coords.y == 2) child.GetComponent<Renderer>().material = upMat;
                    break;
                case "Face_Down":
                    child.gameObject.SetActive(coords.y == 0);
                    if (coords.y == 0) child.GetComponent<Renderer>().material = downMat;
                    break;
                case "Face_Left":
                    child.gameObject.SetActive(coords.x == 0);
                    if (coords.x == 0) child.GetComponent<Renderer>().material = leftMat;
                    break;
                case "Face_Right":
                    child.gameObject.SetActive(coords.x == 2);
                    if (coords.x == 2) child.GetComponent<Renderer>().material = rightMat;
                    break;
                case "Face_Forward":
                    child.gameObject.SetActive(coords.z == 2);
                    if (coords.z == 2) child.GetComponent<Renderer>().material = frontMat;
                    break;
                case "Face_Back":
                    child.gameObject.SetActive(coords.z == 0);
                    if (coords.z == 0) child.GetComponent<Renderer>().material = backMat;
                    break;
            }
        }
    }
}
