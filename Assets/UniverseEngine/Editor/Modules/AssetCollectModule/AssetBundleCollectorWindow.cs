using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UniverseEngine.Editor
{
	public class AssetBundleCollectorWindow : UniverseEditorWindow
	{
		Button m_SaveButton;
		List<string> m_CollectorTypeList;
		List<RuleDisplayName> m_ActiveRuleList;
		List<RuleDisplayName> m_AddressRuleList;
		List<RuleDisplayName> m_PackRuleList;
		List<RuleDisplayName> m_FilterRuleList;

		Toggle m_ShowPackageToogle;
		Toggle m_EnableAddressableToogle;
		Toggle m_UniqueBundleNameToogle;
		Toggle m_ShowEditorAliasToggle;

		VisualElement m_PackageContainer;
		ListView m_PackageListView;
		TextField m_PackageNameTxt;
		TextField m_PackageDescTxt;

		VisualElement m_GroupContainer;
		ListView m_GroupListView;
		TextField m_GroupNameTxt;
		TextField m_GroupDescTxt;
		TextField m_GroupAssetTagsTxt;

		VisualElement m_CollectorContainer;
		ScrollView m_CollectorScrollView;
		PopupField<RuleDisplayName> m_ActiveRulePopupField;

		int m_LastModifyPackageIndex;
		int m_LastModifyGroupIndex;

		public void CreateGUI()
		{
			Undo.undoRedoPerformed -= RefreshWindow;
			Undo.undoRedoPerformed += RefreshWindow;

			try
			{
				m_CollectorTypeList = EnumUtilities.GetNames<ECollectorType>();
				m_ActiveRuleList = AssetCollectModule.GetRuleNames<IActiveRule>();
				m_AddressRuleList = AssetCollectModule.GetRuleNames<IAddressRule>();
				m_PackRuleList = AssetCollectModule.GetRuleNames<IPackRule>();
				m_FilterRuleList = AssetCollectModule.GetRuleNames<IFilterRule>();

				VisualElement root = rootVisualElement;

				// 加载布局文件
				VisualTreeAsset visualAsset = WindowModule.LoadWindowUxml<AssetBundleCollectorWindow>();
				if (visualAsset == null)
					return;

				visualAsset.CloneTree(root);

				// 公共设置相关
				m_ShowPackageToogle = root.Q<Toggle>("ShowPackages");
				m_ShowPackageToogle.RegisterValueChangedCallback(evt =>
				{
					AssetCollectModule.ModifyPackageView(evt.newValue);
					RefreshWindow();
				});
				m_EnableAddressableToogle = root.Q<Toggle>("EnableAddressable");
				m_EnableAddressableToogle.RegisterValueChangedCallback(evt =>
				{
					AssetCollectModule.ModifyAddressable(evt.newValue);
					RefreshWindow();
				});
				m_UniqueBundleNameToogle = root.Q<Toggle>("UniqueBundleName");
				m_UniqueBundleNameToogle.RegisterValueChangedCallback(evt =>
				{
					AssetCollectModule.ModifyUniqueBundleName(evt.newValue);
					RefreshWindow();
				});

				m_ShowEditorAliasToggle = root.Q<Toggle>("ShowEditorAlias");
				m_ShowEditorAliasToggle.RegisterValueChangedCallback(evt =>
				{
					AssetCollectModule.ModifyShowEditorAlias(evt.newValue);
					RefreshWindow();
				});

				// 配置修复按钮
				Button fixBtn = root.Q<Button>("FixButton");
				fixBtn.clicked += FixBtn_clicked;

				// 导入导出按钮
				Button exportBtn = root.Q<Button>("ExportButton");
				exportBtn.clicked += ExportButtonClicked;
				Button importBtn = root.Q<Button>("ImportButton");
				importBtn.clicked += ImportButtonClicked;

				// 配置保存按钮
				m_SaveButton = root.Q<Button>("SaveButton");
				m_SaveButton.clicked += SaveButtonClicked;

				// 包裹容器
				m_PackageContainer = root.Q("PackageContainer");

				// 包裹列表相关
				m_PackageListView = root.Q<ListView>("PackageListView");
				m_PackageListView.makeItem = MakePackageListViewItem;
				m_PackageListView.bindItem = BindPackageListViewItem;
				m_PackageListView.selectionChanged += PackageListView_onSelectionChange;

				// 包裹添加删除按钮
				VisualElement packageAddContainer = root.Q("PackageAddContainer");
				{
					Button addBtn = packageAddContainer.Q<Button>("AddBtn");
					addBtn.clicked += AddPackageButtonClicked;
					Button removeBtn = packageAddContainer.Q<Button>("RemoveBtn");
					removeBtn.clicked += RemovePackageButtonClicked;
				}

				// 包裹名称
				m_PackageNameTxt = root.Q<TextField>("PackageName");
				m_PackageNameTxt.RegisterValueChangedCallback(evt =>
				{
					if (m_PackageListView.selectedItem is AssetBundleCollectorPackage selectPackage)
					{
						selectPackage.PackageName = evt.newValue;
						AssetCollectModule.ModifyPackage(selectPackage);
						FillPackageViewData();
					}
				});

				// 包裹备注
				m_PackageDescTxt = root.Q<TextField>("PackageDesc");
				m_PackageDescTxt.RegisterValueChangedCallback(evt =>
				{
					if (m_PackageListView.selectedItem is AssetBundleCollectorPackage selectPackage)
					{
						selectPackage.PackageDesc = evt.newValue;
						AssetCollectModule.ModifyPackage(selectPackage);
						FillPackageViewData();
					}
				});

				// 分组列表相关
				m_GroupListView = root.Q<ListView>("GroupListView");
				m_GroupListView.makeItem = MakeGroupListViewItem;
				m_GroupListView.bindItem = BindGroupListViewItem;
				m_GroupListView.selectionChanged += GroupListView_onSelectionChange;

				// 分组添加删除按钮
				VisualElement groupAddContainer = root.Q("GroupAddContainer");
				{
					Button addBtn = groupAddContainer.Q<Button>("AddBtn");
					addBtn.clicked += AddGroupButtonClicked;
					Button removeBtn = groupAddContainer.Q<Button>("RemoveBtn");
					removeBtn.clicked += RemoveGroupButtonClicked;
				}

				// 分组容器
				m_GroupContainer = root.Q("GroupContainer");

				// 分组名称
				m_GroupNameTxt = root.Q<TextField>("GroupName");
				m_GroupNameTxt.RegisterValueChangedCallback(evt =>
				{
					if (m_PackageListView.selectedItem is AssetBundleCollectorPackage selectPackage
					 && m_GroupListView.selectedItem is AssetBundleCollectorGroup selectGroup)
					{
						selectGroup.GroupName = evt.newValue;
						AssetCollectModule.ModifyGroup(selectPackage, selectGroup);
						FillGroupViewData();
					}
				});

				// 分组备注
				m_GroupDescTxt = root.Q<TextField>("GroupDesc");
				m_GroupDescTxt.RegisterValueChangedCallback(evt =>
				{
					if (m_PackageListView.selectedItem is AssetBundleCollectorPackage selectPackage
					 && m_GroupListView.selectedItem is AssetBundleCollectorGroup selectGroup)
					{
						selectGroup.GroupDesc = evt.newValue;
						AssetCollectModule.ModifyGroup(selectPackage, selectGroup);
						FillGroupViewData();
					}
				});

				// 分组的资源标签
				m_GroupAssetTagsTxt = root.Q<TextField>("GroupAssetTags");
				m_GroupAssetTagsTxt.RegisterValueChangedCallback(evt =>
				{
					if (m_PackageListView.selectedItem is AssetBundleCollectorPackage selectPackage
					 && m_GroupListView.selectedItem is AssetBundleCollectorGroup selectGroup)
					{
						selectGroup.AssetTags = evt.newValue;
						AssetCollectModule.ModifyGroup(selectPackage, selectGroup);
					}
				});

				// 收集列表容器
				m_CollectorContainer = root.Q("CollectorContainer");

				// 收集列表相关
				m_CollectorScrollView = root.Q<ScrollView>("CollectorScrollView");
				m_CollectorScrollView.style.height = new Length(100, LengthUnit.Percent);
				m_CollectorScrollView.viewDataKey = "scrollView";

				// 收集器创建按钮
				VisualElement collectorAddContainer = root.Q("CollectorAddContainer");
				{
					Button addBtn = collectorAddContainer.Q<Button>("AddBtn");
					addBtn.clicked += AddCollectorButtonClicked;
				}

				// 分组激活规则
				VisualElement activeRuleContainer = root.Q("ActiveRuleContainer");
				{
					m_ActiveRulePopupField = new("Active Rule", m_ActiveRuleList, 0)
					{
						name = "ActiveRuleMaskField",
						style =
						{
							unityTextAlign = TextAnchor.MiddleLeft
						},
						formatListItemCallback = FormatListItemCallback,
						formatSelectedValueCallback = FormatSelectedValueCallback
					};
					m_ActiveRulePopupField.RegisterValueChangedCallback(evt =>
					{
						if (m_PackageListView.selectedItem is AssetBundleCollectorPackage selectPackage
						 && m_GroupListView.selectedItem is AssetBundleCollectorGroup selectGroup)
						{
							selectGroup.ActiveRuleName = evt.newValue.ClassName;
							AssetCollectModule.ModifyGroup(selectPackage, selectGroup);
							FillGroupViewData();
						}
					});
					activeRuleContainer.Add(m_ActiveRulePopupField);
				}

				// 刷新窗体
				RefreshWindow();
			}
			catch (System.Exception e)
			{
				EditorLog.Error(e.ToString());
			}
		}

		public void OnDestroy()
		{
			// 注意：清空所有撤销操作
			Undo.ClearAll();

			AssetCollectModule.Setting.Save();
		}

		void RefreshWindow()
		{
			m_ShowPackageToogle.SetValueWithoutNotify(AssetCollectModule.Setting.ShowPackageView);
			m_EnableAddressableToogle.SetValueWithoutNotify(AssetCollectModule.Setting.EnableAddressable);
			m_UniqueBundleNameToogle.SetValueWithoutNotify(AssetCollectModule.Setting.UniqueBundleName);
			m_ShowEditorAliasToggle.SetValueWithoutNotify(AssetCollectModule.Setting.ShowEditorAlias);

			m_GroupContainer.visible = false;
			m_CollectorContainer.visible = false;

			FillPackageViewData();
		}
		void FixBtn_clicked()
		{
			AssetCollectModule.FixFile();
			RefreshWindow();
		}
		static void ExportButtonClicked()
		{
			string directory = WindowModule.OpenFolderPanel("Export XML", "Assets/");
			if (!string.IsNullOrEmpty(directory))
			{
				string path = FileSystem.ToXmlPath(directory, nameof(AssetBundleCollectorSetting));
				AssetCollectModule.ExportToXml(path);
			}
		}

		void ImportButtonClicked()
		{
			string resultPath = WindowModule.OpenFilePath("Import XML", "Assets/", "xml");
			if (resultPath != null)
			{
				AssetCollectModule.ImportFromXml(resultPath);
				RefreshWindow();
			}
		}

		static void SaveButtonClicked()
		{
			AssetCollectModule.Setting.Save();
		}

		string FormatListItemCallback(RuleDisplayName ruleDisplayName)
		{
			return m_ShowEditorAliasToggle.value ? ruleDisplayName.DisplayName : ruleDisplayName.ClassName;
		}

		string FormatSelectedValueCallback(RuleDisplayName ruleDisplayName)
		{
			return m_ShowEditorAliasToggle.value ? ruleDisplayName.DisplayName : ruleDisplayName.ClassName;
		}

		// 包裹列表相关
		void FillPackageViewData()
		{
			m_PackageListView.Clear();
			m_PackageListView.ClearSelection();
			m_PackageListView.itemsSource = AssetCollectModule.Setting.Packages;
			m_PackageListView.Rebuild();

			if (m_LastModifyPackageIndex >= 0 && m_LastModifyPackageIndex < m_PackageListView.itemsSource.Count)
			{
				m_PackageListView.selectedIndex = m_LastModifyPackageIndex;
			}

			m_PackageContainer.style.display = m_ShowPackageToogle.value ? DisplayStyle.Flex : DisplayStyle.None;
		}

		static VisualElement MakePackageListViewItem()
		{
			VisualElement element = new();
			Label label = new()
			{
				name = "Label1",
				style =
				{
					unityTextAlign = TextAnchor.MiddleLeft,
					flexGrow = 1f,
					height = 20f
				}
			};
			element.Add(label);
			return element;
		}

		static void BindPackageListViewItem(VisualElement element, int index)
		{
			AssetBundleCollectorPackage package = AssetCollectModule.Setting.Packages[index];
			Label textField1 = element.Q<Label>("Label1");
			textField1.text = string.IsNullOrEmpty(package.PackageDesc) ? package.PackageName : $"{package.PackageName} ({package.PackageDesc})";
		}

		void PackageListView_onSelectionChange(IEnumerable<object> objs)
		{
			if (m_PackageListView.selectedItem is not AssetBundleCollectorPackage selectPackage)
			{
				m_GroupContainer.visible = false;
				m_CollectorContainer.visible = false;
				return;
			}

			m_GroupContainer.visible = true;
			m_LastModifyPackageIndex = m_PackageListView.selectedIndex;
			m_PackageNameTxt.SetValueWithoutNotify(selectPackage.PackageName);
			m_PackageDescTxt.SetValueWithoutNotify(selectPackage.PackageDesc);
			FillGroupViewData();
		}
		void AddPackageButtonClicked()
		{
			Undo.RecordObject(AssetCollectModule.Setting, "Universe.Editor.AssetBundleCollectorWindow AddPackage");
			AssetCollectModule.CreatePackage("DefaultPackage");
			FillPackageViewData();
		}

		void RemovePackageButtonClicked()
		{
			if (m_PackageListView.selectedItem is not AssetBundleCollectorPackage selectPackage)
				return;

			Undo.RecordObject(AssetCollectModule.Setting, "Universe.Editor.AssetBundleCollectorWindow RemovePackage");
			AssetCollectModule.RemovePackage(selectPackage);
			FillPackageViewData();
		}

		// 分组列表相关
		void FillGroupViewData()
		{
			if (m_PackageListView.selectedItem is not AssetBundleCollectorPackage selectPackage)
			{
				return;
			}

			m_GroupListView.Clear();
			m_GroupListView.ClearSelection();
			m_GroupListView.itemsSource = selectPackage.Groups;
			m_GroupListView.Rebuild();

			if (m_LastModifyGroupIndex >= 0 && m_LastModifyGroupIndex < m_GroupListView.itemsSource.Count)
			{
				m_GroupListView.selectedIndex = m_LastModifyGroupIndex;
			}
		}

		static VisualElement MakeGroupListViewItem()
		{
			VisualElement element = new();
			Label label = new()
			{
				name = "Label1",
				style =
				{
					unityTextAlign = TextAnchor.MiddleLeft,
					flexGrow = 1f,
					height = 20f
				}
			};
			element.Add(label);
			return element;
		}

		void BindGroupListViewItem(VisualElement element, int index)
		{
			if (m_PackageListView.selectedItem is not AssetBundleCollectorPackage selectPackage)
			{
				return;
			}

			AssetBundleCollectorGroup group = selectPackage.Groups[index];

			Label textField1 = element.Q<Label>("Label1");
			textField1.text = string.IsNullOrEmpty(group.GroupDesc) ? group.GroupName : $"{group.GroupName} ({group.GroupDesc})";

			// 激活状态
			IActiveRule activeRule = AssetCollectModule.GetRuleInstance<IActiveRule>(group.ActiveRuleName);
			bool isActive = activeRule.IsActive();
			textField1.SetEnabled(isActive);
		}

		void GroupListView_onSelectionChange(IEnumerable<object> objs)
		{
			if (m_GroupListView.selectedItem is not AssetBundleCollectorGroup selectGroup)
			{
				m_CollectorContainer.visible = false;
				return;
			}

			m_CollectorContainer.visible = true;
			m_LastModifyGroupIndex = m_GroupListView.selectedIndex;
			m_ActiveRulePopupField.SetValueWithoutNotify(AssetCollectModule.GetRuleName<IActiveRule>(selectGroup.ActiveRuleName));
			m_GroupNameTxt.SetValueWithoutNotify(selectGroup.GroupName);
			m_GroupDescTxt.SetValueWithoutNotify(selectGroup.GroupDesc);
			m_GroupAssetTagsTxt.SetValueWithoutNotify(selectGroup.AssetTags);

			FillCollectorViewData();
		}

		void AddGroupButtonClicked()
		{
			if (m_PackageListView.selectedItem is not AssetBundleCollectorPackage selectPackage)
			{
				return;
			}

			Undo.RecordObject(AssetCollectModule.Setting, "Universe.Editor.AssetBundleCollectorWindow AddGroup");
			AssetCollectModule.CreateGroup(selectPackage, "Default Group");
			FillGroupViewData();
		}

		void RemoveGroupButtonClicked()
		{
			if (m_PackageListView.selectedItem is not AssetBundleCollectorPackage selectPackage)
			{
				return;
			}

			if (m_GroupListView.selectedItem is not AssetBundleCollectorGroup selectGroup)
			{
				return;
			}

			Undo.RecordObject(AssetCollectModule.Setting, "Universe.Editor.AssetBundleCollectorWindow RemoveGroup");
			AssetCollectModule.RemoveGroup(selectPackage, selectGroup);
			FillGroupViewData();
		}

		// 收集列表相关
		void FillCollectorViewData()
		{
			if (m_GroupListView.selectedItem is not AssetBundleCollectorGroup selectGroup)
			{
				return;
			}

			// 填充数据
			m_CollectorScrollView.Clear();
			for (int i = 0; i < selectGroup.Collectors.Count; i++)
			{
				VisualElement element = MakeCollectorListViewItem();
				BindCollectorListViewItem(element, i);
				m_CollectorScrollView.Add(element);
			}
		}

		VisualElement MakeCollectorListViewItem()
		{
			VisualElement element = new();
			VisualElement elementTop = new()
			{
				style =
				{
					flexDirection = FlexDirection.Row
				}
			};
			element.Add(elementTop);

			VisualElement elementBottom = new()
			{
				style =
				{
					flexDirection = FlexDirection.Row
				}
			};
			element.Add(elementBottom);

			VisualElement elementFoldout = new()
			{
				style =
				{
					flexDirection = FlexDirection.Row
				}
			};
			element.Add(elementFoldout);

			VisualElement elementSpace = new()
			{
				style =
				{
					flexDirection = FlexDirection.Column
				}
			};
			element.Add(elementSpace);

			// Top VisualElement
			{
				Button button = new()
				{
					name = "Button1",
					text = "-",
					style =
					{
						unityTextAlign = TextAnchor.MiddleCenter,
						flexGrow = 0f
					}
				};
				elementTop.Add(button);
			}
			{
				ObjectField objectField = new()
				{
					name = "ObjectField1",
					label = "Collector",
					objectType = typeof(Object),
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						flexGrow = 1f
					}
				};
				elementTop.Add(objectField);
				Label label = objectField.Q<Label>();
				label.style.minWidth = 63;
			}

			// Bottom VisualElement
			{
				Label label = new()
				{
					style =
					{
						width = 90
					}
				};
				elementBottom.Add(label);
			}
			{
				PopupField<string> popupField = new(m_CollectorTypeList, 0)
				{
					name = "PopupField0",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						width = 150
					}
				};
				elementBottom.Add(popupField);
			}
			if (m_EnableAddressableToogle.value)
			{
				PopupField<RuleDisplayName> popupField = new(m_AddressRuleList, 0)
				{
					name = "PopupField1",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						width = 220
					}
				};
				elementBottom.Add(popupField);
			}
			{
				PopupField<RuleDisplayName> popupField = new(m_PackRuleList, 0)
				{
					name = "PopupField2",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						width = 220
					}
				};
				elementBottom.Add(popupField);
			}
			{
				PopupField<RuleDisplayName> popupField = new(m_FilterRuleList, 0)
				{
					name = "PopupField3",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						width = 150
					}
				};
				elementBottom.Add(popupField);
			}
			{
				TextField textField = new()
				{
					name = "TextField0",
					label = "UserData",
					style =
					{
						width = 200
					}
				};
				elementBottom.Add(textField);
				Label label = textField.Q<Label>();
				label.style.minWidth = 63;
			}
			{
				TextField textField = new()
				{
					name = "TextField1",
					label = "Tags",
					style =
					{
						width = 100,
						marginLeft = 20,
						flexGrow = 1
					}
				};
				elementBottom.Add(textField);
				Label label = textField.Q<Label>();
				label.style.minWidth = 40;
			}

			// Foldout VisualElement
			{
				Label label = new()
				{
					style =
					{
						width = 90
					}
				};
				elementFoldout.Add(label);
			}
			{
				Foldout foldout = new()
				{
					name = "Foldout1",
					value = false,
					text = "Main Assets"
				};
				elementFoldout.Add(foldout);
			}

			// Space VisualElement
			{
				Label label = new()
				{
					style =
					{
						height = 10
					}
				};
				elementSpace.Add(label);
			}

			return element;
		}

		void BindCollectorListViewItem(VisualElement element, int index)
		{
			if (m_GroupListView.selectedItem is not AssetBundleCollectorGroup selectGroup)
			{
				return;
			}

			AssetBundleCollector collector = selectGroup.Collectors[index];
			Object collectObject = AssetDatabase.LoadAssetAtPath<Object>(collector.CollectPath);
			if (collectObject != null)
			{
				collectObject.name = collector.CollectPath;
			}

			// Foldout
			Foldout foldout = element.Q<Foldout>("Foldout1");
			foldout.RegisterValueChangedCallback(evt =>
			{
				if (evt.newValue)
				{
					RefreshFoldout(foldout, selectGroup, collector);
				}
				else
				{
					foldout.Clear();
				}
			});

			// Remove Button
			Button removeBtn = element.Q<Button>("Button1");
			removeBtn.clicked += () =>
			{
				RemoveCollectorButtonClicked(collector);
			};

			// Collector Path
			ObjectField objectField1 = element.Q<ObjectField>("ObjectField1");
			objectField1.SetValueWithoutNotify(collectObject);
			objectField1.RegisterValueChangedCallback(evt =>
			{
				collector.CollectPath = AssetDatabase.GetAssetPath(evt.newValue);
				collector.CollectorGuid = AssetDatabase.AssetPathToGUID(collector.CollectPath);
				objectField1.value.name = collector.CollectPath;
				AssetCollectModule.ModifyCollector(selectGroup, collector);
				if (foldout.value)
				{
					RefreshFoldout(foldout, selectGroup, collector);
				}
			});

			// Collector Type
			PopupField<string> popupField0 = element.Q<PopupField<string>>("PopupField0");
			popupField0.index = EnumUtilities.GetIndex(collector.CollectorType);
			popupField0.RegisterValueChangedCallback(evt =>
			{
				collector.CollectorType = EnumUtilities.NameToEnum<ECollectorType>(evt.newValue);
				AssetCollectModule.ModifyCollector(selectGroup, collector);
				if (foldout.value)
				{
					RefreshFoldout(foldout, selectGroup, collector);
				}
			});

			// Address Rule
			PopupField<RuleDisplayName> popupField1 = element.Q<PopupField<RuleDisplayName>>("PopupField1");
			if (popupField1 != null)
			{
				popupField1.index = AssetCollectModule.GetRuleIndex<IAddressRule>(collector.AddressRuleName);
				popupField1.formatListItemCallback = FormatListItemCallback;
				popupField1.formatSelectedValueCallback = FormatSelectedValueCallback;
				popupField1.RegisterValueChangedCallback(evt =>
				{
					collector.AddressRuleName = evt.newValue.ClassName;
					AssetCollectModule.ModifyCollector(selectGroup, collector);
					if (foldout.value)
					{
						RefreshFoldout(foldout, selectGroup, collector);
					}
				});
			}

			// Pack Rule
			PopupField<RuleDisplayName> popupField2 = element.Q<PopupField<RuleDisplayName>>("PopupField2");
			popupField2.index = AssetCollectModule.GetRuleIndex<IPackRule>(collector.PackRuleName);
			popupField2.formatListItemCallback = FormatListItemCallback;
			popupField2.formatSelectedValueCallback = FormatSelectedValueCallback;
			popupField2.RegisterValueChangedCallback(evt =>
			{
				collector.PackRuleName = evt.newValue.ClassName;
				AssetCollectModule.ModifyCollector(selectGroup, collector);
				if (foldout.value)
				{
					RefreshFoldout(foldout, selectGroup, collector);
				}
			});

			// Filter Rule
			PopupField<RuleDisplayName> popupField3 = element.Q<PopupField<RuleDisplayName>>("PopupField3");
			popupField3.index = AssetCollectModule.GetRuleIndex<IFilterRule>(collector.FilterRuleName);
			popupField3.formatListItemCallback = FormatListItemCallback;
			popupField3.formatSelectedValueCallback = FormatSelectedValueCallback;
			popupField3.RegisterValueChangedCallback(evt =>
			{
				collector.FilterRuleName = evt.newValue.ClassName;
				AssetCollectModule.ModifyCollector(selectGroup, collector);
				if (foldout.value)
				{
					RefreshFoldout(foldout, selectGroup, collector);
				}
			});

			// UserData
			TextField textFiled0 = element.Q<TextField>("TextField0");
			textFiled0.SetValueWithoutNotify(collector.UserData);
			textFiled0.RegisterValueChangedCallback(evt =>
			{
				collector.UserData = evt.newValue;
				AssetCollectModule.ModifyCollector(selectGroup, collector);
			});

			// Tags
			TextField textFiled1 = element.Q<TextField>("TextField1");
			textFiled1.SetValueWithoutNotify(collector.AssetTags);
			textFiled1.RegisterValueChangedCallback(evt =>
			{
				collector.AssetTags = evt.newValue;
				AssetCollectModule.ModifyCollector(selectGroup, collector);
			});
		}

		void RefreshFoldout(VisualElement foldout, AssetBundleCollectorGroup group, AssetBundleCollector collector)
		{
			// 清空旧元素
			foldout.Clear();

			if (collector.IsValid() == false)
			{
				Debug.LogWarning($"The collector is invalid : {collector.CollectPath} in group : {group.GroupName}");
				return;
			}

			if (collector.CollectorType == ECollectorType.MainAssetCollector || collector.CollectorType == ECollectorType.StaticAssetCollector)
			{
				List<CollectAssetInfo> collectAssetInfos = null;

				try
				{
					CollectCommand command = new(EBuildMode.SimulateBuild, m_PackageNameTxt.value, m_EnableAddressableToogle.value, m_UniqueBundleNameToogle.value);
					collectAssetInfos = collector.GetAllCollectAssets(command, group);
				}
				catch (System.Exception e)
				{
					Debug.LogError(e.ToString());
				}

				if (collectAssetInfos != null)
				{
					foreach (CollectAssetInfo collectAssetInfo in collectAssetInfos)
					{
						VisualElement elementRow = new()
						{
							style =
							{
								flexDirection = FlexDirection.Row
							}
						};
						foldout.Add(elementRow);

						string showInfo = collectAssetInfo.AssetPath;
						if (m_EnableAddressableToogle.value)
							showInfo = $"[{collectAssetInfo.Address}] {collectAssetInfo.AssetPath}";

						Label label = new()
						{
							text = showInfo,
							style =
							{
								width = 300,
								marginLeft = 0,
								flexGrow = 1
							}
						};
						elementRow.Add(label);
					}
				}
			}
		}

		void AddCollectorButtonClicked()
		{
			if (m_GroupListView.selectedItem is not AssetBundleCollectorGroup selectGroup)
				return;

			Undo.RecordObject(AssetCollectModule.Setting, "Universe.Editor.AssetBundleCollectorWindow AddCollector");
			AssetBundleCollector collector = new();
			AssetCollectModule.CreateCollector(selectGroup, collector);
			FillCollectorViewData();
		}

		void RemoveCollectorButtonClicked(AssetBundleCollector selectCollector)
		{
			if (m_GroupListView.selectedItem is not AssetBundleCollectorGroup selectGroup)
				return;
			if (selectCollector == null)
				return;

			Undo.RecordObject(AssetCollectModule.Setting, "Universe.Editor.AssetBundleCollectorWindow RemoveCollector");
			AssetCollectModule.RemoveCollector(selectGroup, selectCollector);
			FillCollectorViewData();
		}
	}
}
