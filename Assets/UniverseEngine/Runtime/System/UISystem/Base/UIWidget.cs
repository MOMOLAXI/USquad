using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UniverseEngine
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class UIWidget : UIElement
    {
        [SerializeField]
        List<Canvas> m_CanvasList = new();

        [SerializeField]
        List<UISubWidget> m_SubWidgets = new();

        Canvas m_Canvas;
        CanvasGroup m_CanvasGroup;

        public CanvasGroup TargetCanvasGroup
        {
            get
            {
                if (m_CanvasGroup == null)
                {
                    m_CanvasGroup = GetComponent<CanvasGroup>();
                }

                return m_CanvasGroup;
            }
        }

        public Canvas TargetCanvas
        {
            get
            {
                if (m_Canvas == null)
                {
                    m_Canvas = GetComponent<Canvas>();
                }

                return m_Canvas;
            }
        }

        public IReadOnlyList<UISubWidget> SubWidgets => m_SubWidgets;

        public IReadOnlyList<Canvas> GetSubCanvas() => m_CanvasList;

        public static void AttachSubWidget<T>(T id, Transform root, Run<UIElement> onOpen = null, Args args = default(Args)) where T : struct
        {
            switch (id)
            {
                case Enum int32ID:
                {
                    UISystem.OpenSubWidget(Convert.ToInt32(int32ID), root, onOpen, args);
                    return;
                }
                default:
                {
                    Log.Error("Widget id type must be enum, because managed widgets were singleton");
                    return;
                }
            }
        }

        public static void CloseSubWidget<T>(T id, bool destroy = false)
        {
            switch (id)
            {
                case Enum int32ID:
                {
                    UISystem.Close(Convert.ToInt32(int32ID), destroy);
                    return;
                }
                default:
                {
                    Log.Error("Widget id type must be enum, because managed widgets were singleton");
                    return;
                }
            }
        }

        internal override void Initialize(int order)
        {
            TargetCanvas.overrideSorting = true;
            TargetCanvas.sortingOrder = order;
            TargetCanvasGroup.blocksRaycasts = true;
            TargetCanvasGroup.interactable = true;
        }

        protected void CloseSelf()
        {
            UISystem.Close(Meta.ID);
        }

        protected override void OnCreate()
        {
            //刷新子页面
            m_SubWidgets.Clear();
            GetComponentsInChildren(true, m_SubWidgets);

            //刷新canvas
            m_CanvasList.Clear();
            GetComponentsInChildren(true, m_CanvasList);
        }

        protected override void OnClose()
        {
            for (int i = 0; i < SubWidgets.Count; i++)
            {
                UISubWidget subWidget = SubWidgets[i];
                Function.Run(subWidget.OnCloseDelegate);
            }
        }

        protected override void OnDispose()
        {
            for (int i = 0; i < SubWidgets.Count; i++)
            {
                UISubWidget subWidget = SubWidgets[i];
                Function.Run(subWidget.OnDisposeDelegate);
            }
        }

        protected override void OnVisibleChanged(bool isVisible)
        {
            for (int i = 0; i < SubWidgets.Count; i++)
            {
                UISubWidget subWidget = SubWidgets[i];
                Function.Run(subWidget.OnVisibleChangedDelegate, isVisible);
            }
        }
    }
}
