namespace UniverseEngine.Editor
{
    /// <summary>
    /// 编辑器工具类
    /// </summary>
    public partial class UniverseEditor
    {
        static UniverseSetting s_Setting;

        public static UniverseSetting Setting
        {
            get
            {
                UniverseSetting setting = SettingModule.GetSetting<UniverseSetting>();
                setting.BuildOptions.Scenes = GetBuiltInScenes();
                return setting;
            }
        }
    }
}
