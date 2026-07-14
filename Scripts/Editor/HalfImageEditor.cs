#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Slice25Image.Editor
{
    [CustomEditor(typeof(Runtime.HalfImage))]
    [CanEditMultipleObjects]
    public class HalfImageEditor : UnityEditor.Editor
    {
        private SerializedProperty _spSprite;
        private SerializedProperty _spColor;
        private SerializedProperty _spMaterial;
        private SerializedProperty _spRaycastTarget;
        private SerializedProperty _spRaycastPadding;
        private SerializedProperty _spMaskable;
        private SerializedProperty _top;

        private void OnEnable()
        {
            _spSprite = serializedObject.FindProperty("m_Sprite");
            _spColor = serializedObject.FindProperty("m_Color");
            _spMaterial = serializedObject.FindProperty("m_Material");
            _spRaycastTarget = serializedObject.FindProperty("m_RaycastTarget");
            _spRaycastPadding = serializedObject.FindProperty("m_RaycastPadding");
            _spMaskable = serializedObject.FindProperty("m_Maskable");
            _top = serializedObject.FindProperty("TopY");
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
            if (_spSprite.objectReferenceValue != null)
            {
                EditorGUILayout.LabelField("Half Offset Setting", EditorStyles.boldLabel);
                Sprite sprite = _spSprite.objectReferenceValue as Sprite;
                int h = (int)sprite.rect.height;
                UpdateIntValue(_top, h);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateIntValue(SerializedProperty a, int maxValue)
        {
            EditorGUILayout.IntSlider(a, 0, maxValue, new GUIContent("Top Y"));
        }
    }
}
#endif
