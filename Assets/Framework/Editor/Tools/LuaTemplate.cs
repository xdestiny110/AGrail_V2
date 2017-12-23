using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Framework
{
    public class LuaTemplate : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            Object obj = CreateAssetFromTemplate(pathName, resourceFile);
            ProjectWindowUtil.ShowCreatedAsset(obj);
        }

        static Object CreateAssetFromTemplate(string pahtName, string resourceFile)
        {
            string fullName = Path.GetFullPath(pahtName);
            StreamWriter writer = new StreamWriter(fullName, false, System.Text.Encoding.UTF8);
            writer.Write("");
            writer.Close();

            AssetDatabase.ImportAsset(pahtName);
            AssetDatabase.Refresh();

            return AssetDatabase.LoadAssetAtPath(pahtName, typeof(Object));
        }
    }
}
