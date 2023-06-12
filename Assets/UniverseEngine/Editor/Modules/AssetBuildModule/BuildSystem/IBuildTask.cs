
namespace UniverseEngine.Editor
{
    public interface IBuildTask
    {
        EBuildMode[] IgnoreBuildModes { get; }
        
        void Run(BuildContext context);

        string GetDisplayName();
    }
}