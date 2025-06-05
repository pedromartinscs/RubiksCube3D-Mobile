using UnityEngine;

public class FaceColorAssigner : MonoBehaviour
{
    public Material upMaterial;
    public Material downMaterial;
    public Material leftMaterial;
    public Material rightMaterial;
    public Material forwardMaterial;
    public Material backMaterial;

    public void ApplyColors()
    {
        foreach (Transform face in transform)
        {
            if (face.name == "Face_Up" && upMaterial != null)
                ApplyMaterial(face, upMaterial);
            else if (face.name == "Face_Down" && downMaterial != null)
                ApplyMaterial(face, downMaterial);
            else if (face.name == "Face_Left" && leftMaterial != null)
                ApplyMaterial(face, leftMaterial);
            else if (face.name == "Face_Right" && rightMaterial != null)
                ApplyMaterial(face, rightMaterial);
            else if (face.name == "Face_Forward" && forwardMaterial != null)
                ApplyMaterial(face, forwardMaterial);
            else if (face.name == "Face_Back" && backMaterial != null)
                ApplyMaterial(face, backMaterial);
        }
    }

    private void ApplyMaterial(Transform face, Material mat)
    {
        var renderer = face.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = mat;
        }
    }
}
