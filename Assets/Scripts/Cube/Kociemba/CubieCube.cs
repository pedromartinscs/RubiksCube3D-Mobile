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

            if (GetTwistIndex() != 0)
            {
                Debug.LogError("🌀 Invalid corner twist.");
                return false;
            }

            if (GetFlipIndex() != 0)
            {
                Debug.LogError("↩️ Invalid edge flip.");
                return false;
            }

            return true;
        }

        public int CornerParity()
        {
            int parity = 0;
            for (int i = 0; i < 7; i++)
            {
                for (int j = i + 1; j < 8; j++)
                {
                    if (cp[i] > cp[j])
                    {
                        parity ^= 1;
                    }
                }
            }
            return parity;
        }

        public int EdgeParity()
        {
            int parity = 0;
            for (int i = 0; i < 11; i++)
            {
                for (int j = i + 1; j < 12; j++)
                {
                    if (ep[i] > ep[j])
                    {
                        parity ^= 1;
                    }
                }
            }
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

        public int GetTwistIndex() => getTwist();

        public int GetFlipIndex() => getFlip();

        public int GetUDSliceIndex()
        {
            int slice = 0;
            for (int i = 0; i < 12; i++)
            {
                if (ep[i] >= 8)
                {
                    slice |= 1 << (11 - i);
                }
            }
            return slice;
        }

        public bool IsSolved()
        {
            for (int i = 0; i < 8; i++)
                if (cp[i] != i || co[i] != 0) return false;
            for (int i = 0; i < 12; i++)
                if (ep[i] != i || eo[i] != 0) return false;
            return true;
        }
    }
}
