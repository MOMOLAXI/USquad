using System;
using UnityEngine;

namespace UniverseEngine.Editor
{
	public abstract class UniverseManagedSetting : ScriptableObject
	{
		public Action<UniverseManagedSetting> MarkDirty;
		
		public void Save()
		{
			MarkDirty?.Invoke(this);
		}
	}
}
