using System;
using UnityEngine;
using Kociemba;

public static class KociembaSolver
{
    public static string Solve(string cubeState, int maxDepth = 21, int timeOut = 5, bool useSeparator = false)
    {
		CubieCube.InitializeMoveCubes();
		
        try
        {
            if (cubeState.Length != 54)
                throw new ArgumentException("Cube state must be 54 characters.");

            return Kociemba.Search.solution(cubeState, maxDepth, timeOut, useSeparator);
        }
        catch (Exception ex)
        {
            Debug.LogError("Solver Error: " + ex.Message);
            return null;
        }
    }
}
