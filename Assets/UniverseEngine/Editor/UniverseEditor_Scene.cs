using UnityEditor;

namespace UniverseEngine.Editor
{
    /// <summary>
    /// 编辑器工具类
    /// </summary>
    public partial class UniverseEditor
    {
        public static string[] GetBuiltInScenes()
        {
            return EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
        }
    }
}
