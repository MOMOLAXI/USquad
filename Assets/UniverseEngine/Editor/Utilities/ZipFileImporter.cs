using System.IO;
using UnityEditor;

namespace UniverseEngine.Editor
{
    public class ZipFileImporter : AssetPostprocessor
    {
        public const string EXTENSIONS = ".zip";

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string importedAsset in importedAssets)
            {
                string extension = Path.GetExtension(importedAsset);
                if (!EXTENSIONS.Contains(extension))
                {
                    continue;
                }

                if (Directory.Exists(importedAsset))
                {
                    continue;
                }
                
                byte[] binary = FileSystem.ReadAllBytes(importedAsset);
                if (binary == null)
                {
                    return;
                }

                string fileName = Path.ChangeExtension(importedAsset, ".bytes");
                EditorLog.Info($"Imported Zip file : {importedAsset}, Load binary and rename to {fileName}");
                FileSystem.CreateFile(fileName, binary);
                AssetDatabase.ImportAsset(fileName);
                AssetDatabase.DeleteAsset(importedAsset);
            }
        }
    }
}
