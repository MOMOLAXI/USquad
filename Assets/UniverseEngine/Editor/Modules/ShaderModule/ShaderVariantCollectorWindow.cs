using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace UniverseEngine.Editor
{
	public class ShaderVariantCollectorWindow : UniverseEditorWindow
	{
		List<string> m_PackageNames;

		Button m_SaveButton;
		Button m_CollectButton;
		TextField m_CollectOutputField;
		Label m_CurrentShaderCountField;
		Label m_CurrentVariantCountField;
		SliderInt m_ProcessCapacitySlider;
		PopupField<string> m_PackageField;

		public void CreateGUI()
		{
			try
			{
				VisualElement root = rootVisualElement;

				// 加载布局文件
				VisualTreeAsset visualAsset = WindowModule.LoadWindowUxml<ShaderVariantCollectorWindow>();
				if (visualAsset == null)
					return;

				visualAsset.CloneTree(root);

				// 配置保存按钮
				m_SaveButton = root.Q<Button>("SaveButton");
				m_SaveButton.clicked += SaveButtonClicked;

				// 包裹名称列表
				m_PackageNames = AssetCollectModule.GetBuildPackageNames();

				// 文件输出目录
				m_CollectOutputField = root.Q<TextField>("CollectOutput");
				m_CollectOutputField.SetValueWithoutNotify(ShaderModule.Setting.SavePath);
				m_CollectOutputField.RegisterValueChangedCallback(_ =>
				{
					ShaderModule.Setting.SavePath = m_CollectOutputField.value;
					ShaderModule.Setting.Save();
				});

				// 收集的包裹
				VisualElement packageContainer = root.Q("PackageContainer");
				if (m_PackageNames.Count > 0)
				{
					int defaultIndex = GetDefaultPackageIndex(ShaderModule.Setting.CollectPackage);
					m_PackageField = new(m_PackageNames, defaultIndex)
					{
						label = "Package",
						style =
						{
							width = 350
						}
					};
					m_PackageField.RegisterValueChangedCallback(_ =>
					{
						ShaderModule.Setting.CollectPackage = m_PackageField.value;
						ShaderModule.Setting.Save();
					});
					packageContainer.Add(m_PackageField);
				}
				else
				{
					m_PackageField = new()
					{
						label = "Package",
						style =
						{
							width = 350
						}
					};
					packageContainer.Add(m_PackageField);
				}

				// 容器值
				m_ProcessCapacitySlider = root.Q<SliderInt>("ProcessCapacity");
				m_ProcessCapacitySlider.SetValueWithoutNotify(ShaderModule.Setting.ProcessCapacity);
				m_ProcessCapacitySlider.RegisterValueChangedCallback(_ =>
				{
					ShaderModule.Setting.ProcessCapacity = m_ProcessCapacitySlider.value;
					ShaderModule.Setting.Save();
				});

				m_CurrentShaderCountField = root.Q<Label>("CurrentShaderCount");
				m_CurrentVariantCountField = root.Q<Label>("CurrentVariantCount");

				// 变种收集按钮
				m_CollectButton = root.Q<Button>("CollectButton");
				m_CollectButton.clicked += CollectButtonClicked;
			}
			catch (Exception e)
			{
				Debug.LogError(e.ToString());
			}
		}
		public void OnDestroy()
		{
			ShaderModule.Setting.Save();
		}

		void Update()
		{
			if (m_CurrentShaderCountField != null)
			{
				int currentShaderCount = ShaderModule.GetCurrentShaderVariantCollectionShaderCount();
				m_CurrentShaderCountField.text = $"Current Shader Count : {currentShaderCount}";
			}

			if (m_CurrentVariantCountField != null)
			{
				int currentVariantCount = ShaderModule.GetCurrentShaderVariantCollectionVariantCount();
				m_CurrentVariantCountField.text = $"Current Variant Count : {currentVariantCount}";
			}
		}

		static void SaveButtonClicked()
		{
			ShaderModule.Setting.Save();
		}

		void CollectButtonClicked()
		{
			string savePath = ShaderModule.Setting.SavePath;
			string packageName = ShaderModule.Setting.CollectPackage;
			int processCapacity = m_ProcessCapacitySlider.value;
			ShaderModule.StartCollect(savePath, packageName, processCapacity, null);
		}

		// 构建包裹相关
		private int GetDefaultPackageIndex(string packageName)
		{
			for (int index = 0; index < m_PackageNames.Count; index++)
			{
				if (m_PackageNames[index] == packageName)
				{
					return index;
				}
			}

			ShaderModule.Setting.CollectPackage = m_PackageNames[0];
			return 0;
		}
	}
}
