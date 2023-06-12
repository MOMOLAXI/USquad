#if UNITY_2019_4_OR_NEWER
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace UniverseEngine.Editor
{
	internal class DebuggerBundleListViewer
	{
		VisualTreeAsset m_VisualAsset;
		TemplateContainer m_Root;

		ListView m_BundleListView;
		ListView m_UsingListView;
		DebugReport m_DebugReport;

		/// <summary>
		/// 初始化页面
		/// </summary>
		public void InitViewer()
		{
			// 加载布局文件
			m_VisualAsset = WindowModule.LoadWindowUxml<DebuggerBundleListViewer>();
			if (m_VisualAsset == null)
				return;

			m_Root = m_VisualAsset.CloneTree();
			m_Root.style.flexGrow = 1f;

			// 资源包列表
			m_BundleListView = m_Root.Q<ListView>("TopListView");
			m_BundleListView.makeItem = MakeBundleListViewItem;
			m_BundleListView.bindItem = BindBundleListViewItem;
			m_BundleListView.selectionChanged += BundleListView_onSelectionChange;

			// 使用列表
			m_UsingListView = m_Root.Q<ListView>("BottomListView");
			m_UsingListView.makeItem = MakeIncludeListViewItem;
			m_UsingListView.bindItem = BindIncludeListViewItem;

			SplitView.Adjuster(m_Root);
		}

		/// <summary>
		/// 清空页面
		/// </summary>
		public void ClearView()
		{
			m_DebugReport = null;
			m_BundleListView.Clear();
			m_BundleListView.ClearSelection();
			m_BundleListView.itemsSource.Clear();
			m_BundleListView.Rebuild();
		}

		/// <summary>
		/// 填充页面数据
		/// </summary>
		public void FillViewData(DebugReport debugReport, string searchKeyWord)
		{
			m_DebugReport = debugReport;
			m_BundleListView.Clear();
			m_BundleListView.ClearSelection();
			m_BundleListView.itemsSource = FilterViewItems(debugReport, searchKeyWord);
			m_BundleListView.Rebuild();
		}

		static List<DebugBundleInfo> FilterViewItems(DebugReport debugReport, string searchKeyWord)
		{
			List<DebugBundleInfo> result = new(1000);
			foreach (DebugPackageData pakcageData in debugReport.PackageDatas)
			{
				Dictionary<string, DebugBundleInfo> tempDic = new(1000);
				foreach (DebugProviderInfo providerInfo in pakcageData.ProviderInfos)
				{
					foreach (DebugBundleInfo bundleInfo in providerInfo.DependBundleInfos)
					{
						if (string.IsNullOrEmpty(searchKeyWord) == false)
						{
							if (bundleInfo.BundleName.Contains(searchKeyWord) == false)
								continue;
						}

						if (tempDic.ContainsKey(bundleInfo.BundleName) == false)
						{
							bundleInfo.PackageName = pakcageData.PackageName;
							tempDic.Add(bundleInfo.BundleName, bundleInfo);
						}
					}
				}

				List<DebugBundleInfo> tempList = tempDic.Values.ToList();
				tempList.Sort();
				result.AddRange(tempList);
			}
			return result;
		}

		/// <summary>
		/// 挂接到父类页面上
		/// </summary>
		public void AttachParent(VisualElement parent)
		{
			parent.Add(m_Root);
		}

		/// <summary>
		/// 从父类页面脱离开
		/// </summary>
		public void DetachParent()
		{
			m_Root.RemoveFromHierarchy();
		}

		// 顶部列表相关
		static VisualElement MakeBundleListViewItem()
		{
			VisualElement element = new()
			{
				style =
				{
					flexDirection = FlexDirection.Row
				}
			};

			{
				Label label = new()
				{
					name = "Label0",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						marginLeft = 3f,
						//label.style.flexGrow = 1f;
						width = 150
					}
				};
				element.Add(label);
			}

			{
				Label label = new()
				{
					name = "Label1",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						marginLeft = 3f,
						flexGrow = 1f,
						width = 280
					}
				};
				element.Add(label);
			}

			{
				Label label = new()
				{
					name = "Label3",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						marginLeft = 3f,
						//label.style.flexGrow = 1f;
						width = 100
					}
				};
				element.Add(label);
			}

			{
				Label label = new()
				{
					name = "Label4",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						marginLeft = 3f,
						//label.style.flexGrow = 1f;
						width = 120
					}
				};
				element.Add(label);
			}

			return element;
		}
		void BindBundleListViewItem(VisualElement element, int index)
		{
			List<DebugBundleInfo> sourceData = m_BundleListView.itemsSource as List<DebugBundleInfo>;
			DebugBundleInfo bundleInfo = sourceData[index];

			// Package Name
			Label label0 = element.Q<Label>("Label0");
			label0.text = bundleInfo.PackageName;

			// Bundle Name
			Label label1 = element.Q<Label>("Label1");
			label1.text = bundleInfo.BundleName;

			// Ref Count
			Label label3 = element.Q<Label>("Label3");
			label3.text = bundleInfo.RefCount.ToString();

			// Status
			StyleColor textColor = bundleInfo.Status == BundleLoaderBase.EStatus.Failed.ToString() ? new(Color.yellow) : label1.style.color;
			Label label4 = element.Q<Label>("Label4");
			label4.text = bundleInfo.Status;
			label4.style.color = textColor;
		}

		void BundleListView_onSelectionChange(IEnumerable<object> objs)
		{
			foreach (object item in objs)
			{
				DebugBundleInfo bundleInfo = item as DebugBundleInfo;
				FillUsingListView(bundleInfo);
			}
		}

		// 底部列表相关
		static VisualElement MakeIncludeListViewItem()
		{
			VisualElement element = new()
			{
				style =
				{
					flexDirection = FlexDirection.Row
				}
			};

			{
				Label label = new()
				{
					name = "Label1",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						marginLeft = 3f,
						flexGrow = 1f,
						width = 280
					}
				};
				element.Add(label);
			}

			{
				Label label = new()
				{
					name = "Label2",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						marginLeft = 3f,
						//label.style.flexGrow = 1f;
						width = 150
					}
				};
				element.Add(label);
			}

			{
				Label label = new()
				{
					name = "Label3",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						marginLeft = 3f,
						//label.style.flexGrow = 1f;
						width = 150
					}
				};
				element.Add(label);
			}

			{
				Label label = new()
				{
					name = "Label4",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						marginLeft = 3f,
						//label.style.flexGrow = 1f;
						width = 100
					}
				};
				element.Add(label);
			}

			{
				Label label = new()
				{
					name = "Label5",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						marginLeft = 3f,
						//label.style.flexGrow = 1f;
						width = 120
					}
				};
				element.Add(label);
			}

			return element;
		}

		void BindIncludeListViewItem(VisualElement element, int index)
		{
			List<DebugProviderInfo> providers = m_UsingListView.itemsSource as List<DebugProviderInfo>;
			DebugProviderInfo providerInfo = providers[index];

			// Asset Path
			Label label1 = element.Q<Label>("Label1");
			label1.text = providerInfo.AssetPath;

			// Spawn Scene
			Label label2 = element.Q<Label>("Label2");
			label2.text = providerInfo.SpawnScene;

			// Spawn Time
			Label label3 = element.Q<Label>("Label3");
			label3.text = providerInfo.SpawnTime;

			// Ref Count
			Label label4 = element.Q<Label>("Label4");
			label4.text = providerInfo.RefCount.ToString();

			// Status
			Label label5 = element.Q<Label>("Label5");
			label5.text = providerInfo.Status;
		}
		void FillUsingListView(DebugBundleInfo selectedBundleInfo)
		{
			List<DebugProviderInfo> source = new();
			foreach (DebugPackageData packageData in m_DebugReport.PackageDatas)
			{
				if (packageData.PackageName != selectedBundleInfo.PackageName)
				{
					continue;
				}
				
				for (int i = 0; i < packageData.ProviderInfos.Count; i++)
				{
					DebugProviderInfo providerInfo = packageData.ProviderInfos[i];
					for (int index = 0; index < providerInfo.DependBundleInfos.Count; index++)
					{
						DebugBundleInfo bundleInfo = providerInfo.DependBundleInfos[index];
						if (bundleInfo.BundleName == selectedBundleInfo.BundleName)
						{
							source.Add(providerInfo);
						}
					}
				}
			}

			m_UsingListView.Clear();
			m_UsingListView.ClearSelection();
			m_UsingListView.itemsSource = source;
			m_UsingListView.Rebuild();
		}
	}
}
#endif
