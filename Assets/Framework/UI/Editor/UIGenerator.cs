using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Framework.UI
{
    public class UIGenerator : ScriptableObject
    {
        public string UIPrefabPath = "Prefabs/";
        public string UIScriptPath = "Scripts/";
        [ReadOnly]
        public string UIConfigPath;
        [ReadOnly]
        public List<string> UITypes = new List<string>();

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
                        instance.UIConfigPath = filePath + "/UICofig.asset";
                        AssetDatabase.CreateAsset(instance, instance.UIConfigPath);
                        AssetDatabase.Refresh();
                    }
                }
                return instance;
            }
        }
    }
}


