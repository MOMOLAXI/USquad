#if ODIN_INSPECTOR || ODIN_INSPECTOR_3
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
#endif

namespace UniverseEngine.Editor
{
#if ODIN_INSPECTOR || ODIN_INSPECTOR_3
    public class UniverseEditorWindow : OdinEditorWindow
    {
        [MenuItem("UniverseEngine/UniverseEditor")]
        static void OpenBuildWindow()
        {
            GetWindow<UniverseEditorWindow>();
        }
        
        protected override IEnumerable<object> GetTargets()
        {
            yield return UniverseEditor.Setting;
        }

        protected override void DrawEditor(int index)
        {
            SirenixEditorGUI.Title(title: "Build Player",
                                   subtitle: $"Setting from {nameof(UniverseSetting)}",
                                   textAlignment: TextAlignment.Left,
                                   horizontalLine: true);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Build Settings", GUILayout.Width(200), GUILayout.Height(40)))
            {
                UniverseEditor.SaveSetting();
            }

            if (GUILayout.Button("资源构建", GUILayout.Width(200), GUILayout.Height(40)))
            {
                UniverseMenuItems.ShowBuilderWindow();
            }
            
            if (GUILayout.Button("资源收集", GUILayout.Width(200), GUILayout.Height(40)))
            {
                AssetBundleCollectorWindow.ShowCollectorWindow();
            }
            
            if (GUILayout.Button("资源调试", GUILayout.Width(200), GUILayout.Height(40)))
            {
                AssetBundleDebuggerWindow.ShowDebugWindow();
            }
            
            if (GUILayout.Button("资源报告", GUILayout.Width(200), GUILayout.Height(40)))
            {
                AssetBundleReporterWindow.ShowReporterWindow();
            }
            
            if (GUILayout.Button("打包UnityPlayer", GUILayout.Width(200), GUILayout.Height(40)))
            {
                UniverseEditor.Execute(() =>
                {
                    UniverseEditor.SaveSetting();
                    UniverseEditor.FastBuildPlayer();
                });
            }
      
            GUILayout.EndHorizontal();
            
            SirenixEditorGUI.DrawThickHorizontalSeparator(2, 10);
            base.DrawEditor(index);
        }
    }
#endif


}
