#region

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

#endregion

namespace DEX.Engine
{
    public class MaterialCache
    {
        private static readonly Dictionary<Hash128, MaterialEntry> materialMap =
            new Dictionary<Hash128, MaterialEntry>();

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void ClearCache()
        {
            foreach (var entry in materialMap.Values) entry.Release();

            materialMap.Clear();
        }
#endif

        public static Material Register(Material baseMaterial, Hash128 hash, Action<Material, Graphic> onModifyMaterial,
            Graphic graphic)
        {
            if (!hash.isValid) return null;

            MaterialEntry entry;
            if (!materialMap.TryGetValue(hash, out entry))
            {
                entry = new MaterialEntry
                {
                    material = new Material(baseMaterial)
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    }
                };

                onModifyMaterial(entry.material, graphic);
                materialMap.Add(hash, entry);
            }

            entry.referenceCount++;
            return entry.material;
        }

        public static void Unregister(Hash128 hash)
        {
            MaterialEntry entry;
            if (!hash.isValid || !materialMap.TryGetValue(hash, out entry)) return;
            if (--entry.referenceCount > 0) return;

            entry.Release();
            materialMap.Remove(hash);
        }

        private class MaterialEntry
        {
            public Material material;
            public int referenceCount;

            public void Release()
            {
                if (material) Object.DestroyImmediate(material, false);

                material = null;
            }
        }
    }
}