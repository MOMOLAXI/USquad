using UnityEngine.UIElements;

namespace UniverseEngine.Editor
{
	/// <summary>
	/// 分屏控件
	/// </summary>
	public class SplitView : TwoPaneSplitView
	{
		public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits> { }

		/// <summary>
		/// 窗口分屏适配
		/// </summary>
		public static void Adjuster(VisualElement root)
		{
			VisualElement topGroup = root.Q<VisualElement>("TopGroup");
			VisualElement bottomGroup = root.Q<VisualElement>("BottomGroup");
			topGroup.style.minHeight = 100f;
			bottomGroup.style.minHeight = 100f;
			root.Remove(topGroup);
			root.Remove(bottomGroup);
			SplitView spliteView = new()
			{
				fixedPaneInitialDimension = 300,
				orientation = TwoPaneSplitViewOrientation.Vertical
			};
			spliteView.contentContainer.Add(topGroup);
			spliteView.contentContainer.Add(bottomGroup);
			root.Add(spliteView);
		}
	}
}
