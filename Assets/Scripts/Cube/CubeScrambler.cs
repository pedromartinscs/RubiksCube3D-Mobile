using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeScrambler : MonoBehaviour
{
    public CubeManager cubeManager; // Link to your existing CubeManager
    public int scrambleMoves = 20;  // Number of random moves to apply

    private bool isScrambling = false;

    public void Scramble()
    {
        if (!isScrambling)
        {
            StartCoroutine(ScrambleRoutine());
        }
    }

    private IEnumerator ScrambleRoutine()
    {
        isScrambling = true;

        string[] faceNames = { "Face_Up", "Face_Down", "Face_Left", "Face_Right", "Face_Forward", "Face_Back" };
        bool[] directions = {true, false}; // These must match your CubeManager logic

        for (int i = 0; i < scrambleMoves; i++)
        {
            string face = faceNames[Random.Range(0, faceNames.Length)];
            bool dir = directions[Random.Range(0, directions.Length)];

            cubeManager.RotateFace(face, dir);

            yield return new WaitForSeconds(0.15f); // Wait between moves for animation clarity
        }

        isScrambling = false;
		Debug.Log("Post-scramble check: " + new string(cubeManager.GetLogicalCube()));
    }
}
