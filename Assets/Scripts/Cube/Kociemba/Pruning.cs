// Pruning.cs
// Implements basic pruning heuristics for orientation
using UnityEngine;

namespace Kociemba
{
    public static class Pruning
    {
        // Estimates how far the cube is from the G1 group (i.e., all edges/corners oriented)
        public static int GetSimpleEOPrune(CubieCube cube)
        {
            int flips = 0;
            for (int i = 0; i < cube.eo.Length; i++)
                if (cube.eo[i] != 0) flips++;
            return flips / 2; // Every 2 flips require at least 1 move
        }

        public static int GetSimpleCOPrune(CubieCube cube)
        {
            int twists = 0;
            for (int i = 0; i < cube.co.Length; i++)
                if (cube.co[i] != 0) twists++;
            return twists / 2; // Every 2 twists require at least 1 move
        }

        public static int GetCombinedPrune(CubieCube cube)
		{
			int twist = cube.GetTwistIndex();
			int slice = cube.GetUDSliceIndex();
			if (slice == -1)
			{
				Debug.LogError("❌ Invalid UD-slice index — cube state has invalid slice configuration.");
				return 999;
			}
			
			int index = twist * 495 + slice;
			if (index >= TwistSlicePrune.Table.Length)
			{
				Debug.LogError($"❌ Prune index {index} is out of bounds (twist={twist}, slice={slice})");
				return 999;
			}
		
			return TwistSlicePrune.GetPrune(twist, slice);
		}
    }
}
