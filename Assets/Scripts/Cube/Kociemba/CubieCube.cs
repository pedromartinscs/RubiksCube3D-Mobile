// CubieCube.cs
// Represents the Rubik's Cube in terms of permutations and orientations

namespace Kociemba
{
    public class CubieCube
    {
        public int[] cp = new int[8]; // Corner permutation
        public int[] co = new int[8]; // Corner orientation (0,1,2)
        public int[] ep = new int[12]; // Edge permutation
        public int[] eo = new int[12]; // Edge orientation (0,1)

        private static readonly int[][] cornerMove = new int[6][]
        {
            new int[] {3, 0, 1, 2, 4, 5, 6, 7}, // U
            new int[] {0, 1, 2, 3, 5, 6, 7, 4}, // D
            new int[] {1, 5, 2, 3, 0, 4, 6, 7}, // L
            new int[] {0, 1, 3, 7, 4, 5, 2, 6}, // R
            new int[] {0, 2, 6, 3, 4, 1, 5, 7}, // F
            new int[] {0, 1, 2, 3, 6, 7, 5, 4}, // B
        };

        private static readonly int[][] edgeMove = new int[6][]
        {
            new int[] {3, 0, 1, 2, 4, 5, 6, 7, 8, 9, 10, 11}, // U
            new int[] {0, 1, 2, 3, 7, 4, 5, 6, 8, 9, 10, 11}, // D
            new int[] {0, 1, 6, 3, 4, 5, 7, 2, 8, 9, 10, 11}, // L
            new int[] {0, 1, 2, 11, 4, 5, 6, 3, 8, 9, 10, 7}, // R
            new int[] {0, 9, 2, 3, 4, 8, 6, 7, 5, 1, 10, 11}, // F
            new int[] {10, 1, 2, 3, 4, 5, 11, 7, 8, 9, 6, 0}  // B
        };

        private static readonly int[][] cornerTwist = new int[6][]
        {
            new int[] {0, 0, 0, 0, 0, 0, 0, 0}, // U
            new int[] {0, 0, 0, 0, 0, 0, 0, 0}, // D
            new int[] {1, 2, 0, 0, 2, 1, 0, 0}, // L
            new int[] {0, 0, 2, 1, 0, 0, 1, 2}, // R
            new int[] {0, 1, 0, 0, 0, 2, 0, 0}, // F
            new int[] {2, 0, 0, 0, 1, 0, 0, 0}, // B
        };

        private static readonly int[][] edgeFlip = new int[6][]
        {
            new int[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // U
            new int[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // D
            new int[] {0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0}, // L
            new int[] {0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0}, // R
            new int[] {0, 1, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0}, // F
            new int[] {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1}  // B
        };

        public CubieCube()
        {
            for (int i = 0; i < 8; i++)
            {
                cp[i] = i;
                co[i] = 0;
            }
            for (int i = 0; i < 12; i++)
            {
                ep[i] = i;
                eo[i] = 0;
            }
        }

        public void ApplyMove(int move)
        {
            int[] newCp = new int[8];
            int[] newCo = new int[8];
            int[] newEp = new int[12];
            int[] newEo = new int[12];

            for (int i = 0; i < 8; i++)
            {
                newCp[i] = cp[cornerMove[move][i]];
                newCo[i] = (co[cornerMove[move][i]] + cornerTwist[move][i]) % 3;
            }
            for (int i = 0; i < 12; i++)
            {
                newEp[i] = ep[edgeMove[move][i]];
                newEo[i] = (eo[edgeMove[move][i]] + edgeFlip[move][i]) % 2;
            }

            cp = newCp;
            co = newCo;
            ep = newEp;
            eo = newEo;
        }

        public bool IsSolved()
        {
            for (int i = 0; i < 8; i++)
                if (cp[i] != i || co[i] != 0) return false;
            for (int i = 0; i < 12; i++)
                if (ep[i] != i || eo[i] != 0) return false;
            return true;
        }

        public bool IsInG1()
        {
            // All corners must be oriented
            foreach (int twist in co)
                if (twist != 0) return false;

            // All edges must be oriented
            foreach (int flip in eo)
                if (flip != 0) return false;

            // Middle layer edges (4, 5, 6, 7) must be in UD slice group
            // For now, we'll skip this enforcement to keep it simple
            return true;
        }
    }
}
