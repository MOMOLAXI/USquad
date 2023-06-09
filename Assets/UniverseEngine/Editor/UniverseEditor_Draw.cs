﻿using UnityEditor;
using UnityEngine;

namespace UniverseEngine.Editor
{
    /// <summary>
    /// 编辑器工具类
    /// </summary>
    public partial class UniverseEditor
    {
        public static void DrawMonoBehaviourField<T>(T target) where T : MonoBehaviour
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour(target), typeof(T), false);
            GUI.enabled = true;
        }

        public static void DrawScriptableObjectField<T>(T target) where T : ScriptableObject
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromScriptableObject(target), typeof(T), false);
            GUI.enabled = true;
        }

        public static void DrawEditorLayoutHorizontalLine(Color color, int thickness = 1, int padding = 10)
        {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));

            rect.height = thickness;
            rect.y += (float)padding / 2;

            EditorGUI.DrawRect(rect, color);
        }

        public static void DrawEditorHorizontalLine(ref Rect rect, Color color, int thickness = 1, int padding = 10)
        {
            rect.height = thickness;
            rect.y += (float)padding / 2;

            EditorGUI.DrawRect(rect, color);

            rect.y += padding;
            rect.height = EditorGUIUtility.singleLineHeight;
        }

        public static void DrawWireCapsule(Vector3 position, Quaternion rotation, float radius, float height, Color color = default)
        {
            if (color != default)
            {
                Handles.color = color;
            }

            Matrix4x4 angleMatrix = Matrix4x4.TRS(position, rotation, Handles.matrix.lossyScale);

            using (new Handles.DrawingScope(angleMatrix))
            {
                float pointOffset = (height - (radius * 2)) / 2;

                //draw sideways
                Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, radius);
                Handles.DrawLine(new(0, pointOffset, -radius), new(0, -pointOffset, -radius));
                Handles.DrawLine(new(0, pointOffset, radius), new(0, -pointOffset, radius));
                Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, radius);

                //draw front ways
                Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, radius);
                Handles.DrawLine(new(-radius, pointOffset, 0), new(-radius, -pointOffset, 0));
                Handles.DrawLine(new(radius, pointOffset, 0), new(radius, -pointOffset, 0));
                Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, radius);

                //draw center
                Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, radius);
                Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, radius);
            }
        }


        public static void DrawArray(SerializedProperty rootProperty, string namePropertyString = null, bool showAddElement = true, bool removeIconToTheRight = true)
        {
            if (!rootProperty.isArray)
            {
                return;
            }

            for (int i = 0; i < rootProperty.arraySize; i++)
            {
                SerializedProperty item = rootProperty.GetArrayElementAtIndex(i);
                item.isExpanded = true;

                GUILayout.BeginVertical(EditorStyles.helpBox);

                DrawArrayElement(item, namePropertyString);

                GUILayout.Space(20);


                if (GUILayout.Button("Remove element", EditorStyles.miniButton))
                {
                    rootProperty.DeleteArrayElementAtIndex(i);
                    break;
                }

                GUILayout.EndVertical();

                GUILayout.Space(20);
            }


            if (showAddElement)
            {
                if (GUILayout.Button("Add element"))
                {
                    rootProperty.arraySize++;
                }
            }
        }

        public static void DrawArrayElement(SerializedProperty property, string namePropertyString = null, bool skipFirstChild = false)
        {
            EditorGUI.indentLevel++;

            SerializedProperty nameProperty = null;

            if (namePropertyString != null)
            {
                nameProperty = property.FindPropertyRelative(namePropertyString);
                if (nameProperty != null)
                {
                    string name = nameProperty.stringValue;

                    EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
                }

                EditorGUI.indentLevel++;
            }


            SerializedProperty itr = property.Copy();

            bool enterChildren = true;

            while (itr.Next(enterChildren))
            {
                if (SerializedProperty.EqualContents(itr, property.GetEndProperty()))
                    break;

                if (enterChildren && skipFirstChild)
                {
                    enterChildren = false;
                    continue;
                }

                EditorGUILayout.PropertyField(itr, enterChildren);

                enterChildren = false;
            }

            EditorGUI.indentLevel = nameProperty != null ? EditorGUI.indentLevel - 2 : EditorGUI.indentLevel - 1;
        }

        public static void DrawArrayElement(Rect rect, SerializedProperty property, string namePropertyString = null, bool skipFirstChild = false)
        {
            if (property.isArray)
            {
                return;
            }

            EditorGUI.indentLevel++;

            SerializedProperty nameProperty = null;

            if (namePropertyString != null)
            {
                nameProperty = property.FindPropertyRelative(namePropertyString);
                if (nameProperty != null)
                {
                    string name = nameProperty.stringValue;
                    EditorGUI.LabelField(rect, name, EditorStyles.boldLabel);
                    rect.y += rect.height;
                }

                EditorGUI.indentLevel++;
            }


            SerializedProperty itr = property.Copy();

            bool enterChildren = true;

            while (itr.Next(enterChildren))
            {
                if (SerializedProperty.EqualContents(itr, property.GetEndProperty()))
                {
                    break;
                }

                if (enterChildren && skipFirstChild)
                {
                    enterChildren = false;
                    continue;
                }

                EditorGUI.PropertyField(rect, itr, enterChildren);
                rect.y += rect.height;

                enterChildren = false;
            }

            EditorGUI.indentLevel = nameProperty != null ? EditorGUI.indentLevel - 2 : EditorGUI.indentLevel - 1;
        }

        public static void DebugRay(Vector3 point, Vector3 direction = default(Vector3), float duration = 2f, Color color = default(Color))
        {
            Vector3 drawDirection = direction == default ? Vector3.up : direction;
            Color drawColor = color == default ? Color.blue : color;

            Debug.DrawRay(point, drawDirection, drawColor, duration);
        }

        public static void DrawArrowGizmo(Vector3 start, Vector3 end, Color color, float radius = 0.25f)
        {
            Gizmos.color = color;
            Gizmos.DrawLine(start, end);

            Gizmos.DrawRay(end, Quaternion.AngleAxis(45, Vector3.forward) * Vector3.Normalize(start - end) * radius);

            Gizmos.DrawRay(end, Quaternion.AngleAxis(-45, Vector3.forward) * Vector3.Normalize(start - end) * radius);
        }

        public static void DrawGizmoCross(Vector3 point, float radius, Color color)
        {
            Gizmos.color = color;

            Gizmos.DrawRay(point + Vector3.up * 0.5f * radius, Vector3.down * radius);

            Gizmos.DrawRay(point + Vector3.right * 0.5f * radius, Vector3.left * radius);
        }

        public static void DrawDebugCross(Vector3 point, float radius, Color color, float angleOffset = 0f)
        {
            Debug.DrawRay(point + Quaternion.Euler(0, 0, angleOffset) * Vector3.up * 0.5f * radius,
                          Quaternion.Euler(0, 0, angleOffset) * Vector3.down * radius,
                          color);

            Debug.DrawRay(point + Quaternion.Euler(0, 0, angleOffset) * Vector3.right * 0.5f * radius,
                          Quaternion.Euler(0, 0, angleOffset) * Vector3.left * radius,
                          color);
        }
    }
}
