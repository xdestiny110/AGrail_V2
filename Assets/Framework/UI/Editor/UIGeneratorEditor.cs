using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Framework.UI
{
    [CustomEditor(typeof(UIGenerator))]
    public class UIGeneratorEditor : Editor
    {
        [MenuItem("Framework/UI Generator")]
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
            if (!Directory.Exists(EditorTool.UnityPathToSystemPath(UIGenerator.Instance.UIPrefabPath + UIFactory.UIPrefabPath)))
                Directory.CreateDirectory(EditorTool.UnityPathToSystemPath(UIGenerator.Instance.UIPrefabPath + UIFactory.UIPrefabPath));
            if(!Directory.Exists(EditorTool.UnityPathToSystemPath(UIGenerator.Instance.UIScriptPath + UIFactory.UIPrefabPath)))
                Directory.CreateDirectory(EditorTool.UnityPathToSystemPath(UIGenerator.Instance.UIScriptPath + UIFactory.UIPrefabPath));

            List<string> uiPrefabs = EditorTool.AssetPathOfUnityFolder(UIGenerator.Instance.UIPrefabPath + UIFactory.UIPrefabPath, false, ".prefab");
            UIGenerator.Instance.UITypes.Clear();
            foreach (var v in uiPrefabs)
                addUIBase(v);
            AssetDatabase.Refresh();
        }

        private void addUIBase(string prefabPath)
        {
            UIGenerator.Instance.UITypes.Add(prefabPath);
            var importer = AssetImporter.GetAtPath(prefabPath);
            if (importer.assetBundleName == null)
                importer.assetBundleName = UIFactory.UIPrefabPath.ToLower();
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (go.GetComponent<UIBase>() == null)
                go.AddComponent<UIBase>();
            var systemPath = EditorTool.UnityPathToSystemPath(UIGenerator.Instance.UIScriptPath + UIFactory.UIPrefabPath + "/" + go.name + ".txt");
            if (!File.Exists(systemPath))
            {
                generateLuaScripts(systemPath);
                AssetDatabase.Refresh();
                importer = AssetImporter.GetAtPath("Assets/" + UIGenerator.Instance.UIScriptPath + UIFactory.UIPrefabPath + "/" + go.name + ".txt");
                importer.assetBundleName = "lua_ui";
            }
            importer.SaveAndReimport();
        }

        private void generateLuaScripts(string systemPath)
        {
            using(var fw = new FileWriter(systemPath))
            {
                fw.Append("function awake()");
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
                fw.Append("function onEventTriiger()");
                fw.Append("    ");
                fw.Append("end");
            }
        }
    }
}
