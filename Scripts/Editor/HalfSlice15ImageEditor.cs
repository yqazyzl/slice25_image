#if UNITY_EDITOR
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEngine;

namespace Slice25Image.Editor
{
    [CustomEditor(typeof(Runtime.HalfSlice15Image))]
    public class HalfSlice15ImageEditor : UnityEditor.Editor
    {
        private SerializedProperty _spSprite;
        private SerializedProperty _spColor;
        private SerializedProperty _spMaterial;
        private SerializedProperty _centerWidth;
        //private SerializedProperty _bottom;

        private void OnEnable()
        {
            _spSprite = serializedObject.FindProperty("m_Sprite");
            _spColor = serializedObject.FindProperty("m_Color");
            _spMaterial = serializedObject.FindProperty("m_Material");
            _centerWidth = serializedObject.FindProperty("m_CenterWidth");
        }

        public override void OnInspectorGUI()
        {
            // 1) Update the serialized object so we have the latest data
            serializedObject.Update();

            //// 2) Draw the fields we want from the base Image
            EditorGUILayout.PropertyField(_spSprite, new GUIContent("Source Image"));
            EditorGUILayout.PropertyField(_spColor, new GUIContent("Color"));
            EditorGUILayout.PropertyField(_spMaterial, new GUIContent("Material"));

            //EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_centerWidth, new GUIContent("CenterWidth"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
