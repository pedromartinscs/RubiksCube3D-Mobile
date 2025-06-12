// Pruning.cs
// Implements simple pruning guidance for edge orientation (EO) and corner orientation (CO)

namespace Kociemba
{
    public static class Pruning
    {
        // These are dummy pruning tables with basic symmetry
        // In a real solver these would be precomputed and cover all states

        public static int GetSimpleEOPrune(CubieCube cube)
        {
            int flips = 0;
            for (int i = 0; i < cube.eo.Length; i++)
                if (cube.eo[i] != 0) flips++;
            return flips / 2; // Crude heuristic
        }

        public static int GetSimpleCOPrune(CubieCube cube)
        {
            int twists = 0;
            for (int i = 0; i < cube.co.Length; i++)
                if (cube.co[i] != 0) twists++;
            return twists / 2; // Crude heuristic
        }

        public static int GetCombinedPrune(CubieCube cube)
        {
            return GetSimpleEOPrune(cube) + GetSimpleCOPrune(cube);
        }
    }
}
