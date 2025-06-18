// FaceCube.cs
// Represents the cube as a string of 54 facelets and converts it to CubieCube
using System;
using System.Linq;

namespace Kociemba
{
    public class FaceCube
    {
        private readonly char[] facelets;

        public FaceCube(string facelets)
        {
            if (facelets.Length != 54)
                throw new System.ArgumentException("Cube string must be 54 characters");

            this.facelets = facelets.ToCharArray();
        }

        public CubieCube ToCubieCube()
		{
			var cubie = new CubieCube();
			
			// === CORNERS ===
			int[][] cornerIndices = new int[][]
			{
				new int[] { 8,  9, 20 },   // URF
				new int[] { 6, 18, 38 },   // UFL
				new int[] { 0, 36, 47 },   // ULB
				new int[] { 2, 45, 11 },   // UBR
				new int[] { 29, 26, 15 },  // DFR
				new int[] { 27, 44, 24 },  // DLF
				new int[] { 33, 53, 42 },  // DBL
				new int[] { 35, 17, 51 }   // DRB
			};

			for (int i = 0; i < 8; i++)
			{
				char[] faces = new char[3];
				for (int j = 0; j < 3; j++)
					faces[j] = facelets[cornerIndices[i][j]];

				for (int ori = 0; ori < 3; ori++)
				{
					string key = new string(new char[]
					{
						faces[ori],
						faces[(ori + 1) % 3],
						faces[(ori + 2) % 3]
					});

					int idx = CornerColorIndex(key);
					if (idx != -1)
					{
						cubie.cp[i] = idx;
						cubie.co[i] = ori;
						break;
					}
				}
			}

			// === EDGES ===
			int[][] edgeIndices = new int[][]
			{
				new int[] { 5, 10 },  // UR
				new int[] { 7, 19 },  // UF
				new int[] { 3, 37 },  // UL
				new int[] { 1, 46 },  // UB
				new int[] { 32, 16 }, // DR
				new int[] { 28, 25 }, // DF
				new int[] { 30, 43 }, // DL
				new int[] { 34, 52 }, // DB
				new int[] { 23, 12 }, // FR
				new int[] { 21, 41 }, // FL
				new int[] { 39, 50 }, // BL
				new int[] { 14, 48 }  // BR
			};

			string[] edgeColors = new string[]
			{
				"UR", "UF", "UL", "UB",
				"DR", "DF", "DL", "DB",
				"FR", "FL", "BL", "BR"
			};

			for (int i = 0; i < 12; i++)
			{
				char a = facelets[edgeIndices[i][0]];
				char b = facelets[edgeIndices[i][1]];

				string key = $"{a}{b}";
				string keyFlipped = $"{b}{a}";

				int idx = Array.IndexOf(edgeColors, key);
				int idxFlip = Array.IndexOf(edgeColors, keyFlipped);

				if (idx != -1)
				{
					cubie.ep[i] = idx;
					cubie.eo[i] = 0;
				}
				else if (idxFlip != -1)
				{
					cubie.ep[i] = idxFlip;
					cubie.eo[i] = 1;
				}
			}

			return cubie;
		}

		private int CornerColorIndex(string key)
		{
			string[] standard = new string[]
			{
				"URF", "UFL", "ULB", "UBR",
				"DFR", "DLF", "DBL", "DRB"
			};

			for (int i = 0; i < standard.Length; i++)
			{
				string std = standard[i];
				if (new string(std.OrderBy(c => c).ToArray()) == new string(key.OrderBy(c => c).ToArray()))
					return i;
			}

			return -1;
		}
    }
}
