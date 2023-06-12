using System;
using System.Collections.Generic;
using UnityEditor;

namespace UniverseEngine.Editor
{
	public class BuildContext
	{
		readonly Dictionary<Type, IContextObject> m_ContextObjects = new();

		public void SetContextObject<T>(ContextObject<T> contextObject)
		{
			Type type = typeof(T);
			m_ContextObjects[type] = contextObject;
		}

		public T GetContext<T>()
		{
			if (m_ContextObjects.TryGetValue(typeof(T), out IContextObject contextObject))
			{
				ContextObject<T> context = (ContextObject<T>)contextObject;
				return context;
			}

			return default;
		}

		public EBuildPipeline BuildPipeline { get; private set; }
		public EBuildMode BuildMode { get; private set; }
		public BuildTarget BuildTarget { get; private set; }
		public bool WriteLinkXML { get; private set; }
		public string CacheServerHost { get; private set; }
		public int CacheServerPort { get; private set; }
		public string OutputRoot { get; private set; }
		public string PackageName { get; private set; }
		public string PackageVersion { get; private set; }
		public bool VerifyBuildingResult { get; private set; }
		public EShareAssetPackRule ShareAssetPackRule { get; private set; }
		public EAssetEncryption EncryptionType { get; private set; }
		public EAssetOutputNameStyle OutputNameStyle { get; private set; }
		public ECopyBuildinFileOption CopyBuildinFileOption { get; private set; }
		public string CopyBuildinFileTags { get; private set; }
		public ECompressOption CompressOption { get; private set; }
		public bool DisableWriteTypeTree { get; private set; }
		public bool IgnoreTypeTreeChanges { get; private set; }

		public void Initialize(BuildArguments arguments)
		{
			BuildMode = arguments.BuildMode;
			BuildTarget = arguments.BuildTarget;
			BuildPipeline = arguments.BuildPipeline;
			WriteLinkXML = arguments.WriteLinkXML;
			CacheServerHost = arguments.CacheServerHost;
			CacheServerPort = arguments.CacheServerPort;
			OutputRoot = arguments.OutputRoot;
			PackageName = arguments.PackageName;
			PackageVersion = arguments.PackageVersion;
			VerifyBuildingResult = arguments.VerifyBuildingResult;
			ShareAssetPackRule = arguments.ShareAssetPackRule;
			EncryptionType = arguments.EncryptionType;
			OutputNameStyle = arguments.OutputNameStyle;
			CopyBuildinFileOption = arguments.CopyBuildinFileOption;
			CopyBuildinFileTags = arguments.CopyBuildinFileTags;
			CompressOption = arguments.CompressOption;
			DisableWriteTypeTree = arguments.DisableWriteTypeTree;
			IgnoreTypeTreeChanges = arguments.IgnoreTypeTreeChanges;
		}

		public BuildPipelineArgs GetPipelineArguments()
		{
			BuildPipelineArgs args = new();
			BuildParametersContext buildParametersContext = GetContextObject<BuildParametersContext>();
			BuildMapContext buildMapContext = GetContextObject<BuildMapContext>();
			args.OutputPath = buildParametersContext.GetPipelineOutputDirectory();
			args.Builds = buildMapContext.GetPipelineBuilds();
			args.AssetBundleOptions = buildParametersContext.GetPipelineBuildOptions();
			args.TargetPlatform = buildParametersContext.Arguments.BuildTarget;
			return args;
		}

		/// <summary>
		/// 设置情景对象
		/// </summary>
		public void SetContextObject(IContextObject contextObject)
		{
			if (contextObject == null)
			{
				throw new ArgumentNullException(nameof(contextObject));
			}

			Type type = contextObject.GetType();
			if (m_ContextObjects.ContainsKey(type))
			{
				throw new($"Context object {type} is already existed.");
			}

			m_ContextObjects.Add(type, contextObject);
		}

		/// <summary>
		/// 获取情景对象
		/// </summary>
		public T GetContextObject<T>() where T : IContextObject
		{
			Type type = typeof(T);
			if (m_ContextObjects.TryGetValue(type, out IContextObject contextObject))
			{
				return (T)contextObject;
			}

			throw new($"Not found context object : {type}");
		}
	}
}
