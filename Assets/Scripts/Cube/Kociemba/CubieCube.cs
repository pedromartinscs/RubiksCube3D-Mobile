using System;
using UnityEngine;

namespace Kociemba
{
    public class CubieCube
    {
        public int[] cp = new int[8];  // Corner permutation
        public int[] co = new int[8];  // Corner orientation
        public int[] ep = new int[12]; // Edge permutation
        public int[] eo = new int[12]; // Edge orientation

        public static CubieCube[] MoveCube = new CubieCube[18];

        public CubieCube() { }

        public CubieCube(CubieCube other)
        {
            Array.Copy(other.cp, cp, 8);
            Array.Copy(other.co, co, 8);
            Array.Copy(other.ep, ep, 12);
            Array.Copy(other.eo, eo, 12);
        }

        public void CopyTo(CubieCube target)
        {
            Array.Copy(this.cp, target.cp, 8);
            Array.Copy(this.co, target.co, 8);
            Array.Copy(this.ep, target.ep, 12);
            Array.Copy(this.eo, target.eo, 12);
        }

        public bool IsValid()
        {
            int sum = 0;
            for (int i = 0; i < 8; i++) sum += co[i];
            if (sum % 3 != 0) return false;

            sum = 0;
            for (int i = 0; i < 12; i++) sum += eo[i];
            if (sum % 2 != 0) return false;

            if ((CornerParity() ^ EdgeParity()) != 0)
            {
                Debug.LogError("🧩 Invalid parity between corner and edge permutations.");
                return false;
            }

            return true;
        }

        public int CornerParity()
        {
            int parity = 0;
            for (int i = 0; i < 7; i++)
                for (int j = i + 1; j < 8; j++)
                    if (cp[i] > cp[j])
                        parity ^= 1;
            return parity;
        }

        public int EdgeParity()
        {
            int parity = 0;
            for (int i = 0; i < 11; i++)
                for (int j = i + 1; j < 12; j++)
                    if (ep[i] > ep[j])
                        parity ^= 1;
            return parity;
        }

        public static void cornerMultiply(CubieCube a, CubieCube b, CubieCube ab)
        {
            for (int i = 0; i < 8; i++)
            {
                ab.cp[i] = a.cp[b.cp[i]];
                int oriA = a.co[b.cp[i]];
                int oriB = b.co[i];
                ab.co[i] = (oriA + oriB) % 3;
            }
        }

        public static void edgeMultiply(CubieCube a, CubieCube b, CubieCube ab)
        {
            for (int i = 0; i < 12; i++)
            {
                ab.ep[i] = a.ep[b.ep[i]];
                ab.eo[i] = (a.eo[b.ep[i]] + b.eo[i]) % 2;
            }
        }

        public int GetTwistIndex()
        {
            int twist = 0;
            for (int i = 0; i < 7; i++)
                twist = 3 * twist + co[i];
            return twist;
        }

        public int GetFlipIndex()
        {
            int flip = 0;
            for (int i = 0; i < 11; i++)
                flip = 2 * flip + eo[i];
            return flip;
        }

        public int GetUDSliceIndex()
		{
			int count = 0;
			int index = 0;
			for (int i = 0; i < 12; i++)
			{
				if (ep[i] >= 8) // slice edges are FR, FL, BL, BR (indices 8 to 11)
				{
					index += choose(i, count + 1);
					count++;
				}
			}
		
			return index;
		}
		
		private int choose(int n, int k)
		{
			if (k > n) return 0;
			if (k == 0 || k == n) return 1;
			int res = 1;
			for (int i = 1; i <= k; i++)
				res = res * (n - i + 1) / i;
			return res;
		}

        public bool IsSolved()
        {
            for (int i = 0; i < 8; i++)
                if (cp[i] != i || co[i] != 0) return false;
            for (int i = 0; i < 12; i++)
                if (ep[i] != i || eo[i] != 0) return false;
            return true;
        }
		
		public int getTwist()
		{
			int twist = 0;
			for (int i = 0; i < 7; i++)
				twist = 3 * twist + co[i];
			return twist;
		}
		
		public int getFlip()
		{
			int flip = 0;
			for (int i = 0; i < 11; i++)
				flip = 2 * flip + eo[i];
			return flip;
		}

        public static void InitializeMoveCubes()
        {
            int[][] cpMoves = new int[][]
            {
                new int[]{3,0,1,2,4,5,6,7}, // U
                new int[]{0,1,2,3,5,6,7,4}, // D
                new int[]{0,1,6,3,4,2,5,7}, // L
                new int[]{4,1,2,0,7,5,6,3}, // F
                new int[]{0,5,2,3,4,6,1,7}, // B
                new int[]{0,1,2,7,3,5,6,4}  // R
            };

            int[][] coMoves = new int[][]
            {
                new int[]{0,0,0,0,0,0,0,0}, // U
                new int[]{0,0,0,0,0,0,0,0}, // D
                new int[]{0,0,2,0,0,1,1,0}, // L
                new int[]{1,0,0,2,2,0,0,1}, // F
                new int[]{2,0,0,1,1,0,0,2}, // B
                new int[]{0,1,2,0,0,2,1,0}  // R
            };

            int[][] epMoves = new int[][]
            {
                new int[]{3,0,1,2,4,5,6,7,8,9,10,11}, // U
                new int[]{0,1,2,3,5,6,7,4,8,9,10,11}, // D
                new int[]{0,1,10,3,4,5,11,7,8,9,6,2}, // L
                new int[]{0,1,2,3,4,9,6,8,5,10,7,11}, // F
                new int[]{0,1,2,3,11,5,10,7,4,6,9,8}, // B
                new int[]{8,1,2,9,0,5,6,3,4,7,10,11}  // R
            };

            int[][] eoMoves = new int[][]
            {
                new int[]{0,0,0,0,0,0,0,0,0,0,0,0}, // U
                new int[]{0,0,0,0,0,0,0,0,0,0,0,0}, // D
                new int[]{0,0,0,0,0,0,0,0,0,0,1,1}, // L
                new int[]{0,0,0,0,0,1,0,1,1,0,0,0}, // F
                new int[]{0,0,0,0,1,0,1,0,0,1,0,0}, // B
                new int[]{1,0,0,1,1,0,0,1,0,0,0,0}  // R
            };

            for (int i = 0; i < 6; i++)
            {
                CubieCube baseMove = new CubieCube();
                for (int j = 0; j < 8; j++) { baseMove.cp[j] = cpMoves[i][j]; baseMove.co[j] = coMoves[i][j]; }
                for (int j = 0; j < 12; j++) { baseMove.ep[j] = epMoves[i][j]; baseMove.eo[j] = eoMoves[i][j]; }

                CubieCube move1 = new CubieCube(baseMove);
                CubieCube move2 = new CubieCube(); cornerMultiply(move1, move1, move2); edgeMultiply(move1, move1, move2);
                CubieCube move3 = new CubieCube(); cornerMultiply(move1, move2, move3); edgeMultiply(move1, move2, move3);

                MoveCube[3 * i + 0] = move1;
                MoveCube[3 * i + 1] = move2;
                MoveCube[3 * i + 2] = move3;
            }
        }
    }
}
