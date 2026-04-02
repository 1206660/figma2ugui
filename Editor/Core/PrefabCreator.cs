using System.IO;
using UnityEditor;
using UnityEngine;

namespace Figma2Ugui.Core
{
    public class PrefabCreator
    {
        private const string PREFAB_DIR = "Assets/Prefabs";

        public void SaveAsPrefab(GameObject go, string name)
        {
            Directory.CreateDirectory(PREFAB_DIR);
            var path = $"{PREFAB_DIR}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            AssetDatabase.Refresh();
        }
    }
}
