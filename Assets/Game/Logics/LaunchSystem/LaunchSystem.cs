using UniverseEngine;

namespace USquad
{
    public class LaunchSystem : GameSystem
    {
        // public async UniTaskVoid Launch(GameInitializeArguments args)
        // {
        //     if (args == null)
        //     {
        //         return;
        //     }
        //
        //     for (int i = 0; i < args.AssetPackageInfos.Count; i++)
        //     {
        //         AssetPackageInfo packageInfo = args.AssetPackageInfos[i];
        //         packageInfo.PlayMode = args.PlayMode;
        //         packageInfo.DefaultHostServer = args.DefaultHostServer;
        //         packageInfo.FallbackHostServer = args.FallbackHostServer;
        //         args.AssetPackageInfos[i] = packageInfo;
        //     }
        //
        //     await AssetSystem.InitializeAssetPackage(args.AssetPackageInfos);
        // }
    }
}
