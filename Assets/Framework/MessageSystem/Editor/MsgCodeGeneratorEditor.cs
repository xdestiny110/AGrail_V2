using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Framework.Message
{
    [CustomEditor(typeof(MsgCodeGenerator))]
    public class MsgCodeGeneratorEditor : Editor
    {
        private ReorderableList constList;
        private ReorderableList protoList;
        private ReorderableList customList;
        private bool foldConstList;
        private bool foldProtoList;
        private bool foldCustomList;

        [MenuItem("Framework/Message Code Generator")]
        public static void generateCode()
        {
            Selection.activeObject = MsgCodeGenerator.Instance;
        }

        void OnEnable()
        {
            buildReorderableList(ref constList, "msgTypesConst", "Const Message Type", false);
            buildReorderableList(ref protoList, "msgTypesProto", "Protobuf Message Type", false);
            buildReorderableList(ref customList, "msgTypes", "Custom Message Type");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            foldConstList = EditorGUILayout.Foldout(foldConstList, "Const Message Type");
            if (foldConstList)
                constList.DoLayoutList();
            foldProtoList = EditorGUILayout.Foldout(foldProtoList, "Protobuf Message Type");
            if (foldProtoList)
                protoList.DoLayoutList();
            foldCustomList = EditorGUILayout.Foldout(foldCustomList, "Custom Message Type");
            if(foldCustomList)
                customList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private void buildReorderableList(ref ReorderableList list, string propName, string header, bool enable = true)
        {
            var prop = serializedObject.FindProperty(propName);
            list = new ReorderableList(serializedObject, prop, true, true, enable, enable);
            list.drawHeaderCallback = (rect) => { EditorGUI.LabelField(rect, header); };
            list.drawElementCallback = (rect, idx, isActive, isFocused) =>
            {
                var element = prop.GetArrayElementAtIndex(idx);
                rect.y += 2;
                if (!enable)
                    GUI.enabled = false;
                EditorGUI.PropertyField(rect, element, GUIContent.none);
                if (!enable)
                    GUI.enabled = true;
            };
        }
    }
}


