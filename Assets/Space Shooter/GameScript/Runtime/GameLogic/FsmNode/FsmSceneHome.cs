using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using UniFramework.Window;
using UniFramework.Singleton;
using UniverseEngine;

internal class FsmSceneHome : IStateNode
{
	private StateMachine _machine;

	void IStateNode.OnCreate(StateMachine machine)
	{
		_machine = machine;
	}
	void IStateNode.OnEnter()
	{
		UniSingleton.StartCoroutine(Prepare());
	}
	void IStateNode.OnUpdate()
	{
	}
	void IStateNode.OnExit()
	{
		UniWindow.CloseWindow<UIHomeWindow>();
	}

	private IEnumerator Prepare()
	{
		yield return AssetSystem.LoadSceneAsync("scene_home");	
		yield return UniWindow.OpenWindowAsync<UIHomeWindow>("UIHome");

		// 释放资源
		if (AssetSystem.TryGetPackage("DefaultPackage", out ResourcePackage package))
		{
			package.UnloadUnusedAssets();
		}
	}
}