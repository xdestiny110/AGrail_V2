using UnityEngine;
using Framework.AssetBundle;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace Framework.Message
{
#if UNITY_EDITOR
    [InitializeOnLoad, System.Serializable]
#else
    [System.Serializable]
#endif
    public class MsgCodeGenerator : ScriptableObject
    {
        public string MessageConfigPath;

        private static MsgCodeGenerator instance;
        public static MsgCodeGenerator Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = AssetBundleManager.Instance.LoadAsset<MsgCodeGenerator>("util", "MessageConfig");
#if UNITY_EDITOR
                    if (instance == null)
                    {
                        var filePath = new System.Diagnostics.StackTrace(1, true).GetFrame(0).GetFileName();
                        filePath = filePath.Replace("\\", "/");
                        filePath = filePath.Substring(filePath.IndexOf("Assets/"));
                        filePath = filePath.Substring(0, filePath.LastIndexOf("/"));
                        filePath = filePath.Substring(0, filePath.LastIndexOf("/"));
                        instance = CreateInstance<MsgCodeGenerator>();
                        instance.MessageConfigPath = filePath + "/MessageConfig.asset";
                        AssetDatabase.CreateAsset(instance, instance.MessageConfigPath);
                        var importer = AssetImporter.GetAtPath(instance.MessageConfigPath);
                        if (importer.assetBundleName == "")
                        {
                            importer.assetBundleName = "util";
                            importer.SaveAndReimport();
                        }
                        AssetDatabase.Refresh();
                    }
#endif
                }
                return instance;
            }
        }

        [HideInInspector]
        public List<string> msgTypesConst = new List<string>()
        {
            "Null = 0",
            "OnConnect",
            "OnDisconnect",
            "OnReconnect",
            "OnUICreate",
            "OnUIDestroy",
            "OnUIShow",
            "OnUIHide",
            "OnUIPause",
            "OnUIResume",
        };

        [HideInInspector]
        public List<string> msgTypesProto = new List<string>();


        [HideInInspector]
        public List<string> msgTypes = new List<string>();

    }
}


