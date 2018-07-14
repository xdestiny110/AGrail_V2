using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Framework.UI
{
    [CustomEditor(typeof(ItemGenerator))]
    public class ItemGeneratorEditor : Editor
    {
        private const string prefabPath = "Item";

        [MenuItem("Framework/UI/Item Generator")]
        static void itemGenerator()
        {
            Selection.activeObject = ItemGenerator.Instance;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Generate Code"))
                generateItem();
        }

        private void generateItem()
        {
            if (!Directory.Exists(Tool.UnityPathToSystemPath(ItemGenerator.Instance.PrefabPath + prefabPath)))
                Directory.CreateDirectory(Tool.UnityPathToSystemPath(ItemGenerator.Instance.PrefabPath + prefabPath));
            if (!Directory.Exists(Tool.UnityPathToSystemPath(ItemGenerator.Instance.ScriptPath + prefabPath)))
                Directory.CreateDirectory(Tool.UnityPathToSystemPath(ItemGenerator.Instance.ScriptPath + prefabPath));

            List<string> uiPrefabs = Tool.AssetPathOfUnityFolder(ItemGenerator.Instance.PrefabPath + prefabPath, false, ".prefab");
            foreach (var v in uiPrefabs)
                addItemBase(v);
            AssetDatabase.Refresh();
        }

        private void addItemBase(string prefabPath)
        {
            var importer = AssetImporter.GetAtPath(prefabPath);
            if (importer.assetBundleName == null)
            {
                importer.assetBundleName = prefabPath.ToLower();
                importer.SaveAndReimport();
            }
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (go.GetComponent<ItemBase>() == null)
                go.AddComponent<ItemBase>();
            if (!Directory.Exists(Tool.UnityPathToSystemPath(ItemGenerator.Instance.ScriptPath + ItemGeneratorEditor.prefabPath)))
                Directory.CreateDirectory(Tool.UnityPathToSystemPath(ItemGenerator.Instance.ScriptPath + ItemGeneratorEditor.prefabPath));
            var systemPath = Tool.UnityPathToSystemPath(ItemGenerator.Instance.ScriptPath + ItemGeneratorEditor.prefabPath + "/" + go.name + ".txt");
            if (!File.Exists(systemPath))
            {
                generateLuaScripts(systemPath);
                AssetDatabase.Refresh();
                importer = AssetImporter.GetAtPath("Assets/" + ItemGenerator.Instance.ScriptPath + ItemGeneratorEditor.prefabPath + "/" + go.name + ".txt");
                importer.assetBundleName = "lua_ui";
            }
            importer.SaveAndReimport();
        }

        private void generateLuaScripts(string systemPath)
        {
            using (var fw = new FileWriter(systemPath))
            {
                fw.Append("function start()");
                fw.Append("    ");
                fw.Append("end");
                fw.Append("");
                fw.Append("function destroy()");
                fw.Append("    ");
                fw.Append("end");
                fw.Append("");
            }
        }
    }
}
