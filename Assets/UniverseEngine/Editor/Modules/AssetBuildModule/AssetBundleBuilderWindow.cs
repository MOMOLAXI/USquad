using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UniverseEngine.Editor
{
	public class AssetBundleBuilderWindow : UniverseEditorWindow
	{
		readonly List<string> m_EncryptionServicesClassNames = new();
		List<string> m_BuildPackageNames;

		Button m_SaveButton;
		Button m_ImportButton;
		Button m_ExportButton;
		Button m_RefreshButton;
		TextField m_BuildOutputField;
		EnumField m_BuildPipelineField;
		EnumField m_BuildModeField;
		TextField m_BuildVersionField;
		IntegerField m_MajorVersionField;
		IntegerField m_SubVersionField;
		IntegerField m_RevisionVersionField;
		PopupField<string> m_BuildPackageField;
		EnumField m_EncryptionField;
		EnumField m_CompressionField;
		EnumField m_OutputNameStyleField;
		EnumField m_CopyBuildinFileOptionField;
		TextField m_CopyBuildinFileTagsField;

		public void CreateGUI()
		{
			try
			{
				VisualElement root = rootVisualElement;

				// 加载布局文件
				VisualTreeAsset visualAsset = WindowModule.LoadWindowUxml<AssetBundleBuilderWindow>();
				if (visualAsset == null)
					return;

				visualAsset.CloneTree(root);

				// 配置保存按钮
				m_SaveButton = root.Q<Button>("SaveButton");
				m_SaveButton.clicked += SaveButtonClicked;
				
				m_RefreshButton = root.Q<Button>("RefreshButton");
				m_RefreshButton.clicked += RefreshWindow;
				
				m_ImportButton = root.Q<Button>("ImportButton");
				m_ImportButton.clicked += ImportButtonOnclicked;

				m_ExportButton = root.Q<Button>("ExportButton");
				m_ExportButton.clicked += AssetBuildModule.ExportBuildArgumentsToXml;
				
				// 包裹名称列表
				m_BuildPackageNames = AssetCollectModule.GetBuildPackageNames();

				// 加密服务类
				AssetEncryptionSystem.GetEncryptionNames(m_EncryptionServicesClassNames);

				// 输出目录
				string defaultOutputRoot = FileSystem.GetBundleOutputDirectory();
				m_BuildOutputField = root.Q<TextField>("BuildOutput");
				m_BuildOutputField.SetValueWithoutNotify(defaultOutputRoot);
				m_BuildOutputField.SetEnabled(false);

				// 构建管线
				m_BuildPipelineField = root.Q<EnumField>("BuildPipeline");
				m_BuildPipelineField.Init(AssetBuildModule.Setting.BuildPipeline);
				m_BuildPipelineField.SetValueWithoutNotify(AssetBuildModule.Setting.BuildPipeline);
				m_BuildPipelineField.style.width = 350;
				m_BuildPipelineField.RegisterValueChangedCallback(_ =>
				{
					AssetBuildModule.Setting.BuildPipeline = (EBuildPipeline)m_BuildPipelineField.value;
					AssetBuildModule.Setting.Save();
					RefreshWindow();
				});

				// 构建模式
				m_BuildModeField = root.Q<EnumField>("BuildMode");
				m_BuildModeField.Init(AssetBuildModule.Setting.BuildMode);
				m_BuildModeField.SetValueWithoutNotify(AssetBuildModule.Setting.BuildMode);
				m_BuildModeField.style.width = 350;
				m_BuildModeField.RegisterValueChangedCallback(_ =>
				{
					AssetBuildModule.Setting.BuildMode = (EBuildMode)m_BuildModeField.value;
					AssetBuildModule.Setting.Save();
					RefreshWindow();
				});

				// 构建版本
				m_BuildVersionField = root.Q<TextField>("BuildVersion");
				m_BuildVersionField.SetValueWithoutNotify(AssetBuildModule.Setting.Version);
				
				m_MajorVersionField = root.Q<IntegerField>("MajorVersion");
				m_MajorVersionField.SetValueWithoutNotify(AssetBuildModule.Setting.MajorVersion);
				m_MajorVersionField.RegisterValueChangedCallback(_ =>
				{
					AssetBuildModule.Setting.MajorVersion = m_MajorVersionField.value;
					m_BuildVersionField.SetValueWithoutNotify(AssetBuildModule.Setting.Version);
					AssetBuildModule.Setting.Save();
					RefreshWindow();
				});
				
				m_SubVersionField = root.Q<IntegerField>("SubVersion");
				m_SubVersionField.SetValueWithoutNotify(AssetBuildModule.Setting.SubVersion);
				m_SubVersionField.RegisterValueChangedCallback(_ =>
				{
					AssetBuildModule.Setting.SubVersion = m_SubVersionField.value;
					m_BuildVersionField.SetValueWithoutNotify(AssetBuildModule.Setting.Version);
					AssetBuildModule.Setting.Save();
					RefreshWindow();
				});
				
				m_RevisionVersionField = root.Q<IntegerField>("RevisionVersion");
				m_RevisionVersionField.SetValueWithoutNotify(AssetBuildModule.Setting.RevisionVersion);
				m_RevisionVersionField.RegisterValueChangedCallback(_ =>
				{
					AssetBuildModule.Setting.RevisionVersion = m_RevisionVersionField.value;
					m_BuildVersionField.SetValueWithoutNotify(AssetBuildModule.Setting.Version);
					AssetBuildModule.Setting.Save();
					RefreshWindow();
				});

				// 构建包裹
				VisualElement buildPackageContainer = root.Q("BuildPackageContainer");
				if (m_BuildPackageNames.Count > 0)
				{
					int defaultIndex = GetDefaultPackageIndex(AssetBuildModule.Setting.BuildPackage);
					m_BuildPackageField = new(m_BuildPackageNames, defaultIndex)
					{
						label = "Build Package",
						style =
						{
							width = 350
						}
					};
					m_BuildPackageField.RegisterValueChangedCallback(_ =>
					{
						AssetBuildModule.Setting.BuildPackage = m_BuildPackageField.value;
						AssetBuildModule.Setting.Save();
					});
					buildPackageContainer.Add(m_BuildPackageField);
				}
				else
				{
					m_BuildPackageField = new()
					{
						label = "Build Package",
						style =
						{
							width = 350
						}
					};
					buildPackageContainer.Add(m_BuildPackageField);
				}

				//加密方法
				m_EncryptionField = root.Q<EnumField>("Encryption");
				m_EncryptionField.Init(AssetBuildModule.Setting.EncryptionType);
				m_EncryptionField.SetValueWithoutNotify(AssetBuildModule.Setting.EncryptionType);
				m_EncryptionField.style.width = 350;
				m_EncryptionField.RegisterValueChangedCallback(_ =>
				{
					AssetBuildModule.Setting.EncryptionType = (EAssetEncryption)m_EncryptionField.value;
					AssetBuildModule.Setting.Save();
				});

				// 压缩方式选项
				m_CompressionField = root.Q<EnumField>("Compression");
				m_CompressionField.Init(AssetBuildModule.Setting.CompressOption);
				m_CompressionField.SetValueWithoutNotify(AssetBuildModule.Setting.CompressOption);
				m_CompressionField.style.width = 350;
				m_CompressionField.RegisterValueChangedCallback(_ =>
				{
					AssetBuildModule.Setting.CompressOption = (ECompressOption)m_CompressionField.value;
					AssetBuildModule.Setting.Save();
				});

				// 输出文件名称样式
				m_OutputNameStyleField = root.Q<EnumField>("OutputNameStyle");
				m_OutputNameStyleField.Init(AssetBuildModule.Setting.OutputNameStyle);
				m_OutputNameStyleField.SetValueWithoutNotify(AssetBuildModule.Setting.OutputNameStyle);
				m_OutputNameStyleField.style.width = 350;
				m_OutputNameStyleField.RegisterValueChangedCallback(_ =>
				{
					AssetBuildModule.Setting.OutputNameStyle = (EAssetOutputNameStyle)m_OutputNameStyleField.value;
					AssetBuildModule.Setting.Save();
				});

				// 首包文件拷贝选项
				m_CopyBuildinFileOptionField = root.Q<EnumField>("CopyBuildinFileOption");
				m_CopyBuildinFileOptionField.Init(AssetBuildModule.Setting.CopyBuildinFileOption);
				m_CopyBuildinFileOptionField.SetValueWithoutNotify(AssetBuildModule.Setting.CopyBuildinFileOption);
				m_CopyBuildinFileOptionField.style.width = 350;
				m_CopyBuildinFileOptionField.RegisterValueChangedCallback(_ =>
				{
					AssetBuildModule.Setting.CopyBuildinFileOption = (ECopyBuildinFileOption)m_CopyBuildinFileOptionField.value;
					AssetBuildModule.Setting.Save();
					RefreshWindow();
				});

				// 首包文件的资源标签
				m_CopyBuildinFileTagsField = root.Q<TextField>("CopyBuildinFileTags");
				m_CopyBuildinFileTagsField.SetValueWithoutNotify(AssetBuildModule.Setting.CopyBuildinFileTags);
				m_CopyBuildinFileTagsField.RegisterValueChangedCallback(_ =>
				{
					AssetBuildModule.Setting.CopyBuildinFileTags = m_CopyBuildinFileTagsField.value;
					AssetBuildModule.Setting.Save();
				});

				// 构建按钮
				Button buildButton = root.Q<Button>("Build");
				buildButton.clicked += BuildButtonClicked;
				
				RefreshWindow();
			}
			catch (Exception e)
			{
				Debug.LogError(e.ToString());
			}
		}
		void ImportButtonOnclicked()
		{
			string resultPath = WindowModule.OpenFilePath("Import XML", "Assets/", "xml");
			if (resultPath != null)
			{
				AssetBuildModule.ImportSettingFromXml(resultPath);
				RefreshWindow();
			}
		}
		
		public void OnDestroy()
		{
			AssetBuildModule.Setting.Save();
		}

		void RefreshWindow()
		{
			EBuildPipeline buildPipeline = AssetBuildModule.Setting.BuildPipeline;
			EBuildMode buildMode = AssetBuildModule.Setting.BuildMode;
			ECopyBuildinFileOption copyOption = AssetBuildModule.Setting.CopyBuildinFileOption;
			bool enableElement = buildMode == EBuildMode.ForceRebuild;
			bool tagsFiledVisible = copyOption is ECopyBuildinFileOption.ClearAndCopyByTags or ECopyBuildinFileOption.OnlyCopyByTags;

			if (buildPipeline == EBuildPipeline.BuiltinBuildPipeline)
			{
				m_CompressionField.SetEnabled(enableElement);
				m_OutputNameStyleField.SetEnabled(enableElement);
				m_CopyBuildinFileOptionField.SetEnabled(enableElement);
				m_CopyBuildinFileTagsField.SetEnabled(enableElement);
			}
			else
			{
				m_CompressionField.SetEnabled(true);
				m_OutputNameStyleField.SetEnabled(true);
				m_CopyBuildinFileOptionField.SetEnabled(true);
				m_CopyBuildinFileTagsField.SetEnabled(true);
			}
			
			m_CopyBuildinFileTagsField.visible = tagsFiledVisible;
			m_BuildPipelineField.SetValueWithoutNotify(AssetBuildModule.Setting.BuildPipeline);
			m_BuildModeField.SetValueWithoutNotify(AssetBuildModule.Setting.BuildMode);
			m_BuildVersionField.SetValueWithoutNotify(AssetBuildModule.Setting.Version);
			m_MajorVersionField.SetValueWithoutNotify(AssetBuildModule.Setting.MajorVersion);
			m_SubVersionField.SetValueWithoutNotify(AssetBuildModule.Setting.SubVersion);
			m_RevisionVersionField.SetValueWithoutNotify(AssetBuildModule.Setting.RevisionVersion);
			m_BuildPackageField.value = AssetBuildModule.Setting.BuildPackage;
			m_EncryptionField.SetValueWithoutNotify(AssetBuildModule.Setting.EncryptionType);
			m_CompressionField.SetValueWithoutNotify(AssetBuildModule.Setting.CompressOption);
			m_OutputNameStyleField.SetValueWithoutNotify(AssetBuildModule.Setting.OutputNameStyle);
			m_CopyBuildinFileOptionField.SetValueWithoutNotify(AssetBuildModule.Setting.CopyBuildinFileOption);
			m_CopyBuildinFileTagsField.SetValueWithoutNotify(AssetBuildModule.Setting.CopyBuildinFileTags);
		}

		static void SaveButtonClicked()
		{
			AssetBuildModule.Setting.Save();
		}

		static void BuildButtonClicked()
		{
			EBuildMode buildMode = AssetBuildModule.Setting.BuildMode;
			if (EditorUtility.DisplayDialog("提示", $"通过构建模式【{buildMode}】来构建！", "Yes", "No"))
			{
				WindowModule.ClearUnityConsole();
				UniverseEditor.ExecuteDelay(ExecuteBuild);
			}
			else
			{
				Debug.LogWarning("[Build] 打包已经取消");
			}
		}

		/// <summary>
		/// 执行构建
		/// </summary>
		static void ExecuteBuild()
		{
			BuildResult buildResult = AssetBuildModule.StartBuild();
			if (buildResult.IsSuccess)
			{
				EditorUtility.RevealInFinder(buildResult.OutputPackageDirectory);
			}
		}

		// 构建包裹相关
		int GetDefaultPackageIndex(string packageName)
		{
			for (int index = 0; index < m_BuildPackageNames.Count; index++)
			{
				if (m_BuildPackageNames[index] == packageName)
				{
					return index;
				}
			}

			AssetBuildModule.Setting.BuildPackage = m_BuildPackageNames[0];
			return 0;
		}
	}
}
