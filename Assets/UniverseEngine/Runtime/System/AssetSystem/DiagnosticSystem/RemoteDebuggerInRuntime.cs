using System;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;

namespace UniverseEngine
{
	internal class RemoteDebuggerInRuntime : MonoBehaviour
	{
		/// <summary>
		/// 编辑器下获取报告的回调
		/// </summary>
		public static Action<int, DebugReport> EditorHandleDebugReportCallback;

		/// <summary>
		/// 编辑器下请求报告数据
		/// </summary>
		public static void EditorRequestDebugReport()
		{
			if (Application.isPlaying && Application.isEditor)
			{
				DebugReport report = AssetSystem.GetDebugReport();
				EditorHandleDebugReportCallback?.Invoke(0, report);
			}
		}

		private void OnEnable()
		{
			PlayerConnection.instance.Register(RemoteDebuggerDefine.kMsgSendEditorToPlayer, OnHandleEditorMessage);
		}
		
		private void OnDisable()
		{
			PlayerConnection.instance.Unregister(RemoteDebuggerDefine.kMsgSendEditorToPlayer, OnHandleEditorMessage);
		}
		
		private void OnHandleEditorMessage(MessageEventArgs args)
		{
			RemoteCommand command = RemoteCommand.Deserialize(args.data);
			Log<AssetSystem>.Info($"On handle remote command : {command.CommandType} Param : {command.CommandParam}");
			if (command.CommandType == (int)ERemoteCommand.SampleOnce)
			{
				DebugReport debugReport = AssetSystem.GetDebugReport();
				byte[] data = DebugReport.Serialize(debugReport);
				PlayerConnection.instance.Send(RemoteDebuggerDefine.kMsgSendPlayerToEditor, data);
			}
			else
			{
				throw new NotImplementedException(command.CommandType.ToString());
			}
		}
	}
}
