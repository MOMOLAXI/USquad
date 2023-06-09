using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UniverseEngine.Editor
{
	internal class ReporterBundleListViewer
	{
		enum ESortMode
		{
			BundleName,
			BundleSize,
			BundleTags
		}

		VisualTreeAsset m_VisualAsset;
		TemplateContainer m_Root;

		ToolbarButton m_TopBar1;
		ToolbarButton m_TopBar2;
		ToolbarButton m_TopBar3;
		ToolbarButton m_TopBar5;
		ToolbarButton m_BottomBar1;
		ListView m_BundleListView;
		ListView m_IncludeListView;

		BuildReport m_BuildReport;
		string m_ReportFilePath;
		string m_SearchKeyWord;
		ESortMode m_SortMode = ESortMode.BundleName;
		bool m_DescendingSort;

		/// <summary>
		/// 初始化页面
		/// </summary>
		public void InitViewer()
		{
			// 加载布局文件
			m_VisualAsset = WindowModule.LoadWindowUxml<ReporterBundleListViewer>();
			if (m_VisualAsset == null)
				return;

			m_Root = m_VisualAsset.CloneTree();
			m_Root.style.flexGrow = 1f;

			// 顶部按钮栏
			m_TopBar1 = m_Root.Q<ToolbarButton>("TopBar1");
			m_TopBar2 = m_Root.Q<ToolbarButton>("TopBar2");
			m_TopBar3 = m_Root.Q<ToolbarButton>("TopBar3");
			m_TopBar5 = m_Root.Q<ToolbarButton>("TopBar5");
			m_TopBar1.clicked += TopBar1_clicked;
			m_TopBar2.clicked += TopBar2_clicked;
			m_TopBar3.clicked += TopBar3_clicked;
			m_TopBar5.clicked += TopBar4_clicked;

			// 底部按钮栏
			m_BottomBar1 = m_Root.Q<ToolbarButton>("BottomBar1");

			// 资源包列表
			m_BundleListView = m_Root.Q<ListView>("TopListView");
			m_BundleListView.makeItem = MakeBundleListViewItem;
			m_BundleListView.bindItem = BindBundleListViewItem;
			m_BundleListView.selectionChanged += BundleListView_onSelectionChange;

			// 包含列表
			m_IncludeListView = m_Root.Q<ListView>("BottomListView");
			m_IncludeListView.makeItem = MakeIncludeListViewItem;
			m_IncludeListView.bindItem = BindIncludeListViewItem;

			SplitView.Adjuster(m_Root);
		}

		/// <summary>
		/// 填充页面数据
		/// </summary>
		public void FillViewData(BuildReport buildReport, string reprotFilePath, string searchKeyWord)
		{
			m_BuildReport = buildReport;
			m_ReportFilePath = reprotFilePath;
			m_SearchKeyWord = searchKeyWord;
			RefreshView();
		}
		void RefreshView()
		{
			m_BundleListView.Clear();
			m_BundleListView.ClearSelection();
			m_BundleListView.itemsSource = FilterAndSortViewItems();
			m_BundleListView.Rebuild();
			RefreshSortingSymbol();
		}
		List<ReportBundleInfo> FilterAndSortViewItems()
		{
			List<ReportBundleInfo> result = new(m_BuildReport.BundleInfos.Count);

			// 过滤列表
			foreach (ReportBundleInfo bundleInfo in m_BuildReport.BundleInfos)
			{
				if (string.IsNullOrEmpty(m_SearchKeyWord) == false)
				{
					if (bundleInfo.BundleName.Contains(m_SearchKeyWord) == false)
						continue;
				}
				result.Add(bundleInfo);
			}

			return m_SortMode switch
			{
				// 排序列表
				ESortMode.BundleName when m_DescendingSort => result.OrderByDescending(a => a.BundleName).ToList(),
				ESortMode.BundleName => result.OrderBy(a => a.BundleName).ToList(),
				ESortMode.BundleSize when m_DescendingSort => result.OrderByDescending(a => a.FileSize).ToList(),
				ESortMode.BundleSize => result.OrderBy(a => a.FileSize).ToList(),
				ESortMode.BundleTags when m_DescendingSort => result.OrderByDescending(a => a.GetTagsString()).ToList(),
				ESortMode.BundleTags => result.OrderBy(a => a.GetTagsString()).ToList(),
				_ => throw new NotImplementedException()
			};
		}

		void RefreshSortingSymbol()
		{
			// 刷新符号
			m_TopBar1.text = $"Bundle Name ({m_BundleListView.itemsSource.Count})";
			m_TopBar2.text = "Size";
			m_TopBar3.text = "Hash";
			m_TopBar5.text = "Tags";

			switch (m_SortMode)
			{
				case ESortMode.BundleName when m_DescendingSort:
				{
					m_TopBar1.text = $"Bundle Name ({m_BundleListView.itemsSource.Count}) ↓";
					break;
				}
				case ESortMode.BundleName:
				{
					m_TopBar1.text = $"Bundle Name ({m_BundleListView.itemsSource.Count}) ↑";
					break;
				}
				case ESortMode.BundleSize when m_DescendingSort:
				{
					m_TopBar2.text = "Size ↓";
					break;
				}
				case ESortMode.BundleSize:
				{
					m_TopBar2.text = "Size ↑";
					break;
				}
				case ESortMode.BundleTags when m_DescendingSort:
				{
					m_TopBar5.text = "Tags ↓";
					break;
				}
				case ESortMode.BundleTags:
				{
					m_TopBar5.text = "Tags ↑";
					break;
				}
				default:
				{
					throw new NotImplementedException();
				}
			}
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
						width = 100
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
						width = 280
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
						width = 150
					}
				};
				element.Add(label);
			}

			{
				Label label = new()
				{
					name = "Label6",
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						marginLeft = 3f,
						flexGrow = 1f,
						width = 80
					}
				};
				element.Add(label);
			}

			return element;
		}
		void BindBundleListViewItem(VisualElement element, int index)
		{
			List<ReportBundleInfo> sourceData = m_BundleListView.itemsSource as List<ReportBundleInfo>;
			ReportBundleInfo bundleInfo = sourceData[index];

			// Bundle Name
			Label label1 = element.Q<Label>("Label1");
			label1.text = bundleInfo.BundleName;

			// Size
			Label label2 = element.Q<Label>("Label2");
			label2.text = EditorUtility.FormatBytes(bundleInfo.FileSize);

			// Hash
			Label label3 = element.Q<Label>("Label3");
			label3.text = bundleInfo.FileHash;

			// LoadMethod
			Label label5 = element.Q<Label>("Label5");
			label5.text = bundleInfo.LoadMethod.ToString();

			// Tags
			Label label6 = element.Q<Label>("Label6");
			label6.text = bundleInfo.GetTagsString();
		}
		void BundleListView_onSelectionChange(IEnumerable<object> objs)
		{
			foreach (object item in objs)
			{
				ReportBundleInfo bundleInfo = item as ReportBundleInfo;
				FillIncludeListView(bundleInfo);
				ShowAssetBundleInspector(bundleInfo);
				break;
			}
		}
		void ShowAssetBundleInspector(ReportBundleInfo bundleInfo)
		{
			if (bundleInfo.IsRawFile)
				return;

			string rootDirectory = Path.GetDirectoryName(m_ReportFilePath);
			string filePath = $"{rootDirectory}/{bundleInfo.FileName}";
			Selection.activeObject = File.Exists(filePath) ? AssetBundleRecorder.GetAssetBundle(filePath) : null;
		}

		void TopBar1_clicked()
		{
			if (m_SortMode != ESortMode.BundleName)
			{
				m_SortMode = ESortMode.BundleName;
				m_DescendingSort = false;
				RefreshView();
			}
			else
			{
				m_DescendingSort = !m_DescendingSort;
				RefreshView();
			}
		}

		void TopBar2_clicked()
		{
			if (m_SortMode != ESortMode.BundleSize)
			{
				m_SortMode = ESortMode.BundleSize;
				m_DescendingSort = false;
				RefreshView();
			}
			else
			{
				m_DescendingSort = !m_DescendingSort;
				RefreshView();
			}
		}

		void TopBar3_clicked() { }

		void TopBar4_clicked()
		{
			if (m_SortMode != ESortMode.BundleTags)
			{
				m_SortMode = ESortMode.BundleTags;
				m_DescendingSort = false;
				RefreshView();
			}
			else
			{
				m_DescendingSort = !m_DescendingSort;
				RefreshView();
			}
		}

		// 底部列表相关
		void FillIncludeListView(ReportBundleInfo bundleInfo)
		{
			List<ReportAssetInfo> containsList = m_BuildReport.AssetInfos.Where(assetInfo => assetInfo.MainBundleName == bundleInfo.BundleName).ToList();
			m_IncludeListView.Clear();
			m_IncludeListView.ClearSelection();
			m_IncludeListView.itemsSource = containsList;
			m_IncludeListView.Rebuild();
			m_BottomBar1.text = $"Include Assets ({containsList.Count})";
		}

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
						width = 280
					}
				};
				element.Add(label);
			}

			return element;
		}
		void BindIncludeListViewItem(VisualElement element, int index)
		{
			List<ReportAssetInfo> containsList = m_IncludeListView.itemsSource as List<ReportAssetInfo>;
			ReportAssetInfo assetInfo = containsList[index];

			// Asset Path
			Label label1 = element.Q<Label>("Label1");
			label1.text = assetInfo.AssetPath;

			// GUID
			Label label2 = element.Q<Label>("Label2");
			label2.text = assetInfo.AssetGuid;
		}
	}
}
