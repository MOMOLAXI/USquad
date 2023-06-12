using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Window;
using UniFramework.Event;
using UniFramework.Machine;
using UniFramework.Singleton;
using UniverseEngine;

internal class FsmSceneBattle : IStateNode
{
	private BattleRoom _battleRoom;

	void IStateNode.OnCreate(StateMachine machine)
	{
	}
	void IStateNode.OnEnter()
	{
		UniSingleton.StartCoroutine(Prepare());
	}
	void IStateNode.OnUpdate()
	{
		if (_battleRoom != null)
			_battleRoom.UpdateRoom();
	}
	void IStateNode.OnExit()
	{
		if (_battleRoom != null)
		{
			_battleRoom.DestroyRoom();
			_battleRoom = null;
		}
	}

	private IEnumerator Prepare()
	{
		yield return AssetSystem.LoadSceneAsync("scene_battle");

		_battleRoom = new();
		yield return _battleRoom.LoadRoom();

		// 释放资源
		if (AssetSystem.TryGetPackage("DefaultPackage", out ResourcePackage package))
		{
			package.UnloadUnusedAssets();
		}
	}
}
