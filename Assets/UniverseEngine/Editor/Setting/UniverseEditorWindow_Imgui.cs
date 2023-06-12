// using UnityEditor;
// using UnityEngine;
//
// namespace UniverseEditor
// {
//     public class UniverseEditorWindowImGUI : EditorWindow
//     {
//         public Editor Settingditor;
//
//         [MenuItem("UniverseEngine/UniverseEditorImGUI")]
//         static void OpenBuildWindow()
//         {
//             UniverseEditorWindowImGUI window = GetWindow<UniverseEditorWindowImGUI>("UniverseEditor", true);
//             window.Settingditor = Editor.CreateEditor(UniverseEditor.Setting);
//         }
//
//         void OnGUI()
//         {
//             Settingditor.OnInspectorGUI();
//             if (GUILayout.Button("Save Build Settings", GUILayout.Height(40)))
//             {
//                 UniverseEditor.Setting.Save();
//             }
//
//             if (GUILayout.Button("资源构建", GUILayout.Height(40)))
//             {
//                 UniverseMenuItems.ShowAssetBuilderWindow();
//             }
//
//             if (GUILayout.Button("资源收集", GUILayout.Height(40)))
//             {
//                 UniverseMenuItems.ShowAssetCollectorWindow();
//             }
//
//             if (GUILayout.Button("资源调试", GUILayout.Height(40)))
//             {
//                 UniverseMenuItems.ShowAssetDebuggerWindow();
//             }
//
//             if (GUILayout.Button("资源报告", GUILayout.Height(40)))
//             {
//                 UniverseMenuItems.ShowAssetReporterWindow();
//             }
//
//             if (GUILayout.Button("打包UnityPlayer", GUILayout.Height(40)))
//             {
//                 UniverseEditor.ExecuteDelay(() =>
//                 {
//                     UniverseEditor.Setting.Save();
//                     UniverseEditor.FastBuildPlayer();
//                 });
//             }
//         }
//
//     }
// }
