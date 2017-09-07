using UnityEngine;
using UnityEditor;

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
    }
}