using UnityEngine;

namespace Kociemba
{
    public static class TwistSlicePrune
    {
        public static int[] Table;

        public static void LoadFromTextAsset(TextAsset asset)
		{
			byte[] rawBytes = asset.bytes;
		
			if (rawBytes.Length != 1082565)
			{
				Debug.LogError($"❌ Invalid pruning table size: {rawBytes.Length} bytes (expected 1082565)");
			}
		
			Table = new int[rawBytes.Length];
			for (int i = 0; i < rawBytes.Length; i++)
				Table[i] = rawBytes[i];
		
			Debug.Log($"✅ Pruning table loaded. Entries: {Table.Length}");
		}

        public static int GetPrune(int twist, int slice)
        {
            int index = twist * 495 + slice;

            Debug.Log($"🧪 twist = {twist}, slice = {slice}");
            Debug.Log($"🧪 index = {index}, Table.Length = {Table?.Length}");

            if (twist < 0 || twist >= 2187)
                Debug.LogError($"❌ Invalid twist index: {twist} (expected 0–2186)");
            if (slice < 0 || slice >= 495)
                Debug.LogError($"❌ Invalid slice index: {slice} (expected 0–494)");
            if (index >= Table.Length)
                Debug.LogError($"❌ Invalid pruning index: {index} (twist={twist}, slice={slice})");

            return Table[index];
        }
    }
}
