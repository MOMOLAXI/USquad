using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using UniverseEngine;

/// <summary>
/// 清理未使用的缓存文件
/// </summary>
internal class FsmClearCache : IStateNode
{
	private StateMachine _machine;

	void IStateNode.OnCreate(StateMachine machine)
	{
		_machine = machine;
	}
	void IStateNode.OnEnter()
	{
		PatchEventDefine.PatchStatesChange.SendEventMessage("清理未使用的缓存文件！");
		if (AssetSystem.TryGetPackage("DefaultPackage", out ResourcePackage package))
		{
			package.UnloadUnusedAssets();
			var operation = package.ClearUnusedCacheFilesAsync();
			operation.Completed += Operation_Completed;
		}
	}
	void IStateNode.OnUpdate()
	{
	}
	void IStateNode.OnExit()
	{
	}

	private void Operation_Completed(UniverseEngine.AsyncOperationBase obj)
	{
		_machine.ChangeState<FsmPatchDone>();
	}
}
