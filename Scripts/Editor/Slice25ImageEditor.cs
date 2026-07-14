#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Slice25Image.Editor
{
    [CustomEditor(typeof(Runtime.Slice25Image))]
    [CanEditMultipleObjects]
    public class Slice25ImageEditor : UnityEditor.Editor
    {
        private SerializedProperty _spSprite;
        private SerializedProperty _spColor;
        private SerializedProperty _spMaterial;
        private SerializedProperty _spRaycastTarget;
        private SerializedProperty _spRaycastPadding;
        private SerializedProperty _spMaskable;
        private SerializedProperty horizontalRatioProp;
        private SerializedProperty verticalRatioProp;

        private void OnEnable()
        {
            _spSprite = serializedObject.FindProperty("m_Sprite");
            _spColor = serializedObject.FindProperty("m_Color");
            _spMaterial = serializedObject.FindProperty("m_Material");
            _spRaycastTarget = serializedObject.FindProperty("m_RaycastTarget");
            _spRaycastPadding = serializedObject.FindProperty("m_RaycastPadding");
            _spMaskable = serializedObject.FindProperty("m_Maskable");
            horizontalRatioProp = serializedObject.FindProperty("horizontalRatio");
            verticalRatioProp = serializedObject.FindProperty("verticalRatio");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_spSprite, new GUIContent("Source Image"));
            EditorGUILayout.PropertyField(_spColor, new GUIContent("Color"));
            EditorGUILayout.PropertyField(_spMaterial, new GUIContent("Material"));
            EditorGUILayout.PropertyField(_spRaycastTarget, new GUIContent("Raycast Target"));
            EditorGUILayout.PropertyField(_spRaycastPadding, new GUIContent("Raycast Padding"));
            EditorGUILayout.PropertyField(_spMaskable, new GUIContent("Maskable"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Slice25Image Ratio Setting", EditorStyles.boldLabel);

            if (horizontalRatioProp != null)
                EditorGUILayout.Slider(horizontalRatioProp, 0f, 1f, new GUIContent("Horizontal Ratio"));

            if (verticalRatioProp != null)
                EditorGUILayout.Slider(verticalRatioProp, 0f, 1f, new GUIContent("Vertical Ratio"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
