// Util.cs
// Placeholder for lookup tables and helpers in Kociemba's algorithm

namespace Kociemba
{
    public static class Util
    {
        public static readonly int[][] MoveTable = new int[6][];

        static Util()
        {
            // Initialize dummy move tables for now
            for (int i = 0; i < 6; i++)
            {
                MoveTable[i] = new int[3]; // [normal, inverse, double]
                MoveTable[i][0] = i;       // Identity move
                MoveTable[i][1] = i;       // Inverse same (placeholder)
                MoveTable[i][2] = i;       // Double same (placeholder)
            }
        }

        public static int ApplyMove(int state, int move)
        {
            return MoveTable[move][0]; // Placeholder: returns dummy
        }
    }
}
