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
        private static int timeoutMilliseconds = 8000;

        private static string solutionString;
        private static bool _isSolved;
        public static bool isSolved => _isSolved;

        private static CubieCube[] moveBuffer = new CubieCube[30];
        private static int[] moveHistory = new int[30];

        public static string solution(string facelets, int maxDepth, int timeOut, bool useSeparator)
        {
            Debug.Log("🧩 Starting solver...");
            Debug.Log("Facelet input: " + facelets);

            _isSolved = false;
            solutionString = null;
            timeoutMilliseconds = timeOut * 1000;

            FaceCube fc = new FaceCube(facelets);
            CubieCube cc = fc.ToCubieCube();

            if (!cc.IsValid())
            {
                Debug.LogError("❌ Invalid cube state (parity or orientation)");
                return null;
            }

            if (cc.IsSolved())
            {
                Debug.Log("✅ Cube is already solved.");
                return "";
            }
			
			// Check for solution in one move
			CubieCube test = new CubieCube();
			cc.CopyTo(test);
			for (int move = 0; move < 18; move++)
			{
				CubieCube candidate = new CubieCube();
				CubieCube.cornerMultiply(test, CubieCube.MoveCube[move], candidate);
				CubieCube.edgeMultiply(test, CubieCube.MoveCube[move], candidate);
			
				if (candidate.IsSolved())
				{
					Debug.Log($"🎯 Solved in 1 move: {MoveStr[move]}");
					return MoveStr[move];
				}
			}

            int twist = cc.getTwist();
            int flip = cc.getFlip();
            int slice = cc.GetUDSliceIndex();
            int prune = TwistSlicePrune.GetPrune(twist, slice);

            Debug.Log($"🔍 Twist: {twist}, Flip: {flip}, Slice: {slice}");
            Debug.Log($"🔎 Initial prune value: {prune}");

            startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            moveBuffer[0] = new CubieCube();
            cc.CopyTo(moveBuffer[0]);

            try
            {
                for (int depth = prune; depth <= maxDepth; depth++)
                {
                    Debug.Log($"🔄 Trying depth: {depth}...");

                    if (search(0, depth, twist, flip, slice, -1))
                    {
                        Debug.Log("🎉 Solution found!");
                        return solutionString;
                    }

                    if (TimedOut())
                    {
                        Debug.LogError("⏱ Timeout reached.");
                        return _isSolved ? solutionString : null;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("❌ Search failed: " + e.Message);
                return null;
            }

            if (_isSolved)
            {
                Debug.Log("✅ Solution found late in search.");
                return solutionString;
            }

            Debug.LogError("❌ No solution found.");
            return null;
        }

        private static bool search(int depth, int maxDepth, int twist, int flip, int slice, int lastMove)
        {
            if (TimedOut()) return false;

            for (int move = 0; move < 18; move++)
            {
                int axis = move / 3;
                if (axis == lastMove) continue;

                moveBuffer[depth + 1] = new CubieCube();
                CubieCube.cornerMultiply(moveBuffer[depth], CubieCube.MoveCube[move], moveBuffer[depth + 1]);
                CubieCube.edgeMultiply(moveBuffer[depth], CubieCube.MoveCube[move], moveBuffer[depth + 1]);

                int nextTwist = moveBuffer[depth + 1].getTwist();
                int nextFlip = moveBuffer[depth + 1].getFlip();
                int nextSlice = moveBuffer[depth + 1].GetUDSliceIndex();

                int prune = TwistSlicePrune.GetPrune(nextTwist, nextSlice);

                if (nextTwist == 0 && nextFlip == 0 && nextSlice == 0)
                {
                    if (moveBuffer[depth + 1].IsSolved())
                    {
                        _isSolved = true;
                        moveHistory[depth] = move;
                        solutionString = stringifyMoves(depth + 1);
                        Debug.Log($"✅ Solved at depth {depth + 1}: {solutionString}");
                        return true;
                    }
                }

                if (depth + prune > maxDepth)
                    continue;

                moveHistory[depth] = move;

                if (search(depth + 1, maxDepth, nextTwist, nextFlip, nextSlice, axis))
                    return true;
            }

            return false;
        }

        private static bool TimedOut()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime > timeoutMilliseconds;
        }

        private static string stringifyMoves(int depth)
        {
            string result = "";
            for (int i = 0; i < depth; i++)
                result += MoveStr[moveHistory[i]] + " ";
            return result.Trim();
        }
    }
}
