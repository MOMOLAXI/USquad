#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor.IMGUI.Controls;
using System.Text;
using System.Text.RegularExpressions;

namespace UniverseEngine.Editor
{
    public class UniTaskTrackerViewItem : TreeViewItem
    {
        static readonly Regex s_RemoveHref = new("<a href.+>(.+)</a>", RegexOptions.Compiled);

        public string TaskType { get; set; }
        public string Elapsed { get; set; }
        public string Status { get; set; }

        string m_Position;
        public string Position
        {
            get => m_Position;
            set
            {
                m_Position = value;
                PositionFirstLine = GetFirstLine(m_Position);
            }
        }

        public string PositionFirstLine { get; private set; }

        static string GetFirstLine(string str)
        {
            StringBuilder sb = new();
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '\r' || str[i] == '\n')
                {
                    break;
                }
                sb.Append(str[i]);
            }

            return s_RemoveHref.Replace(sb.ToString(), "$1");
        }

        public UniTaskTrackerViewItem(int id) : base(id)
        {

        }
    }

    public class UniTaskTrackerTreeView : TreeView
    {
        const string SORTED_COLUMN_INDEX_STATE_KEY = "UniTaskTrackerTreeView_sortedColumnIndex";

        public IReadOnlyList<TreeViewItem> CurrentBindingItems;

        public UniTaskTrackerTreeView() : this(new(), new(new(new[]
        {
            new MultiColumnHeaderState.Column { headerContent = new("TaskType"), width = 20 },
            new MultiColumnHeaderState.Column { headerContent = new("Elapsed"), width = 10 },
            new MultiColumnHeaderState.Column { headerContent = new("Status"), width = 10 },
            new MultiColumnHeaderState.Column { headerContent = new("Position") },
        })))
        {
        }

        UniTaskTrackerTreeView(TreeViewState state, MultiColumnHeader header) : base(state, header)
        {
            rowHeight = 20;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            header.sortingChanged += Header_sortingChanged;

            header.ResizeToFit();
            Reload();

            header.sortedColumnIndex = SessionState.GetInt(SORTED_COLUMN_INDEX_STATE_KEY, 1);
        }

        public void ReloadAndSort()
        {
            List<int> currentSelected = state.selectedIDs;
            Reload();
            Header_sortingChanged(multiColumnHeader);
            state.selectedIDs = currentSelected;
        }

        private void Header_sortingChanged(MultiColumnHeader multiColumnHeader)
        {
            SessionState.SetInt(SORTED_COLUMN_INDEX_STATE_KEY, multiColumnHeader.sortedColumnIndex);
            int index = multiColumnHeader.sortedColumnIndex;
            bool ascending = multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex);

            IEnumerable<UniTaskTrackerViewItem> items = rootItem.children.Cast<UniTaskTrackerViewItem>();

            IOrderedEnumerable<UniTaskTrackerViewItem> orderedEnumerable = index switch
            {
                0 => ascending ? items.OrderBy(item => item.TaskType) : items.OrderByDescending(item => item.TaskType),
                1 => ascending ? items.OrderBy(item => double.Parse(item.Elapsed)) : items.OrderByDescending(item => double.Parse(item.Elapsed)),
                2 => ascending ? items.OrderBy(item => item.Status) : items.OrderByDescending(item => item.Elapsed),
                3 => ascending ? items.OrderBy(item => item.Position) : items.OrderByDescending(item => item.PositionFirstLine),
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
            };

            CurrentBindingItems = rootItem.children = orderedEnumerable.Cast<TreeViewItem>().ToList();
            BuildRows(rootItem);
        }

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new() { depth = -1 };

            List<TreeViewItem> children = new();

            TaskTracker.ForEachActiveTask((trackingId, awaiterType, status, created, stackTrace) =>
            {
                children.Add(new UniTaskTrackerViewItem(trackingId)
                {
                    TaskType = awaiterType,
                    Status = status.ToString(),
                    Elapsed = (DateTime.UtcNow - created).TotalSeconds.ToString("00.00"),
                    Position = stackTrace
                });
            });

            CurrentBindingItems = children;
            root.children = CurrentBindingItems as List<TreeViewItem>;
            return root;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            UniTaskTrackerViewItem item = args.item as UniTaskTrackerViewItem;

            for (int visibleColumnIndex = 0; visibleColumnIndex < args.GetNumVisibleColumns(); visibleColumnIndex++)
            {
                Rect rect = args.GetCellRect(visibleColumnIndex);
                int columnIndex = args.GetColumn(visibleColumnIndex);

                GUIStyle labelStyle = args.selected ? EditorStyles.whiteLabel : EditorStyles.label;
                labelStyle.alignment = TextAnchor.MiddleLeft;
                switch (columnIndex)
                {
                    case 0:
                    {
                        EditorGUI.LabelField(rect, item.TaskType, labelStyle);
                        break;
                    }
                    case 1:
                    {
                        EditorGUI.LabelField(rect, item.Elapsed, labelStyle);
                        break;
                    }
                    case 2:
                    {
                        EditorGUI.LabelField(rect, item.Status, labelStyle);
                        break;
                    }
                    case 3:
                    {
                        EditorGUI.LabelField(rect, item.PositionFirstLine, labelStyle);
                        break;
                    }
                    default:
                    {
                        throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
                    }
                }
            }
        }
    }
}
