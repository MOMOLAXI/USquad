namespace UniverseEngine
{
    public static partial class UniverseEngine
    {
        /// <summary>
        /// 获取沙盒的根路径
        /// </summary>
        public static string GetSandboxRoot()
        {
            return FileSystem.GetPersistentRootPath();
        }

        /// <summary>
        /// 清空沙盒目录
        /// </summary>
        public static void ClearSandbox()
        {
            FileSystem.DeleteCacheSystem();
        }
    }
}