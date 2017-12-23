using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Framework.UI
{
    public class UIGenerator : ScriptableObject
    {
        public string PrefabPath = "Prefabs/";
        public string ScriptPath = "Scripts/";
        [ReadOnly]
        public string ConfigPath;

        private static UIGenerator instance;
        public static UIGenerator Instance
        {
            get
            {
                if(instance == null)
                {
                    var guids = AssetDatabase.FindAssets("UIConfig.asset");
                    if (guids.Length > 0)
                        instance = AssetDatabase.LoadAssetAtPath<UIGenerator>(AssetDatabase.GUIDToAssetPath(guids[0]));
                    if(instance == null)
                    {
                        var filePath = Tool.SystemPathToUnityPath(
                            new System.Diagnostics.StackTrace(1, true).GetFrame(0).GetFileName());
                        filePath = filePath.Substring(0, filePath.LastIndexOf("/"));
                        instance = CreateInstance<UIGenerator>();
                        instance.ConfigPath = filePath + "/UICofig.asset";
                        AssetDatabase.CreateAsset(instance, instance.ConfigPath);
                        AssetDatabase.Refresh();
                    }
                }
                return instance;
            }
        }
    }
}


