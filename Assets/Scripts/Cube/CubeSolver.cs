using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CubeSolver : MonoBehaviour
{
    public CubeStateExtractor extractor;
    public CubeManager cubeManager;

    public float moveDelay = 0.3f; // Delay between moves

    // Entry point for Solve button
    public void SolveCube()
    {
        string state = extractor.GetCubeStateString();
        if (state.Length != 54)
        {
            Debug.LogError("❌ Invalid cube state length: " + state);
            return;
        }
		Debug.Log("🧩 CubeState: " + state);
        string solution = KociembaSolver.Solve(state, maxDepth: 21, timeOut: 5, useSeparator: false);
        if (solution == null)
		{
			Debug.LogError("❌ Solver returned no solution.");
			return;
		}
		
		if (solution == "")
		{
			Debug.Log("✅ Cube is already solved.");
			return;
		}

        Debug.Log("✅ Solver Output: " + solution);
        List<(string face, string direction)> parsedMoves = ParseSolution(solution);
        StartCoroutine(ExecuteMoves(parsedMoves));
    }

    private List<(string, string)> ParseSolution(string moves)
    {
        var list = new List<(string, string)>();
        string[] parts = moves.Trim().Split(' ');

        foreach (string move in parts)
        {
            string face = move[0] switch
            {
                'U' => "Face_Up",
                'D' => "Face_Down",
                'L' => "Face_Left",
                'R' => "Face_Right",
                'F' => "Face_Forward",
                'B' => "Face_Back",
                _ => null
            };

            if (face == null) continue;

            string direction = "Right"; // Default is clockwise
            if (move.Length > 1)
            {
                if (move[1] == '\'') direction = "Left";
                else if (move[1] == '2')
                {
                    list.Add((face, "Right"));
                    list.Add((face, "Right"));
                    continue;
                }
            }

            list.Add((face, direction));
        }

        return list;
    }

    private IEnumerator ExecuteMoves(List<(string face, string direction)> moves)
    {
        foreach (var move in moves)
        {
            cubeManager.RotateFace(move.face, move.direction);
            yield return new WaitForSeconds(moveDelay);
        }
    }
}