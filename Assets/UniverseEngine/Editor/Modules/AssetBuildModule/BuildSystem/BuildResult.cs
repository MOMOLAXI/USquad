
namespace UniverseEngine.Editor
{
    /// <summary>
    /// 构建结果
    /// </summary>
    public struct BuildResult
    {
        /// <summary>
        /// 构建是否成功
        /// </summary>
        public bool IsSuccess;

        /// <summary>
        /// 构建失败的任务
        /// </summary>
        public string FailedTask;

        /// <summary>
        /// 构建失败的信息
        /// </summary>
        public string ErrorInfo;

        /// <summary>
        /// 输出的补丁包目录
        /// </summary>
        public string OutputPackageDirectory;
        
        public static BuildResult Unknown = new()
        {
            IsSuccess = true,
            FailedTask = string.Empty,
            ErrorInfo = string.Empty,
            OutputPackageDirectory = string.Empty
        };
    }
}