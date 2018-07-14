using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Framework.UI
{
    [CustomEditor(typeof(UIGenerator))]
    public class UIGeneratorEditor : Editor
    {
        [MenuItem("Framework/UI/UI Generator")]
        static void uiGenerator()
        {
            Selection.activeObject = UIGenerator.Instance;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Generate Code"))
                generateUI();
        }

        private void generateUI()
        {
            if (!Directory.Exists(Tool.UnityPathToSystemPath(UIGenerator.Instance.PrefabPath + UIFactory.UIPrefabPath)))
                Directory.CreateDirectory(Tool.UnityPathToSystemPath(UIGenerator.Instance.PrefabPath + UIFactory.UIPrefabPath));
            if(!Directory.Exists(Tool.UnityPathToSystemPath(UIGenerator.Instance.ScriptPath + UIFactory.UIPrefabPath)))
                Directory.CreateDirectory(Tool.UnityPathToSystemPath(UIGenerator.Instance.ScriptPath + UIFactory.UIPrefabPath));

            List<string> uiPrefabs = Tool.AssetPathOfUnityFolder(UIGenerator.Instance.PrefabPath + UIFactory.UIPrefabPath, false, ".prefab");
            foreach (var v in uiPrefabs)
                addUIBase(v);
            AssetDatabase.Refresh();
        }

        private void addUIBase(string prefabPath)
        {
            var importer = AssetImporter.GetAtPath(prefabPath);
            if (importer.assetBundleName == null)
            {
                importer.assetBundleName = UIFactory.UIPrefabPath.ToLower();
                importer.SaveAndReimport();
            }
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (go.GetComponent<UIBase>() == null)
                go.AddComponent<UIBase>();
            if (!Directory.Exists(Tool.UnityPathToSystemPath(UIGenerator.Instance.ScriptPath + UIFactory.UIPrefabPath)))
                Directory.CreateDirectory(Tool.UnityPathToSystemPath(UIGenerator.Instance.ScriptPath + UIFactory.UIPrefabPath));
            var systemPath = Tool.UnityPathToSystemPath(UIGenerator.Instance.ScriptPath + UIFactory.UIPrefabPath + "/" + go.name + ".txt");
            if (!File.Exists(systemPath))
            {
                generateLuaScripts(systemPath);
                AssetDatabase.Refresh();
                importer = AssetImporter.GetAtPath("Assets/" + UIGenerator.Instance.ScriptPath + UIFactory.UIPrefabPath + "/" + go.name + ".txt");
                importer.assetBundleName = "lua_ui";
            }
            importer.SaveAndReimport();
        }

        private void generateLuaScripts(string systemPath)
        {
            using(var fw = new FileWriter(systemPath))
            {
                fw.Append("function start()");
                fw.Append("    ");
                fw.Append("end");
                fw.Append("");
                fw.Append("function destroy()");
                fw.Append("    ");
                fw.Append("end");
                fw.Append("");
                fw.Append("function onShow()");
                fw.Append("    ");
                fw.Append("end");
                fw.Append("");
                fw.Append("function onHide()");
                fw.Append("    ");
                fw.Append("end");
                fw.Append("");
                fw.Append("function onPause()");
                fw.Append("    ");
                fw.Append("end");
                fw.Append("");
                fw.Append("function onResume()");
                fw.Append("    ");
                fw.Append("end");
                fw.Append("");
                fw.Append("function onEventTriiger(eventType, parms)");
                fw.Append("    ");
                fw.Append("end");
            }
        }
    }
}
