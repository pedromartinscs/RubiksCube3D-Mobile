using System;
using UnityEngine;

namespace Kociemba
{
    public static class Search
    {
        private static readonly string[] MoveStr = {
            "U", "U2", "U'", "R", "R2", "R'", "F", "F2", "F'",
            "D", "D2", "D'", "L", "L2", "L'", "B", "B2", "B'"
        };

        private static long startTime;
        private const int timeoutMilliseconds = 8000;

        private static string solutionString;
        private static bool _isSolved;
		public static bool isSolved => _isSolved;

        private static CubieCube[] moveBuffer = new CubieCube[30];
        private static int[] moveHistory = new int[30];

        public static string solution(string facelets, int maxDepth, int timeOut, bool useSeparator)
        {
            _isSolved = false;
            solutionString = null;

            FaceCube fc = new FaceCube(facelets);
            CubieCube cc = fc.ToCubieCube();

            if (!cc.IsValid())
            {
                Debug.LogError("❌ Invalid cube state");
                return null;
            }

            int twist = cc.getTwist();
            int flip = cc.getFlip();
            int slice = cc.GetUDSliceIndex();

            int prune = TwistSlicePrune.GetPrune(twist, slice);

            if (prune == 0)
            {
                return "";
            }

            startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            moveBuffer[0] = new CubieCube();
			cc.CopyTo(moveBuffer[0]);
            try
            {
                for (int depth = prune; depth <= maxDepth; depth++)
                {
                    Debug.Log($"🔍 Trying solution at depth {depth}...");

                    if (search(0, depth, twist, flip, slice, -1))
                    {
                        return solutionString;
                    }

                    if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime > timeoutMilliseconds)
                    {
                        Debug.LogError("❌ Solver timed out.");
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("❌ Solver failed: " + e.Message);
                return null;
            }

            Debug.LogError("❌ No solution found.");
            return null;
        }

        private static bool search(int depth, int maxDepth, int twist, int flip, int slice, int lastMove)
        {
            if (depth == maxDepth)
            {
                if (twist == 0 && flip == 0 && slice == 0)
                {
                    _isSolved = true;
                    solutionString = stringifyMoves(depth);
                    return true;
                }
                return false;
            }

            if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime > timeoutMilliseconds)
                return false;

            for (int move = 0; move < 18; move++)
            {
                int axis = move / 3;

                if (axis == lastMove)
                    continue;

                moveBuffer[depth + 1] = new CubieCube();
				
				if (moveBuffer[depth] == null)
				{
					Debug.LogError($"❌ moveBuffer[{depth}] is null!");
					return false;
				}
				
                CubieCube.cornerMultiply(moveBuffer[depth], CubieCube.MoveCube[move], moveBuffer[depth + 1]);
				CubieCube.edgeMultiply(moveBuffer[depth], CubieCube.MoveCube[move], moveBuffer[depth + 1]);

                int nextTwist = moveBuffer[depth + 1].getTwist();
                int nextFlip = moveBuffer[depth + 1].getFlip();
                int nextSlice = moveBuffer[depth + 1].GetUDSliceIndex();

                int prune = TwistSlicePrune.GetPrune(nextTwist, nextSlice);

                if (depth + prune > maxDepth)
                    continue;

                moveHistory[depth] = move;

                if (search(depth + 1, maxDepth, nextTwist, nextFlip, nextSlice, axis))
                    return true;
            }

            return false;
        }

        private static string stringifyMoves(int depth)
        {
            string result = "";
            for (int i = 0; i < depth; i++)
            {
                result += MoveStr[moveHistory[i]] + " ";
            }
            return result.Trim();
        }
    }
}
