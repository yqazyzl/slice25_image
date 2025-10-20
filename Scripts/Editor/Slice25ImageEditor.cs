#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Slice25Image.Editor
{
    [CustomEditor(typeof(Runtime.Slice25Image))]
    public class Slice25ImageEditor : UnityEditor.Editor
    {
        private SerializedProperty _spSprite;
        private SerializedProperty _spColor;
        private SerializedProperty _spMaterial;
        private SerializedProperty horizontalRatioProp;
        private SerializedProperty verticalRatioProp;

        private void OnEnable()
        {
            _spSprite = serializedObject.FindProperty("m_Sprite");
            _spColor = serializedObject.FindProperty("m_Color");
            _spMaterial = serializedObject.FindProperty("m_Material");
            horizontalRatioProp = serializedObject.FindProperty("horizontalRatio");
            verticalRatioProp = serializedObject.FindProperty("verticalRatio");
        }

        public override void OnInspectorGUI()
        {
            // 1) Update the serialized object so we have the latest data
            serializedObject.Update();

            // 2) Draw the fields we want from the base Image
            EditorGUILayout.PropertyField(_spSprite, new GUIContent("Source Image"));
            EditorGUILayout.PropertyField(_spColor, new GUIContent("Color"));
            EditorGUILayout.PropertyField(_spMaterial, new GUIContent("Material"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Slice25Image Ratio Setting", EditorStyles.boldLabel);

            if (horizontalRatioProp != null)
                EditorGUILayout.Slider(horizontalRatioProp, 0f, 1f, new GUIContent("horizontal Ratio"));

            if (verticalRatioProp != null)
                EditorGUILayout.Slider(verticalRatioProp, 0f, 1f, new GUIContent("vertical Ratio"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
