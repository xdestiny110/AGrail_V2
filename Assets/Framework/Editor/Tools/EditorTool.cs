using UnityEngine;
using UnityEditor;
using System.IO;

namespace Framework
{
    public static class EditorTool
    {
        [MenuItem("Framework/Utils/Clean PlayerPref")]
        static void Clean()
        {
            Debug.Log("Clean PlayerPref");
            PlayerPrefs.DeleteAll();
        }

        [MenuItem("Framework/Utils/Get world position")]
        static void GetWorldPos()
        {
            var go = Selection.activeGameObject;
            Debug.LogFormat("World postion = {0}", go.transform.position);
        }

        [MenuItem("Assets/Create/Lua script")]
        static void CreateLuaScript()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<LuaTemplate>(),
                GetSelectedPathOrFallback() + "/New Lua.txt",
                null,
                null);
        }

        static string GetSelectedPathOrFallback()
        {
            string path = "Assets";
            foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }
    }
}