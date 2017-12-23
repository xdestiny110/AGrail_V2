using UnityEditor;
using UnityEngine;

namespace Framework.UI
{
    public class ItemGenerator : ScriptableObject
    {
        public string PrefabPath = "Prefabs/";
        public string ScriptPath = "Scripts/";
        [ReadOnly]
        public string ConfigPath;

        private static ItemGenerator instance;
        public static ItemGenerator Instance
        {
            get
            {
                if (instance == null)
                {
                    var guids = AssetDatabase.FindAssets("ItemConfig.asset");
                    if (guids.Length > 0)
                        instance = AssetDatabase.LoadAssetAtPath<ItemGenerator>(AssetDatabase.GUIDToAssetPath(guids[0]));
                    if (instance == null)
                    {
                        var filePath = Tool.SystemPathToUnityPath(
                            new System.Diagnostics.StackTrace(1, true).GetFrame(0).GetFileName());
                        filePath = filePath.Substring(0, filePath.LastIndexOf("/"));
                        instance = CreateInstance<ItemGenerator>();
                        instance.ConfigPath = filePath + "/ItemCofig.asset";
                        AssetDatabase.CreateAsset(instance, instance.ConfigPath);
                        AssetDatabase.Refresh();
                    }
                }
                return instance;
            }
        }
    }
}


