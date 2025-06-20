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
		
			Debug.Log($"🧪 twist = {twist}, slice = {slice}");
		
			int index = twist * 495 + slice;
			Debug.Log($"🧪 Calculated pruning index = {index}, Table.Length = {TwistSlicePrune.Table.Length}");
		
			if (slice == -1)
			{
				Debug.LogError("❌ Invalid UD-slice index — cube state has invalid slice configuration.");
				return 999;
			}
		
			if (twist < 0 || twist >= 2187)
				Debug.LogError($"❌ Invalid twist index: {twist} (should be in 0–2186)");
			if (slice < 0 || slice >= 495)
				Debug.LogError($"❌ Invalid slice index: {slice} (should be in 0–494)");
			if (index >= TwistSlicePrune.Table.Length)
				Debug.LogError($"❌ Prune index {index} is out of bounds (twist={twist}, slice={slice})");
		
			return TwistSlicePrune.GetPrune(twist, slice);
		}
    }
}
