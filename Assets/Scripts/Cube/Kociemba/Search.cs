// Search.cs
// Core of the Kociemba 2-phase Rubik's Cube solving algorithm (Phase 1 + Phase 2)

using System;
using System.Collections.Generic;

namespace Kociemba
{
    public static class Search
    {
        private static readonly string[] moveNames =
        {
            "U", "U2", "U'", "R", "R2", "R'", "F", "F2", "F'",
            "D", "D2", "D'", "L", "L2", "L'", "B", "B2", "B'"
        };

        public static string solution(string facelets, int maxDepth, int timeOut, bool useSeparator)
        {
            if (facelets.Length != 54)
                throw new ArgumentException("Facelet string must be exactly 54 characters");

            var faceCube = new FaceCube(facelets);
            CubieCube cube = faceCube.ToCubieCube();

            if (cube.IsSolved())
                return "";

            for (int depth = 1; depth <= maxDepth; depth++)
            {
                var result = SearchPhase1(cube, depth, new List<string>(), -1);
                if (result != null)
                {
                    // Phase 2: solve from G1 to full solution
                    CubieCube g1Cube = ApplyMoves(cube, result);
                    var phase2 = SearchPhase2(g1Cube, maxDepth - result.Count);
                    if (phase2 != null)
                    {
                        result.AddRange(phase2);
                        return string.Join(" ", result);
                    }
                }
            }

            return null;
        }

        private static List<string> SearchPhase1(CubieCube cube, int depthLeft, List<string> path, int lastFace)
        {
            if (cube.IsInG1())
                return new List<string>(path);

            int heuristic = Pruning.GetCombinedPrune(cube);
            if (heuristic > depthLeft)
                return null;

            if (depthLeft == 0)
                return null;

            for (int move = 0; move < 6; move++)
            {
                if (move == lastFace) continue;

                for (int power = 1; power <= 3; power++)
                {
                    CubieCube nextCube = new CubieCube();
                    Array.Copy(cube.cp, nextCube.cp, 8);
                    Array.Copy(cube.co, nextCube.co, 8);
                    Array.Copy(cube.ep, nextCube.ep, 12);
                    Array.Copy(cube.eo, nextCube.eo, 12);

                    for (int i = 0; i < power; i++)
                        nextCube.ApplyMove(move);

                    var moveStr = moveNames[move * 3 + (power - 1)];
                    path.Add(moveStr);

                    var result = SearchPhase1(nextCube, depthLeft - 1, path, move);
                    if (result != null)
                        return result;

                    path.RemoveAt(path.Count - 1);
                }
            }

            return null;
        }

        private static List<string> SearchPhase2(CubieCube cube, int maxDepth)
        {
            // Very simplified Phase 2: brute-force up to 6 moves
            List<string> path = new List<string>();
            for (int depth = 0; depth <= maxDepth; depth++)
            {
                var result = Phase2Search(cube, depth, path, -1);
                if (result != null) return result;
            }
            return null;
        }

        private static List<string> Phase2Search(CubieCube cube, int depthLeft, List<string> path, int lastFace)
        {
            if (depthLeft == 0)
                return cube.IsSolved() ? new List<string>(path) : null;

            for (int move = 0; move < 6; move++)
            {
                if (move == lastFace) continue;
                if (move == 2 || move == 4) continue; // Skip F and B in Phase 2 for simplicity

                for (int power = 1; power <= 3; power++)
                {
                    CubieCube nextCube = new CubieCube();
                    Array.Copy(cube.cp, nextCube.cp, 8);
                    Array.Copy(cube.co, nextCube.co, 8);
                    Array.Copy(cube.ep, nextCube.ep, 12);
                    Array.Copy(cube.eo, nextCube.eo, 12);

                    for (int i = 0; i < power; i++)
                        nextCube.ApplyMove(move);

                    var moveStr = moveNames[move * 3 + (power - 1)];
                    path.Add(moveStr);

                    var result = Phase2Search(nextCube, depthLeft - 1, path, move);
                    if (result != null)
                        return result;

                    path.RemoveAt(path.Count - 1);
                }
            }
            return null;
        }

        private static CubieCube ApplyMoves(CubieCube cube, List<string> moves)
        {
            CubieCube result = new CubieCube();
            Array.Copy(cube.cp, result.cp, 8);
            Array.Copy(cube.co, result.co, 8);
            Array.Copy(cube.ep, result.ep, 12);
            Array.Copy(cube.eo, result.eo, 12);

            for (int i = 0; i < moves.Count; i++)
            {
                string move = moves[i];
                int baseIdx = "URFDLB".IndexOf(move[0]);
                int power = 1;
                if (move.EndsWith("2")) power = 2;
                else if (move.EndsWith("'")) power = 3;

                for (int j = 0; j < power; j++)
                    result.ApplyMove(baseIdx);
            }

            return result;
        }
    }
}
