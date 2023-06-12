#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Collections.Generic;

namespace UniverseEngine.Editor
{
    public class UniTaskTrackerWindow : UniverseEditorWindow
    {
        static int s_Interval;
        static readonly GUILayoutOption[] s_EmptyLayoutOption = Array.Empty<GUILayoutOption>();

        UniTaskTrackerTreeView m_TreeView;
        object m_SplitterState;

        void OnEnable()
        {
            m_SplitterState = SplitterGUILayout.CreateSplitterState(new[] { 75f, 25f }, new[] { 32, 32 }, null);
            m_TreeView = new();
            TaskTracker.EditorEnableState.EnableAutoReload = EditorPrefs.GetBool(TaskTracker.EnableAutoReloadKey, false);
            TaskTracker.EditorEnableState.EnableTracking = EditorPrefs.GetBool(TaskTracker.EnableTrackingKey, false);
            TaskTracker.EditorEnableState.EnableStackTrace = EditorPrefs.GetBool(TaskTracker.EnableStackTraceKey, false);
        }

        void OnGUI()
        {
            // Head
            RenderHeadPanel();

            // Splittable
            SplitterGUILayout.BeginVerticalSplit(m_SplitterState, s_EmptyLayoutOption);
            {
                // Column Tabble
                RenderTable();

                // StackTrace details
                RenderDetailsPanel();
            }
            SplitterGUILayout.EndVerticalSplit();
        }

    #region HeadPanel

        public static bool EnableAutoReload => TaskTracker.EditorEnableState.EnableAutoReload;
        public static bool EnableTracking => TaskTracker.EditorEnableState.EnableTracking;
        public static bool EnableStackTrace => TaskTracker.EditorEnableState.EnableStackTrace;
        static readonly GUIContent s_EnableAutoReloadHeadContent = EditorGUIUtility.TrTextContent("Enable AutoReload", "Reload automatically.");
        static readonly GUIContent s_ReloadHeadContent = EditorGUIUtility.TrTextContent("Reload", "Reload View.");
        static readonly GUIContent s_GCHeadContent = EditorGUIUtility.TrTextContent("GC.Collect", "Invoke GC.Collect.");
        static readonly GUIContent s_EnableTrackingHeadContent = EditorGUIUtility.TrTextContent("Enable Tracking", "Start to track async/await UniTask. Performance impact: low");
        static readonly GUIContent s_EnableStackTraceHeadContent = EditorGUIUtility.TrTextContent("Enable StackTrace", "Capture StackTrace when task is started. Performance impact: high");

        // [Enable Tracking] | [Enable StackTrace]
        void RenderHeadPanel()
        {
            EditorGUILayout.BeginVertical(s_EmptyLayoutOption);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, s_EmptyLayoutOption);

            if (GUILayout.Toggle(EnableAutoReload, s_EnableAutoReloadHeadContent, EditorStyles.toolbarButton, s_EmptyLayoutOption) != EnableAutoReload)
            {
                TaskTracker.EditorEnableState.EnableAutoReload = !EnableAutoReload;
            }

            if (GUILayout.Toggle(EnableTracking, s_EnableTrackingHeadContent, EditorStyles.toolbarButton, s_EmptyLayoutOption) != EnableTracking)
            {
                TaskTracker.EditorEnableState.EnableTracking = !EnableTracking;
            }

            if (GUILayout.Toggle(EnableStackTrace, s_EnableStackTraceHeadContent, EditorStyles.toolbarButton, s_EmptyLayoutOption) != EnableStackTrace)
            {
                TaskTracker.EditorEnableState.EnableStackTrace = !EnableStackTrace;
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(s_ReloadHeadContent, EditorStyles.toolbarButton, s_EmptyLayoutOption))
            {
                TaskTracker.CheckAndResetDirty();
                m_TreeView.ReloadAndSort();
                Repaint();
            }

            if (GUILayout.Button(s_GCHeadContent, EditorStyles.toolbarButton, s_EmptyLayoutOption))
            {
                GC.Collect(0);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

    #endregion

    #region TableColumn

        Vector2 m_TableScroll;
        GUIStyle m_TableListStyle;

        void RenderTable()
        {
            m_TableListStyle ??= new("CN Box")
            {
                margin =
                {
                    top = 0
                },
                padding =
                {
                    left = 3
                }
            };

            EditorGUILayout.BeginVertical(m_TableListStyle, s_EmptyLayoutOption);

            m_TableScroll = EditorGUILayout.BeginScrollView(m_TableScroll, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(2000f));
            Rect controlRect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));


            m_TreeView?.OnGUI(controlRect);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void Update()
        {
            if (EnableAutoReload)
            {
                if (s_Interval++ % 120 == 0)
                {
                    if (TaskTracker.CheckAndResetDirty())
                    {
                        m_TreeView.ReloadAndSort();
                        Repaint();
                    }
                }
            }
        }

    #endregion

    #region Details

        static GUIStyle s_DetailsStyle;
        Vector2 m_DetailsScroll;

        void RenderDetailsPanel()
        {
            s_DetailsStyle ??= new("CN Message")
            {
                wordWrap = false,
                stretchHeight = true,
                margin =
                {
                    right = 15
                }
            };

            string message = "";
            List<int> selected = m_TreeView.state.selectedIDs;
            if (selected.Count > 0)
            {
                int first = selected[0];
                if (m_TreeView.CurrentBindingItems.FirstOrDefault(x => x.id == first) is UniTaskTrackerViewItem item)
                {
                    message = item.Position;
                }
            }

            m_DetailsScroll = EditorGUILayout.BeginScrollView(m_DetailsScroll, s_EmptyLayoutOption);
            Vector2 vector = s_DetailsStyle.CalcSize(new(message));
            EditorGUILayout.SelectableLabel(message, s_DetailsStyle, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true), GUILayout.MinWidth(vector.x), GUILayout.MinHeight(vector.y));
            EditorGUILayout.EndScrollView();
        }

    #endregion
    }
}
