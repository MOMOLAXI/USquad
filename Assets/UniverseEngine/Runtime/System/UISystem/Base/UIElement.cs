using UnityEngine;
using UnityEngine.EventSystems;

namespace UniverseEngine
{
    public class UIElement : UIBehaviour
    {
        public Run OnCreateDelegate { get; private set; }
        public Run<Args> OnOpenDelegate { get; private set; }
        public Run OnCloseDelegate { get; private set; }
        public Run OnDisposeDelegate { get; private set; }
        public Run<bool> OnVisibleChangedDelegate { get; private set; }

        internal UIMeta Meta { get; set; }
        public RectTransform Rect => GetComponent<RectTransform>();

        protected override void Awake()
        {
            OnCreateDelegate = OnCreate;
            OnOpenDelegate = OnOpen;
            OnCloseDelegate = OnClose;
            OnDisposeDelegate = OnDispose;
            OnVisibleChangedDelegate = OnVisibleChanged;
        }

        internal virtual void Initialize(int order)
        {
        }

        /// <summary>
        /// UI创建时调用
        /// </summary>
        protected virtual void OnCreate()
        {
        }

        /// <summary>
        /// UI释放时调用
        /// </summary>
        protected virtual void OnDispose()
        {
        }

        /// <summary>
        /// UI打开时调用
        /// </summary>
        protected virtual void OnOpen(Args args = default(Args))
        {
        }

        /// <summary>
        /// UI关闭时调用，该UI可能被放入缓存，并不释放
        /// </summary>
        protected virtual void OnClose()
        {
        }

        /// <summary>
        /// 可见状态改变
        /// </summary>
        protected virtual void OnVisibleChanged(bool isVisible)
        {
        }
    }
}
