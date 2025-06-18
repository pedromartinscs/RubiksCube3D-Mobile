using UnityEngine;

namespace Kociemba
{
    public static class TwistSlicePrune
    {
        public static byte[] Table;

        public static void LoadFromTextAsset(TextAsset binaryAsset)
        {
            if (binaryAsset == null)
            {
                Debug.LogError("❌ Pruning table binary asset is null.");
                return;
            }

            Table = binaryAsset.bytes;
            Debug.Log($"✅ Pruning table loaded. Entries: {Table.Length}");
        }

        public static int GetPrune(int twistIndex, int sliceIndex)
        {
            if (Table == null)
            {
                Debug.LogError("❌ Pruning table not loaded yet.");
                return 999;
            }

            int index = twistIndex * 495 + sliceIndex;
            if (index < 0 || index >= Table.Length)
            {
                Debug.LogError($"❌ Invalid pruning index: {index}");
                return 999;
            }

            return Table[index];
        }
    }
}
