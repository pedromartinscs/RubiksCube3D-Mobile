using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Kociemba
{
    public class FaceCube
    {
        private readonly char[] facelets;
        private readonly Dictionary<char, char> colorToFace;

        public FaceCube(string facelets)
        {
            if (facelets.Length != 54)
                throw new ArgumentException("Cube string must be 54 characters");

            this.facelets = facelets.ToCharArray();

            colorToFace = new Dictionary<char, char>
            {
                [facelets[4]]  = 'U',
                [facelets[13]] = 'R',
                [facelets[22]] = 'F',
                [facelets[31]] = 'D',
                [facelets[40]] = 'L',
                [facelets[49]] = 'B'
            };
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

            string[] cornerColors = new string[]
            {
                "URF", "UFL", "ULB", "UBR",
                "DFR", "DLF", "DBL", "DRB"
            };

            bool[] cornerUsed = new bool[8];

            for (int i = 0; i < 8; i++)
            {
                char[] raw = new char[3];
                for (int j = 0; j < 3; j++)
                    raw[j] = colorToFace[facelets[cornerIndices[i][j]]];

                for (int target = 0; target < 8; target++)
                {
                    if (cornerUsed[target]) continue;

                    string expected = cornerColors[target];

                    for (int ori = 0; ori < 3; ori++)
                    {
                        char[] oriented = new char[]
                        {
                            raw[ori],
                            raw[(ori + 1) % 3],
                            raw[(ori + 2) % 3]
                        };

                        string candidate = new string(oriented);
                        if (new string(candidate.OrderBy(c => c).ToArray()) ==
                            new string(expected.OrderBy(c => c).ToArray()))
                        {
                            cubie.cp[i] = target;
                            cubie.co[i] = ori;
                            cornerUsed[target] = true;
                            goto nextCorner;
                        }
                    }
                }

            nextCorner:
                continue;
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

            char[] edgeOrientRef = new char[]
            {
                'U', 'U', 'U', 'U',
                'D', 'D', 'D', 'D',
                'F', 'F', 'L', 'R'
            };

            bool[] edgeUsed = new bool[12];

            for (int i = 0; i < 12; i++)
            {
                char c1 = colorToFace[facelets[edgeIndices[i][0]]];
                char c2 = colorToFace[facelets[edgeIndices[i][1]]];

                for (int target = 0; target < 12; target++)
                {
                    if (edgeUsed[target]) continue;

                    string expected = edgeColors[target];
                    char refFace = edgeOrientRef[target];

                    if ((c1 == expected[0] && c2 == expected[1]) ||
                        (c1 == expected[1] && c2 == expected[0]))
                    {
                        cubie.ep[i] = target;
                        if (c1 == refFace)
							cubie.eo[i] = 0;
						else if (c2 == refFace)
							cubie.eo[i] = 1;
						else
						{
							Debug.LogError($"🚨 Invalid edge orientation: {expected} → [{c1},{c2}], refFace={refFace}");
							cubie.eo[i] = 0; // fallback to 0
						}

                        edgeUsed[target] = true;
                        break;
                    }
                }
            }
			
			if (cubie.eo.Any(e => e != 0))
				Debug.LogWarning("⚠️ Cube has non-zero edge orientation in initial state.");

            // 🔍 Debug logs
            Debug.Log("🔍 Final cp[]: " + string.Join(",", cubie.cp));
            Debug.Log("🔍 Final co[]: " + string.Join(",", cubie.co));
            Debug.Log("🔍 Final ep[]: " + string.Join(",", cubie.ep));
            Debug.Log("🔍 Final eo[]: " + string.Join(",", cubie.eo));

            // Safety checks
            if (cubie.ep.Distinct().Count() != 12)
                Debug.LogError("🚨 Duplicate or missing edge permutation values in ep[]!");
            if (cubie.cp.Distinct().Count() != 8)
                Debug.LogError("🚨 Duplicate or missing corner permutation values in cp[]!");

            return cubie;
        }
    }
}
