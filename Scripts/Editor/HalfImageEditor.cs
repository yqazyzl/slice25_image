#if UNITY_EDITOR
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEngine;

namespace Slice25Image.Editor
{
    [CustomEditor(typeof(Runtime.HalfImage))]
    public class HalfImageEditor : UnityEditor.Editor
    {
        private SerializedProperty _spSprite;
        private SerializedProperty _spColor;
        private SerializedProperty _spMaterial;
        private SerializedProperty _left;
        private SerializedProperty _right;
        private SerializedProperty _top;
        //private SerializedProperty _bottom;

        private void OnEnable()
        {
            _spSprite = serializedObject.FindProperty("m_Sprite");
            _spColor = serializedObject.FindProperty("m_Color");
            _spMaterial = serializedObject.FindProperty("m_Material");
            //_left = serializedObject.FindProperty("L");
            //_right = serializedObject.FindProperty("R");
            _top = serializedObject.FindProperty("TopY");
            //_bottom = serializedObject.FindProperty("B");

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
            if (_spSprite.objectReferenceValue != null) {
                EditorGUILayout.LabelField("Half Offset Setting", EditorStyles.boldLabel);
                Sprite sprite = _spSprite.objectReferenceValue as Sprite;
                int w = (int)sprite.rect.width;
                int h = (int)sprite.rect.height;

                //if (_left.intValue < 0) _left.intValue = 0;
                //if (_right.intValue < 0) _right.intValue = 0;
                if (_top.intValue < 0) _top.intValue = 0;
                //if (_bottom.intValue < 0) _bottom.intValue = 0;

                //UpdateIntValue(_left, _right, w);
                //UpdateIntValue(_right, _left, w);
                //UpdateIntValue(_top, _bottom, h);
                //UpdateIntValue(_bottom, _top, h);
                UpdateIntValue(_top, h);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateIntValue(SerializedProperty a, SerializedProperty b, int maxValue) {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(a);
            if (EditorGUI.EndChangeCheck())
            {
                if (a.intValue < 0) a.intValue = 0;
                if (a.intValue > maxValue) a.intValue = maxValue;
                if (a.intValue + b.intValue > maxValue) b.intValue = maxValue - a.intValue;
            }
        }

        private void UpdateIntValue(SerializedProperty a, int maxValue)
        {
            EditorGUI.BeginChangeCheck();
            //EditorGUILayout.PropertyField(a);
            EditorGUILayout.IntSlider(a, 0, maxValue, new GUIContent("TopY"));
            if (EditorGUI.EndChangeCheck())
            {
                if (a.intValue < 0) a.intValue = 0;
                if (a.intValue > maxValue) a.intValue = maxValue;
            }
        }
    }
}
#endif
